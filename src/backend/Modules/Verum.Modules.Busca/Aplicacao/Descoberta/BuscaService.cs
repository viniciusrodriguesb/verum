using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Busca.Dominio;
using Verum.Modules.Busca.Infraestrutura.Persistencia;
using BuscaEntidade = Verum.Modules.Busca.Dominio.Busca;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal sealed class BuscaService(BuscaDbContext db, IOptions<BuscaOptions> options, IBuscaCache cache,
  IAcessoBuscaService acesso, ISendEndpointProvider? sender = null) : IBuscaService
{
  public async Task<BuscaAceita> SolicitarAsync(AtorBusca ator, SolicitarBusca pedido, CancellationToken cancellationToken)
  {
    if (sender is null)
      throw new ErroAplicacaoException("BUSCA_INDISPONIVEL", "O serviço de busca está indisponível.", TipoErro.Indisponivel);

    var consulta = Validacao.Texto(pedido.Consulta, nameof(pedido.Consulta), 500);

    var chave = Validacao.Texto(pedido.ChaveIdempotencia, nameof(pedido.ChaveIdempotencia), 100);

    await acesso.PrepararAsync(ator, cancellationToken);

    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await db.Database.ExecuteSqlInterpolatedAsync(
      $"SELECT pg_advisory_xact_lock(hashtextextended({ator.Chave}, 710003))", cancellationToken);

    var existente = await db.Buscas.SingleOrDefaultAsync(x => x.ContaId == ator.ContaId
      && x.VisitanteId == ator.VisitanteId && x.ChaveIdempotencia == chave, cancellationToken);

    if (existente is not null)
    {
      if (existente.ConsultaOriginal != consulta)
        throw new ErroAplicacaoException("IDEMPOTENCIA_CONFLITO", "A chave já foi usada com outra consulta.", TipoErro.Conflito);

      return new(existente.Id, existente.Status.ToString());
    }

    var busca = new BuscaEntidade(chave, consulta, DateTimeOffset.UtcNow.AddSeconds(options.Value.PrazoSegundos),
      contaId: ator.ContaId, visitanteId: ator.VisitanteId);

    await acesso.RegistrarAsync(ator, busca.Id, db.Database.GetDbConnection(), transacao.GetDbTransaction(), cancellationToken);

    db.Buscas.Add(busca);

    var etapa = new BuscaEtapa(busca.Id, EtapaBusca.Recepcao);

    etapa.Finalizar(true);

    db.Etapas.Add(etapa);

    var endpoint = await sender.GetSendEndpoint(new Uri("queue:" + options.Value.FilaSolicitacoes));

    await endpoint.Send(new BuscaSolicitada(busca.Id), context => context.CorrelationId = busca.Id, cancellationToken);

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);

    return new(busca.Id, busca.Status.ToString());
  }

  public async Task<IReadOnlyList<BuscaHistorico>> ObterHistoricoAsync(AtorBusca ator, CancellationToken cancellationToken)
  {
    if (!ator.ContaId.HasValue)
      throw new ErroAplicacaoException("HISTORICO_REQUER_CONTA", "Entre para acessar o histórico da sua conta.", TipoErro.AcessoNegado);

    return await db.Buscas.AsNoTracking().Where(x => x.ContaId == ator.ContaId && x.VisitanteId == null)
      .OrderByDescending(x => x.IniciadaEm).ThenByDescending(x => x.Id).Take(50)
      .Select(x => new BuscaHistorico(x.Id, x.ConsultaOriginal, x.Status.ToString(), x.IniciadaEm, x.ConcluidaEm))
      .ToArrayAsync(cancellationToken);
  }

  public async Task<BuscaConsulta> ObterAsync(AtorBusca ator, Guid buscaId, CancellationToken cancellationToken)
  {
    var busca = await db.Buscas
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == buscaId && x.ContaId == ator.ContaId && x.VisitanteId == ator.VisitanteId, cancellationToken)
      ?? throw new ErroAplicacaoException("BUSCA_NAO_ENCONTRADA", "Busca não encontrada.", TipoErro.NaoEncontrado);

    if (busca.ConcluidaEm.HasValue)
    {
      var armazenado = await cache.ObterResultadoAsync<BuscaConsulta>(buscaId, cancellationToken);

      if (armazenado is not null) return Revalidar(armazenado, busca.ConsultaOriginal);
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

    return Revalidar(resultado, busca.ConsultaOriginal);
  }

  internal static BuscaConsulta Revalidar(BuscaConsulta resultado, string consulta)
  {
    var elegiveis = resultado.Ofertas.Where(x => NormalizacaoOferta.Relevancia(consulta, x.Nome) > 30m)
      .Select((x, indice) => x with { Posicao = indice + 1 }).ToArray();

    return elegiveis.Length == resultado.Ofertas.Count ? resultado : resultado with
    {
      Ofertas = elegiveis,

      Parcial = true,

      CodigoErro = "CORRESPONDENCIA_REVALIDADA"
    };
  }
}
