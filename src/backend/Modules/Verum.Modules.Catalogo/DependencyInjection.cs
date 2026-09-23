using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Catalogo.Infraestrutura.Persistencia;

namespace Verum.Modules.Catalogo;

public static class DependencyInjection
{
  public static IServiceCollection AddCatalogoPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<CatalogoDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "catalogo"));
    });
}

