using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.CrossCutting.Pipelines;

namespace Verum.CrossCutting;

public static class DependencyInjection
{
  public static IServiceCollection AddVerumApi(this IServiceCollection services,
    IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureBus = null) =>
    services.AddBase(configuration, "api", configureBus).AddVerumApiModules(configuration).AddVerumWeb(configuration);

  public static IServiceCollection AddVerumWorkerDescoberta(this IServiceCollection services,
    IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureBus = null) =>
    services.AddBase(configuration, "descoberta", configureBus).AddVerumDescobertaModules(configuration);

  public static IServiceCollection AddVerumWorkerRadar(this IServiceCollection services,
    IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureBus = null) =>
    services.AddBase(configuration, "radar", configureBus).AddVerumRadarModules(configuration);

  private static IServiceCollection AddBase(this IServiceCollection services,
    IConfiguration configuration, string host, Action<IBusRegistrationConfigurator>? configureBus) =>
    services.AddVerumHttpClients(configuration)
      .AddVerumInteligenciaArtificial(configuration)
      .AddVerumCache(configuration)
      .AddVerumObservabilidade(configuration, $"Verum.{host}")
      .AddVerumMensageria(configuration, host, configureBus);
}
