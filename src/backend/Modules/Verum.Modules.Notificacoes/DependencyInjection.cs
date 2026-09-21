using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Notificacoes.Infraestrutura.Persistencia;

namespace Verum.Modules.Notificacoes;

public static class DependencyInjection
{
  public static IServiceCollection AddNotificacoesPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddDbContext<NotificacoesDbContext>(options =>
    {

      var connection = configuration.GetConnectionString("PostgreSQL");
      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "notificacoes"));

    });
}

