using MassTransit;
using Microsoft.Extensions.Options;

namespace Verum.CrossCutting.Mensageria;

public sealed class PublicadorMensagem<TMensagem>(IPublishEndpoint endpoint, IOptions<RabbitMqOptions> options) where TMensagem : class
{
  public async Task PublicarAsync(TMensagem mensagem, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(mensagem);

    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    deadline.CancelAfter(TimeSpan.FromSeconds(options.Value.PublishTimeoutSeconds));

    await endpoint.Publish(mensagem, deadline.Token);
  }
}

