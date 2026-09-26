using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class PlaywrightPipeline
{
  public static IServiceCollection AddVerumPlaywright(this IServiceCollection services, IConfiguration configuration) =>
    Modules.Ofertas.DependencyInjection.AddOfertasDescoberta(services, configuration);
}
