using MassTransit;
using Verum.CrossCutting.Mensageria;
using Verum.Modules.Radar.Contratos;

namespace Verum.Worker.Radar.Consumidores;

public sealed class VerificacaoRadarSolicitadaConsumer(ILogger<VerificacaoRadarSolicitadaConsumer> logger)
  : ConsumidorMensagem<VerificacaoRadarSolicitada>(logger)
{
  protected override Task ProcessarAsync(ConsumeContext<VerificacaoRadarSolicitada> context)
  {
    context.CancellationToken.ThrowIfCancellationRequested();

    if (context.Message.MonitoramentoId == Guid.Empty)
      throw new ArgumentException("MonitoramentoId deve ser informado.");

    // TODO: injetar o serviço/fachada de Radar no construtor e substituir o throw:
    // return processamento.VerificarAsync(context.Message.MonitoramentoId, context.CancellationToken);
    // Aguardar o processamento completo; não usar Task.Run ou fire-and-forget.
    // O serviço cuidará da idempotência, comparação de preço e persistência da oportunidade.
    throw new ProcessamentoNaoConfiguradoException(nameof(VerificacaoRadarSolicitadaConsumer));
  }
}

