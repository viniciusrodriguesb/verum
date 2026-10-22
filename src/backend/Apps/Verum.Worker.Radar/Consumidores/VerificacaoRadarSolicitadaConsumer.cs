using MassTransit;
using Verum.CrossCutting.Mensageria;
using Verum.Modules.Radar.Contratos;

namespace Verum.Worker.Radar.Consumidores;

public sealed class VerificacaoRadarSolicitadaConsumer(ILogger<VerificacaoRadarSolicitadaConsumer> logger, IProcessamentoRadarService processamento)
  : ConsumidorMensagem<VerificacaoRadarSolicitada>(logger)
{
  protected override Task ProcessarAsync(ConsumeContext<VerificacaoRadarSolicitada> context)
  {
    context.CancellationToken.ThrowIfCancellationRequested();

    if (context.Message.MonitoramentoId == Guid.Empty)
      throw new ArgumentException("MonitoramentoId deve ser informado.");

    return processamento.VerificarAsync(context.Message.MonitoramentoId, context.CancellationToken);
  }
}
