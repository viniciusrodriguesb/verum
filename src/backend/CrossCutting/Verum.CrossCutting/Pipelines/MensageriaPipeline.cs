using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Verum.CrossCutting.Mensageria;

namespace Verum.CrossCutting.Pipelines;

public static class MensageriaPipeline
{
  public static IServiceCollection AddVerumMensageria(this IServiceCollection services, IConfiguration configuration, string hostName, Action<IBusRegistrationConfigurator>? registrarConsumidores = null)
  {
    var section = configuration.GetSection(RabbitMqOptions.SectionName);

    var settings = section.Get<RabbitMqOptions>() ?? new();

    if (!settings.Enabled)
      return services;

    services.AddOptions<RabbitMqOptions>().Bind(section).ValidateDataAnnotations()
      .Validate(o => o.ConcurrentMessageLimit <= o.PrefetchCount,
        "ConcurrentMessageLimit deve ser menor ou igual a PrefetchCount.")
      .Validate(o => !o.UseDelayedRedelivery || o.RedeliveryIntervalsSeconds is { Length: > 0 }
        && o.RedeliveryIntervalsSeconds.All(s => s > 0 && s <= 86400),
        "Os intervalos de redelivery devem estar entre 1 e 86400 segundos.")
      .ValidateOnStart();

    services.Configure<MassTransitHostOptions>(o =>
    {
      o.WaitUntilStarted = true;
      o.StartTimeout = TimeSpan.FromSeconds(30);
      o.StopTimeout = TimeSpan.FromSeconds(30);
    });

    services.AddVerumPublishers();

    services.AddMassTransit(bus =>
    {

      bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter($"verum-{hostName}", false));

      registrarConsumidores?.Invoke(bus);

      bus.UsingRabbitMq((context, rabbit) =>
      {
        var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        rabbit.Host(options.Host, (ushort)options.Port, options.VirtualHost, host =>
        {
          host.Username(options.Username);
          host.Password(options.Password);
          if (options.UseSsl) host.UseSsl(ssl => { ssl.ServerName = options.Host; });
        });

        rabbit.PrefetchCount = (ushort)options.PrefetchCount;

        rabbit.UsePublishFilter(typeof(PublicacaoLogFilter<>), context);

        rabbit.ConfigureEndpoints(context);
      });
      bus.AddConfigureEndpointsCallback((context, _, endpoint) =>
        endpoint.ConfigureVerumConsumer(context, settings));
    });

    return services;
  }
}

