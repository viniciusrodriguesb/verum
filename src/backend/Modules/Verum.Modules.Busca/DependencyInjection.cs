using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Busca.Infraestrutura.Persistencia;

namespace Verum.Modules.Busca;

public static class DependencyInjection
{
  public static IServiceCollection AddBuscaPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<BuscaDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "busca"));
    });
}

