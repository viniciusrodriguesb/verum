using MassTransit;
using Verum.Modules.Busca.Contratos;

namespace Verum.Worker.Descoberta.Consumidores;

public sealed class BuscaFalhouConsumer(IFalhaBuscaService falhas) : IConsumer<Fault<BuscaSolicitada>>
{
  public Task Consume(ConsumeContext<Fault<BuscaSolicitada>> context) =>
    falhas.RegistrarAsync(context.Message.Message.BuscaId, context.CancellationToken);
}
