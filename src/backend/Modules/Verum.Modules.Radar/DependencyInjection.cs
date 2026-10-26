using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Radar.Contratos.Configuracoes;
using Verum.Modules.Radar.Infraestrutura.Persistencia;

namespace Verum.Modules.Radar;

public static class DependencyInjection
{
  public static IServiceCollection AddRadarPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddOptions<RadarOptions>().Bind(configuration.GetSection("Radar"))
      .Validate(x => x.LimiteMonitoramentos is > 0 and <= 200 && x.IntervaloVerificacaoMinutos >= 1
        && x.CooldownMinutos >= 1 && x.HisteresePercentual is >= 0 and <= 100 && x.Lote is > 0 and <= 100,
        "Configure limites válidos em Radar.").ValidateOnStart().Services
      .AddDbContext<RadarDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "radar"));
    });
}
