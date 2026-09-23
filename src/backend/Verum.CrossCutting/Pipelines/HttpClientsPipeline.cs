using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Registry;

namespace Verum.CrossCutting.Pipelines;

public static class HttpClientsPipeline
{
  public static IServiceCollection AddVerumHttpClients(this IServiceCollection services, IConfiguration configuration)
  {
    services.TryAddSingleton<ResiliencePipelineRegistry<string>>();

    services.ConfigureHttpClientDefaults(http =>
      http.ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan));

    // O builder concreto fornece o nome real, inclusive para clientes tipados.
    services.ConfigureAll<HttpClientFactoryOptions>(options => options.HttpMessageHandlerBuilderActions.Add(builder =>
    {
      var registry = builder.Services.GetRequiredService<ResiliencePipelineRegistry<string>>();

      var nome = builder.Name ?? string.Empty;

      var pipeline = registry.GetOrAddPipeline<HttpResponseMessage>(nome, pipeline => Configurar(pipeline, configuration, nome));

      builder.AdditionalHandlers.Add(new ResilienceHandler(pipeline));
    }));

    services.AddHttpClient("Verum");

    return services;
  }

  private static void Configurar(ResiliencePipelineBuilder<HttpResponseMessage> pipeline, IConfiguration configuration, string nome)
  {
    var options = new HttpStandardResilienceOptions();

    configuration.GetSection("Http:Resilience").Bind(options);

    var cliente = configuration.GetSection($"Http:Clients:{nome}");

    cliente.GetSection("Resilience").Bind(options);

    var retryEnabled = cliente.GetValue<bool?>("RetryEnabled")
      ?? configuration.GetValue("Http:RetryEnabled", true);

    var circuitEnabled = cliente.GetValue<bool?>("CircuitBreakerEnabled")
      ?? configuration.GetValue("Http:CircuitBreakerEnabled", true);

    if (options.TotalRequestTimeout.Timeout < options.AttemptTimeout.Timeout)
      throw new InvalidOperationException($"Http ({nome}): timeout total deve ser maior ou igual ao timeout da tentativa.");

    if (circuitEnabled && options.CircuitBreaker.SamplingDuration < options.AttemptTimeout.Timeout * 2)
      throw new InvalidOperationException($"Http ({nome}): janela do circuito deve ser pelo menos duas vezes o timeout da tentativa.");

    options.Retry.DisableForUnsafeHttpMethods();

    pipeline.AddRateLimiter(options.RateLimiter)
      .AddTimeout(options.TotalRequestTimeout);

    if (retryEnabled) pipeline.AddRetry(options.Retry);

    if (circuitEnabled) pipeline.AddCircuitBreaker(options.CircuitBreaker);

    pipeline.AddTimeout(options.AttemptTimeout);
  }
}
