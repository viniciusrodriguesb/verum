using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Busca.Infraestrutura.Persistencia;
using Verum.Modules.Busca.Aplicacao.Descoberta;
using MassTransit;

namespace Verum.Modules.Busca;

public static class DependencyInjection
{
  public static void AddBuscaOutbox(this IBusRegistrationConfigurator bus) =>
    bus.AddEntityFrameworkOutbox<BuscaDbContext>(options =>
    {
      options.UsePostgres();

      options.UseBusOutbox();

      options.QueryDelay = TimeSpan.FromSeconds(1);
    });

  public static IServiceCollection AddBuscaPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddOptions<BuscaOptions>()
      .Bind(configuration.GetSection("Busca"))
      .Validate(x => x.MinimoOfertas is >= 1 and <= 100 && x.MaximoResultados is >= 1 and <= 100
        && x.PrazoSegundos is >= 5 and <= 900 && x.TimeoutFonteSegundos > 0 && x.TimeoutFonteSegundos <= x.PrazoSegundos
        && x.TimeoutIaSegundos > 0 && x.TimeoutIaSegundos <= x.PrazoSegundos
        && x.LimiteVisitanteDiario is >= 1 and <= 1000
        && System.Text.RegularExpressions.Regex.IsMatch(x.FilaSolicitacoes, "^[a-z][a-z0-9-]{1,99}$")
        && x.Lojas.Length <= 10 && x.Lojas.All(y => !string.IsNullOrWhiteSpace(y) && y.Length <= 100),
        "Limites de Busca inválidos.")
      .ValidateOnStart().Services
      .AddDbContext<BuscaDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "busca"));
    });
}
