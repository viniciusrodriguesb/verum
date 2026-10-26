using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Verum.Modules.Assinaturas.Aplicacao;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Assinaturas.Dominio;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;

namespace Verum.Modules.Assinaturas.Infraestrutura.Processadores;

// Executa na API do monólito; SKIP LOCKED permite múltiplas instâncias sem duplicar processamento.
internal sealed class PagamentosWorker(IServiceScopeFactory scopes, IOptions<AssinaturasOptions> options,
  ILogger<PagamentosWorker> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var proximaReconciliacao = DateTimeOffset.MinValue;

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        if (options.Value.Habilitado)
        {
          if (DateTimeOffset.UtcNow >= proximaReconciliacao)
          {
            await EnfileirarReconciliacaoAsync(stoppingToken);

            proximaReconciliacao = DateTimeOffset.UtcNow.AddSeconds(options.Value.IntervaloReconciliacaoSegundos);
          }

          for (var i = 0; i < options.Value.EventosPorCiclo; i++)
          {
            await using var scope = scopes.CreateAsyncScope();

            if (!await scope.ServiceProvider.GetRequiredService<ProcessamentoPagamentos>().ProcessarProximoAsync(stoppingToken)) break;
          }
        }
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        return;
      }
      catch (Exception erro)
      {
        logger.LogError(erro, "Falha no processamento de pagamentos; o recebimento durável será retomado.");
      }

      await Task.Delay(TimeSpan.FromSeconds(options.Value.IntervaloProcessamentoSegundos), stoppingToken);
    }
  }

  private async Task EnfileirarReconciliacaoAsync(CancellationToken cancellationToken)
  {
    await using var scope = scopes.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>();

    var limite = DateTimeOffset.UtcNow.AddDays(-options.Value.JanelaReconciliacaoDias);

    var assinaturas = await db.Assinaturas
      .Where(x => x.Status != StatusAssinatura.Cancelada || x.CanceladaEm > limite || x.PeriodoTerminaEm > limite)
      .OrderBy(x => x.ReconciliacaoAgendadaEm ?? x.CriadaEm).Take(options.Value.LoteReconciliacao).ToListAsync(cancellationToken);

    var janela = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / options.Value.IntervaloReconciliacaoSegundos;

    foreach (var assinatura in assinaturas)
    {
      var chave = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"reconciliacao:{assinatura.Id}:{janela}")));

      if (await db.EventosGateway.AnyAsync(x => x.Gateway == assinatura.Gateway && x.IdentificadorExterno == chave, cancellationToken)) continue;

      // Falha permanente de uma assinatura não impede o rodízio das demais.
      assinatura.AgendarReconciliacao();

      db.EventosGateway.Add(new EventoGateway(assinatura.Gateway, chave, "reconciliacao",
        JsonSerializer.Serialize(new { recursoId = assinatura.Id.ToString() })));

      try
      {
        await db.SaveChangesAsync(cancellationToken);
      }
      catch (DbUpdateException erro) when (erro.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
      {
        db.ChangeTracker.Clear();
      }
    }
  }
}
