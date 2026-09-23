using MassTransit;
using Microsoft.Extensions.Logging;

namespace Verum.CrossCutting.Mensageria;

public abstract class ConsumidorMensagem<TMensagem>(ILogger logger) : IConsumer<TMensagem>
  where TMensagem : class
{
  public async Task Consume(ConsumeContext<TMensagem> context)
  {
    using var scope = logger.BeginScope(new Dictionary<string, object?>
    {
      ["MessageId"] = context.MessageId,
      ["CorrelationId"] = context.CorrelationId,
      ["MessageType"] = typeof(TMensagem).Name
    });

    await ProcessarAsync(context);
  }

  protected abstract Task ProcessarAsync(ConsumeContext<TMensagem> context);
}

