using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Busca.Dominio;
using Verum.Modules.Busca.Infraestrutura.Persistencia;
using BuscaEntidade = Verum.Modules.Busca.Dominio.Busca;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Busca.Contratos.DTOs;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal sealed class BuscaService(BuscaDbContext db, IOptions<BuscaOptions> options, IBuscaCache cache, ISendEndpointProvider? sender = null) : IBuscaService
{
  public async Task<BuscaAceita> SolicitarAsync(Guid visitanteId, SolicitarBusca pedido, CancellationToken cancellationToken)
  {
    if (sender is null)
      throw new ErroAplicacaoException("BUSCA_INDISPONIVEL", "O serviço de busca está indisponível.", TipoErro.Indisponivel);

    var consulta = Validacao.Texto(pedido.Consulta, nameof(pedido.Consulta), 500);

    var chave = Validacao.Texto(pedido.ChaveIdempotencia, nameof(pedido.ChaveIdempotencia), 100);

    Validacao.Identificador(visitanteId, nameof(visitanteId));

    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlInterpolatedAsync(
      $"SELECT pg_advisory_xact_lock(hashtextextended({visitanteId.ToString()}, 710003))", cancellationToken);

    var existente = await db.Buscas.SingleOrDefaultAsync(x => x.VisitanteId == visitanteId && x.ChaveIdempotencia == chave, cancellationToken);

    if (existente is not null)
    {
      if (existente.ConsultaOriginal != consulta)
        throw new ErroAplicacaoException("IDEMPOTENCIA_CONFLITO", "A chave já foi usada com outra consulta.", TipoErro.Conflito);

      return new(existente.Id, existente.Status.ToString());
    }

    var inicioDia = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

    if (await db.Buscas.CountAsync(x => x.VisitanteId == visitanteId && x.IniciadaEm >= inicioDia, cancellationToken) >= options.Value.LimiteVisitanteDiario)
      throw new ErroAplicacaoException("LIMITE_BUSCAS", "Limite diário de buscas atingido.", TipoErro.LimiteExcedido);

    var busca = new BuscaEntidade(chave, consulta, DateTimeOffset.UtcNow.AddSeconds(options.Value.PrazoSegundos), visitanteId: visitanteId);

    db.Buscas.Add(busca);

    var etapa = new BuscaEtapa(busca.Id, EtapaBusca.Recepcao);

    etapa.Finalizar(true);

    db.Etapas.Add(etapa);

    // Comando dirigido à fila durável: a primeira solicitação também aguarda um worker futuro.
    var endpoint = await sender.GetSendEndpoint(new Uri("queue:" + options.Value.FilaSolicitacoes));

    await endpoint.Send(new BuscaSolicitada(busca.Id), context => context.CorrelationId = busca.Id, cancellationToken);

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);

    return new(busca.Id, busca.Status.ToString());
  }

  public async Task<BuscaConsulta> ObterAsync(Guid visitanteId, Guid buscaId, CancellationToken cancellationToken)
  {
    var busca = await db.Buscas.AsNoTracking()
      .SingleOrDefaultAsync(x => x.Id == buscaId && x.VisitanteId == visitanteId, cancellationToken)
      ?? throw new ErroAplicacaoException("BUSCA_NAO_ENCONTRADA", "Busca não encontrada.", TipoErro.NaoEncontrado);

    // A propriedade é sempre autorizada no banco antes de acessar qualquer projeção.
    if (busca.ConcluidaEm.HasValue)
    {
      var armazenado = await cache.ObterResultadoAsync<BuscaConsulta>(buscaId, cancellationToken);

      if (armazenado is not null) return armazenado;
    }

    var snapshots = await db.OfertasResultado
                            .AsNoTracking()
                            .Where(x => x.Resultado.BuscaId == buscaId)
                            .OrderBy(x => x.Posicao)
                            .Select(x => x.OfertaSnapshot)
                            .ToArrayAsync(cancellationToken);

    var ofertas = snapshots.Select(x => JsonSerializer.Deserialize<OfertaResultado>(x)!).ToArray();

    var resultado = new BuscaConsulta(busca.Id, busca.Status.ToString(), busca.EtapaAtual.ToString(), busca.ResultadoParcial,
      busca.MotivoSemResultado?.ToString(), busca.CodigoErro, busca.QuantidadeFontesConsultadas,
      busca.QuantidadeFontesComSucesso, busca.QuantidadeFontesComFalha, ofertas);

    if (busca.ConcluidaEm.HasValue)
      await cache.ArmazenarResultadoAsync(buscaId, resultado,
        ofertas.Length > 0 ? ofertas.Min(x => x.ValidaAte) : DateTimeOffset.UtcNow.AddMinutes(1), cancellationToken);

    return resultado;
  }
}
