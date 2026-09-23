using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;

namespace Verum.Modules.Assinaturas;

public static class DependencyInjection
{
  public static IServiceCollection AddAssinaturasPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<AssinaturasDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "assinaturas"));
    });
}

