using MassTransit;
using Verum.CrossCutting.Mensageria;
using Verum.Modules.Busca.Contratos;

namespace Verum.Worker.Descoberta.Consumidores;

public sealed class BuscaSolicitadaConsumer(ILogger<BuscaSolicitadaConsumer> logger, IProcessamentoBuscaService processamento) : ConsumidorMensagem<BuscaSolicitada>(logger)
{
  protected override Task ProcessarAsync(ConsumeContext<BuscaSolicitada> context)
  {
    context.CancellationToken.ThrowIfCancellationRequested();

    if (context.Message.BuscaId == Guid.Empty)
      throw new ArgumentException("BuscaId deve ser informado.");

    return processamento.ProcessarAsync(context.Message.BuscaId, context.CancellationToken);
  }
}
