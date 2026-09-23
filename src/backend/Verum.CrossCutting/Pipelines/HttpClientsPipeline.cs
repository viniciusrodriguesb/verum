using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Verum.CrossCutting.Pipelines;

public static class HttpClientsPipeline
{
  public static IServiceCollection AddVerumHttpClients(this IServiceCollection services, IConfiguration configuration)
  {
    services.ConfigureHttpClientDefaults(http =>
    {
      http.ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan);

      http.AddStandardResilienceHandler().Configure(configuration.GetSection("Http:Resilience"))
                                         .Configure(options => options.Retry.DisableForUnsafeHttpMethods());
    });

    services.AddHttpClient("Verum");

    return services;
  }
}

