using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Verum.CrossCutting.Mensageria;

namespace Verum.CrossCutting.Pipelines;

public static class PublishersPipeline
{
  public static IServiceCollection AddVerumPublishers(this IServiceCollection services)
  {
    services.TryAddScoped(typeof(PublicadorMensagem<>));

    return services;
  }
}

