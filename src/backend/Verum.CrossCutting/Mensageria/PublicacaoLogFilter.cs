using MassTransit;
using Microsoft.Extensions.Logging;

namespace Verum.CrossCutting.Mensageria;

public sealed class PublicacaoLogFilter<TMensagem>(ILogger<PublicacaoLogFilter<TMensagem>> logger) : IFilter<PublishContext<TMensagem>> where TMensagem : class
{
  public void Probe(ProbeContext context) => context.CreateFilterScope("verum-publicacao");

  public async Task Send(PublishContext<TMensagem> context, IPipe<PublishContext<TMensagem>> next)
  {
    await next.Send(context);

    logger.LogDebug("Mensagem {MessageType} publicada: {MessageId}", typeof(TMensagem).Name, context.MessageId);
  }

}

