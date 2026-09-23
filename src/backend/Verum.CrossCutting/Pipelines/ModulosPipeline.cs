using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Contas;
using Verum.Modules.Catalogo;
using Verum.Modules.Ofertas;
using Verum.Modules.Busca;
using Verum.Modules.Radar;
using Verum.Modules.Notificacoes;
using Verum.Modules.Acesso;
using Verum.Modules.Assinaturas;

namespace Verum.CrossCutting.Pipelines;

public static class ModulosPipeline
{
  public static IServiceCollection AddVerumApiModules(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddContasPersistencia(configuration);

    services.AddCatalogoPersistencia(configuration);

    services.AddOfertasPersistencia(configuration);

    services.AddBuscaPersistencia(configuration);

    services.AddRadarPersistencia(configuration);

    services.AddNotificacoesPersistencia(configuration);

    services.AddAcessoPersistencia(configuration);

    services.AddAssinaturasPersistencia(configuration);

    return services.AddVerumServices(typeof(Verum.Modules.Contas.DependencyInjection).Assembly,
      typeof(Verum.Modules.Catalogo.DependencyInjection).Assembly,
      typeof(Verum.Modules.Ofertas.DependencyInjection).Assembly,
      typeof(Verum.Modules.Busca.DependencyInjection).Assembly,
      typeof(Verum.Modules.Radar.DependencyInjection).Assembly,
      typeof(Verum.Modules.Notificacoes.DependencyInjection).Assembly,
      typeof(Verum.Modules.Acesso.DependencyInjection).Assembly,
      typeof(Verum.Modules.Assinaturas.DependencyInjection).Assembly);
  }

  public static IServiceCollection AddVerumDescobertaModules(this IServiceCollection services, IConfiguration configuration) =>
    services.AddBuscaPersistencia(configuration).AddCatalogoPersistencia(configuration).AddOfertasPersistencia(configuration)
      .AddVerumServices(typeof(Verum.Modules.Busca.DependencyInjection).Assembly,
        typeof(Verum.Modules.Catalogo.DependencyInjection).Assembly, typeof(Verum.Modules.Ofertas.DependencyInjection).Assembly);

  public static IServiceCollection AddVerumRadarModules(this IServiceCollection services, IConfiguration configuration) =>
    services.AddRadarPersistencia(configuration).AddCatalogoPersistencia(configuration)
      .AddOfertasPersistencia(configuration).AddNotificacoesPersistencia(configuration)
      .AddVerumServices(typeof(Verum.Modules.Radar.DependencyInjection).Assembly,
        typeof(Verum.Modules.Catalogo.DependencyInjection).Assembly, typeof(Verum.Modules.Ofertas.DependencyInjection).Assembly,
        typeof(Verum.Modules.Notificacoes.DependencyInjection).Assembly);

}
