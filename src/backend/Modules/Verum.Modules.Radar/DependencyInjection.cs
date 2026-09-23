using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Radar.Infraestrutura.Persistencia;

namespace Verum.Modules.Radar;

public static class DependencyInjection
{
  public static IServiceCollection AddRadarPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<RadarDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "radar"));
    });
}

