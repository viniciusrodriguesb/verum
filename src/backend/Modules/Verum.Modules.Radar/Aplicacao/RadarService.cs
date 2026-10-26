using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Catalogo.Contratos;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Ofertas.Contratos.Radar;
using Verum.Modules.Radar.Contratos;
using Verum.Modules.Radar.Contratos.Configuracoes;
using Verum.Modules.Radar.Contratos.DTOs;
using Verum.Modules.Radar.Dominio;
using Verum.Modules.Radar.Infraestrutura.Persistencia;

namespace Verum.Modules.Radar.Aplicacao;

internal sealed class RadarService(RadarDbContext db, IAcessoRadarService acesso, IOptions<RadarOptions> options,
  IOfertaRadarService ofertas, ICatalogoDescobertaService catalogo, INotificacoesRadarService notificacoes) : IRadarService
{
  public async Task<CapacidadeRadar> CapacidadeAsync(Guid? contaId, CancellationToken cancellationToken)
  {
    var permitido = contaId.HasValue && await acesso.PermitidoAsync(contaId.Value, cancellationToken);

    var utilizado = contaId.HasValue ? await db.Monitoramentos.AsNoTracking()
      .CountAsync(x => x.ContaId == contaId && x.Status != StatusMonitoramento.Excluido, cancellationToken) : 0;

    return new(options.Value.Habilitado, permitido, options.Value.LimiteMonitoramentos, utilizado,
      options.Value.IntervaloVerificacaoMinutos, notificacoes.EmailDisponivel);
  }

  public async Task<IReadOnlyList<MonitoramentoConsulta>> ListarAsync(Guid contaId, CancellationToken cancellationToken)
  {
    var permitida = await acesso.PermitidoAsync(contaId, cancellationToken);

    var lista = await db.Monitoramentos.AsNoTracking().Where(x => x.ContaId == contaId && x.Status != StatusMonitoramento.Excluido)
      .OrderByDescending(x => x.CriadoEm).Take(200).ToArrayAsync(cancellationToken);

    var resultado = new List<MonitoramentoConsulta>();

    foreach (var item in lista)
      resultado.Add(await ConverterAsync(item, permitida, cancellationToken));

    return resultado;
  }

  public async Task<MonitoramentoConsulta> CriarAsync(Guid contaId, CriarMonitoramento pedido, CancellationToken cancellationToken)
  {
    Validacao.Texto(pedido.ChaveIdempotencia, nameof(pedido.ChaveIdempotencia), 100);

    var capacidade = await ExigirAsync(contaId, cancellationToken);

    var repetida = await db.Monitoramentos.AsNoTracking().SingleOrDefaultAsync(x => x.ContaId == contaId
      && x.ChaveIdempotencia == pedido.ChaveIdempotencia, cancellationToken);

    if (repetida is not null)
    {
      if (repetida.OfertaId != pedido.OfertaId || repetida.PrecoAlvo != pedido.PrecoAlvo)
        throw new ErroAplicacaoException("IDEMPOTENCIA_CONFLITO", "A chave já foi usada para outra meta.", TipoErro.Conflito);

      return await ConverterAsync(repetida, true, cancellationToken);
    }

    if (!pedido.IdentidadeConfirmada)
      throw ErroAplicacaoException.Validacao("Confirme a identidade do anúncio e suas condições.");

    var oferta = await ofertas.ObterAsync(pedido.OfertaId, cancellationToken);

    if (oferta is null || oferta.VarianteId != pedido.VarianteId || oferta.ValidaAte <= DateTimeOffset.UtcNow)
      throw ErroAplicacaoException.Validacao("Oferta inválida ou expirada. Faça uma nova busca para confirmar o produto.");

    var produto = await catalogo.ObterProdutoAsync(pedido.VarianteId, cancellationToken)
      ?? throw ErroAplicacaoException.Validacao("Variante não encontrada.");

    // Criar uma meta não altera preferências globais. O canal é configurado separadamente.
    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await BloquearContaAsync(contaId, cancellationToken);

    await ExigirAsync(contaId, cancellationToken);

    var existente = await db.Monitoramentos.SingleOrDefaultAsync(x => x.ContaId == contaId
      && x.ChaveIdempotencia == pedido.ChaveIdempotencia, cancellationToken);

    if (existente is not null)
    {
      if (existente.OfertaId != pedido.OfertaId || existente.PrecoAlvo != pedido.PrecoAlvo)
        throw new ErroAplicacaoException("IDEMPOTENCIA_CONFLITO", "A chave já foi usada para outra meta.", TipoErro.Conflito);

      return await ConverterAsync(existente, true, cancellationToken);
    }

    if (await db.Monitoramentos.CountAsync(x => x.ContaId == contaId && x.Status != StatusMonitoramento.Excluido, cancellationToken) >= capacidade.Limite)
      throw new ErroAplicacaoException("LIMITE_RADAR", "Limite de radares atingido. Encerre uma meta para liberar espaço.", TipoErro.LimiteExcedido);

    if (await db.Monitoramentos.AnyAsync(x => x.ContaId == contaId && x.OfertaId == pedido.OfertaId
      && x.Status != StatusMonitoramento.Excluido, cancellationToken))
      throw new ErroAplicacaoException("RADAR_EXISTENTE", "Você já acompanha este anúncio.", TipoErro.Conflito);

    var monitoramento = new Monitoramento(contaId, produto, oferta.Nome, oferta.Preco, pedido.PrecoAlvo, pedido.VarianteId);

    monitoramento.ConfigurarOrigem(oferta.Id, pedido.ChaveIdempotencia, oferta.Condicao);

    db.Monitoramentos.Add(monitoramento);

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);

    return await ConverterAsync(monitoramento, true, cancellationToken);
  }

  public async Task AlterarAsync(Guid contaId, Guid id, decimal precoAlvo, string acao, CancellationToken cancellationToken)
  {
    if (acao is "editar" or "retomar") await ExigirAsync(contaId, cancellationToken);

    await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

    await BloquearContaAsync(contaId, cancellationToken);

    if (acao is "editar" or "retomar") await ExigirAsync(contaId, cancellationToken);

    var item = await db.Monitoramentos.SingleOrDefaultAsync(x => x.Id == id && x.ContaId == contaId
      && x.Status != StatusMonitoramento.Excluido, cancellationToken)
      ?? throw new ErroAplicacaoException("RADAR_NAO_ENCONTRADO", "Radar não encontrado.", TipoErro.NaoEncontrado);

    item.Alterar(precoAlvo, acao);

    await db.SaveChangesAsync(cancellationToken);

    await transacao.CommitAsync(cancellationToken);
  }

  private Task<int> BloquearContaAsync(Guid contaId, CancellationToken cancellationToken) =>
    db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({contaId.ToString()}, 710020))", cancellationToken);

  private async Task<CapacidadeRadar> ExigirAsync(Guid contaId, CancellationToken cancellationToken)
  {
    var capacidade = await CapacidadeAsync(contaId, cancellationToken);

    if (!capacidade.Disponivel) throw new ErroAplicacaoException("RADAR_INDISPONIVEL", "O Radar não está disponível neste ambiente.", TipoErro.Indisponivel);

    if (!capacidade.Permitido) throw new ErroAplicacaoException("RADAR_SEM_DIREITO", "É necessário um direito válido ao Radar, incluído no Verum Plus.", TipoErro.AcessoNegado);

    return capacidade;
  }

  private async Task<MonitoramentoConsulta> ConverterAsync(Monitoramento item, bool permitido, CancellationToken cancellationToken)
  {
    var oferta = await ofertas.ObterAsync(item.OfertaId, cancellationToken);

    var estado = item.Status == StatusMonitoramento.Pausado ? "Pausado"
      : !permitido || !options.Value.Habilitado ? "SuspensoPorAcesso" : item.EstadoVerificacao;

    return new(item.Id, item.Nome, item.OfertaId, item.ProdutoVarianteId ?? Guid.Empty, item.PrecoInicial,
      item.PrecoAlvo, item.MenorPrecoAtual, estado, item.CriadoEm, item.UltimaVerificacaoEm,
      item.ProximaVerificacaoEm, oferta?.Url, oferta?.Fonte, item.Condicao, oferta?.ImagemUrl);
  }
}
