using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Assinaturas.Aplicacao.Gateways;
using Verum.Modules.Assinaturas.Aplicacao.Gateways.DTOs;
using Verum.Modules.Assinaturas.Dominio;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;

namespace Verum.Modules.Assinaturas.Aplicacao;

internal sealed class ProcessamentoPagamentos(AssinaturasDbContext db, SelecionadorGateway gateways,
  IConcessoesAssinaturaService concessoes, ILogger<ProcessamentoPagamentos> logger, IOptions<AssinaturasOptions> options)
{
  public async Task<bool> ProcessarProximoAsync(CancellationToken stoppingToken)
  {
    using var prazo = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

    prazo.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutProcessamentoSegundos));

    var cancellationToken = prazo.Token;

    long? eventoId = null;

    try
    {
      await using var transacao = await db.Database.BeginTransactionAsync(cancellationToken);

      var evento = await db.EventosGateway.FromSqlRaw("""
        SELECT * FROM assinaturas."EVENTO_GATEWAY"
        WHERE "STATUS" <> 2 AND "TENTATIVAS" < 10
          AND ("PROXIMA_TENTATIVA_EM" IS NULL OR "PROXIMA_TENTATIVA_EM" <= now())
        ORDER BY "ID" LIMIT 1 FOR UPDATE SKIP LOCKED
        """).SingleOrDefaultAsync(cancellationToken);

      if (evento is null) return false;

      eventoId = evento.Id;

      using var dados = JsonDocument.Parse(evento.Conteudo);

      var gateway = gateways.Obter(evento.Gateway.ToString());

      var recursoId = dados.RootElement.GetProperty("recursoId").GetString()!;

      var externa = evento.Tipo == "reconciliacao" ? null
        : await gateway.ResolverAssinaturaAsync(evento.Tipo, recursoId, cancellationToken);

      var assinatura = evento.Tipo == "reconciliacao"
        ? await db.Assinaturas.Include(x => x.Plano).SingleOrDefaultAsync(x => x.Id == Guid.Parse(recursoId), cancellationToken)
        : await LocalizarAsync(evento.Gateway, externa, gateway, cancellationToken);

      if (assinatura is not null)
      {
        await db.Database.ExecuteSqlInterpolatedAsync(
          $"SELECT pg_advisory_xact_lock(hashtextextended({assinatura.Id.ToString()}, 710012))", cancellationToken);

        // Recarrega após obter o lock: nenhuma decisão usa um snapshot anterior a outro consumidor.
        await db.Entry(assinatura).ReloadAsync(cancellationToken);

        await SincronizarAsync(assinatura, gateway, cancellationToken);
      }

      evento.Concluir();

      await db.SaveChangesAsync(cancellationToken);

      await transacao.CommitAsync(cancellationToken);

      return true;
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception erro)
    {
      db.ChangeTracker.Clear();

      if (eventoId.HasValue)
      {
        var evento = await db.EventosGateway.SingleAsync(x => x.Id == eventoId, stoppingToken);

        evento.RegistrarFalha();

        await db.SaveChangesAsync(stoppingToken);
      }

      logger.LogWarning(erro, "Falha ao confirmar pagamento. EventoGatewayId {EventoGatewayId}", eventoId);

      return false;
    }
  }

  private async Task<Assinatura?> LocalizarAsync(GatewayPagamento tipo, string? id, IGatewayAssinaturas gateway, CancellationToken cancellationToken)
  {
    if (id is null) return null;

    var remota = await gateway.ObterAsync(id, cancellationToken);

    return await db.Assinaturas.Include(x => x.Plano)
      .SingleOrDefaultAsync(x => x.Id == remota.Referencia && x.Gateway == tipo, cancellationToken);
  }

  private async Task SincronizarAsync(Assinatura assinatura, IGatewayAssinaturas gateway, CancellationToken cancellationToken)
  {
    var remota = assinatura.IdentificadorExterno.StartsWith("pendente:", StringComparison.Ordinal)
      ? await gateway.LocalizarAsync(assinatura.Id, cancellationToken)
      : await gateway.ObterAsync(assinatura.IdentificadorExterno, cancellationToken);

    if (remota is null) return;

    if (remota.Referencia != assinatura.Id || remota.Valor != assinatura.ValorContratado || remota.Moeda != assinatura.Moeda)
      throw new InvalidOperationException("Referência, valor ou moeda da assinatura divergentes.");

    assinatura.VincularGateway(remota.Id, remota.UrlCheckout);

    if (assinatura.CancelamentoSolicitadoEm.HasValue && remota.Status != "cancelled")
    {
      await gateway.CancelarAsync(remota.Id, cancellationToken);

      remota = await gateway.ObterAsync(remota.Id, cancellationToken);
    }

    var cobrancas = await gateway.ConsultarCobrancasAsync(remota.Id, cancellationToken);

    foreach (var cobranca in cobrancas)
      await AplicarCobrancaAsync(assinatura, cobranca, cancellationToken);

    await db.SaveChangesAsync(cancellationToken);

    var pagamentos = await db.Pagamentos.Where(x => x.AssinaturaId == assinatura.Id && x.Status == StatusPagamento.Aprovado
      && x.PeriodoFim > DateTimeOffset.UtcNow).OrderByDescending(x => x.PeriodoFim).ToListAsync(cancellationToken);

    var vigente = pagamentos.FirstOrDefault(x => x.PeriodoInicio <= DateTimeOffset.UtcNow);

    var cobrancaEmAtraso = await db.Pagamentos.AnyAsync(x => x.AssinaturaId == assinatura.Id
      && (x.Status == StatusPagamento.Recusado || x.Status == StatusPagamento.Aprovado), cancellationToken);

    assinatura.Sincronizar(remota.Status, vigente?.PeriodoInicio, vigente?.PeriodoFim, cobrancaEmAtraso);
  }

  private async Task AplicarCobrancaAsync(Assinatura assinatura, CobrancaGateway cobranca, CancellationToken cancellationToken)
  {
    if (cobranca.Referencia != assinatura.Id || cobranca.AssinaturaId != assinatura.IdentificadorExterno
      || cobranca.Valor != assinatura.ValorContratado || cobranca.Moeda != assinatura.Moeda
      || cobranca.PeriodoInicio < assinatura.CriadaEm.AddDays(-1) || cobranca.PeriodoInicio > DateTimeOffset.UtcNow.AddMonths(1))
      throw new InvalidOperationException("Cobrança incompatível com a contratação.");

    if (cobranca.Status == "approved" && (cobranca.PagaEm is null || cobranca.PagaEm > DateTimeOffset.UtcNow.AddMinutes(5)))
      throw new InvalidOperationException("Pagamento aprovado sem data válida.");

    var pagamento = await db.Pagamentos.SingleOrDefaultAsync(x => x.AssinaturaId == assinatura.Id
      && x.IdentificadorExterno == cobranca.Id, cancellationToken);

    if (pagamento is null)
    {
      pagamento = new Pagamento(assinatura.Id, cobranca.Id, cobranca.Valor, cobranca.Moeda);

      db.Pagamentos.Add(pagamento);
    }

    if (!pagamento.Sincronizar(cobranca.Status, cobranca.PagaEm, cobranca.PeriodoInicio, cobranca.AtualizadaEm)) return;

    await concessoes.AplicarAsync(assinatura.ContaId, pagamento.Id, assinatura.Plano.PacoteAcessoCodigo,
      pagamento.PeriodoInicio!.Value, pagamento.PeriodoFim!.Value, pagamento.Status == StatusPagamento.Aprovado,
      db.Database.GetDbConnection(), db.Database.CurrentTransaction!.GetDbTransaction(), cancellationToken);
  }
}
