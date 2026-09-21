using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Contas.Infraestrutura.Persistencia;

namespace Verum.Modules.Contas;

public static class DependencyInjection
{
  public static IServiceCollection AddContasPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<ContasDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");
      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");
      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "contas"));
    });
}

