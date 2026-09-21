using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;

namespace Verum.Modules.Acesso;

public static class DependencyInjection
{
  public static IServiceCollection AddAcessoPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<AcessoDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");
      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");
      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "acesso"));
    });
}

