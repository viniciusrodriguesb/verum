using MassTransit;
using Verum.CrossCutting.Mensageria;

namespace Verum.CrossCutting.Pipelines;

public static class ConsumersPipeline
{
  public static void ConfigureVerumConsumer(this IReceiveEndpointConfigurator endpoint, IRegistrationContext context, RabbitMqOptions options)
  {
    endpoint.ConcurrentMessageLimit = options.ConcurrentMessageLimit;

    if (options.UseDelayedRedelivery)
      endpoint.UseDelayedRedelivery(retry =>
      {
        retry.Ignore<ArgumentException>();

        retry.Ignore<ProcessamentoNaoConfiguradoException>();

        retry.Ignore<OperationCanceledException>();

        retry.Intervals(options.RedeliveryIntervalsSeconds.Select(seconds => TimeSpan.FromSeconds(seconds)).ToArray());
      });

    endpoint.UseMessageRetry(retry =>
    {
      retry.Ignore<ArgumentException>();

      retry.Ignore<ProcessamentoNaoConfiguradoException>();

      retry.Ignore<OperationCanceledException>();

      retry.Exponential(options.RetryCount, TimeSpan.FromMilliseconds(200),
        TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(200));
    });

    if (options.UseInMemoryOutbox) endpoint.UseInMemoryOutbox(context);
  }
}
