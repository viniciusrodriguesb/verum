using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Notificacoes.Infraestrutura.Persistencia;
using Verum.Modules.Notificacoes.Aplicacao.Configuracoes;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Notificacoes.Infraestrutura.Email;

namespace Verum.Modules.Notificacoes;

public static class DependencyInjection
{
  public static IServiceCollection AddNotificacoesPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddOptions<EmailOptions>().Bind(configuration.GetSection("Notificacoes:Email"))
      .Validate(x => !x.Habilitado || !string.IsNullOrWhiteSpace(x.Host) && x.Porta > 0
        && x.Remetente.Contains('@') && x.MaximoTentativas is > 0 and <= 20, "Configure SMTP válido.")
      .ValidateOnStart().Services.AddScoped<IEnvioEmail, EnvioEmailSmtp>()
      .AddDbContext<NotificacoesDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "notificacoes"));
    });
}
