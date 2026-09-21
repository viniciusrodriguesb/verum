using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Ofertas.Infraestrutura.Persistencia;

namespace Verum.Modules.Ofertas;

public static class DependencyInjection
{
  public static IServiceCollection AddOfertasPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<OfertasDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");
      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");
      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "ofertas"));
    });
}

