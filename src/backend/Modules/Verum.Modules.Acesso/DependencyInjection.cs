using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Acesso.Aplicacao.Configuracoes;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;

namespace Verum.Modules.Acesso;

public static class DependencyInjection
{
  public static IServiceCollection AddAcessoPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddOptions<AcessoOptions>().Bind(configuration.GetSection("Acesso"))
      .Configure(x => x.LimiteVisitanteDiario = configuration.GetValue<int?>("Acesso:LimiteVisitanteDiario")
        ?? configuration.GetValue<int?>("Busca:LimiteVisitanteDiario") ?? x.LimiteVisitanteDiario)
      .Validate(x => x.LimiteVisitanteDiario > 0 && x.LimiteFreeDiario > 0 && x.LimitePagoDiario > 0,
        "Configure limites positivos em Acesso.").ValidateOnStart().Services
      .AddDbContext<AcessoDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "acesso"));
    });
}
