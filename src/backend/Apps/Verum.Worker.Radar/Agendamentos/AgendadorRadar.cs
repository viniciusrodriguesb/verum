using Verum.CrossCutting.Mensageria;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Radar.Contratos;

namespace Verum.Worker.Radar.Agendamentos;

public sealed class AgendadorRadar(IServiceScopeFactory scopes, ILogger<AgendadorRadar> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

    do
    {
      await ExecutarEtapaAsync("Agendamento", async provider =>
      {
        var radar = provider.GetRequiredService<IProcessamentoRadarService>();

        var publicador = provider.GetService<PublicadorMensagem<VerificacaoRadarSolicitada>>();

        if (publicador is not null)
          foreach (var id in await radar.PendentesAsync(stoppingToken))
            await publicador.PublicarAsync(new(id), stoppingToken);
      }, stoppingToken);

      await ExecutarEtapaAsync("Oportunidades", provider => provider.GetRequiredService<IProcessamentoRadarService>()
        .EncaminharAvisosAsync(stoppingToken), stoppingToken);

      await ExecutarEtapaAsync("Entregas", provider => provider.GetRequiredService<INotificacoesRadarService>()
        .EnviarPendentesAsync(stoppingToken), stoppingToken);
    } while (await timer.WaitForNextTickAsync(stoppingToken));
  }

  private async Task ExecutarEtapaAsync(string etapa, Func<IServiceProvider, Task> executar, CancellationToken cancellationToken)
  {
    try
    {
      await using var scope = scopes.CreateAsyncScope();

      await executar(scope.ServiceProvider);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception exception)
    {
      logger.LogError(exception, "Falha na etapa {Etapa} do Radar; trabalho persistido será retomado.", etapa);
    }
  }
}
