using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Notificacoes.Aplicacao.Configuracoes;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Notificacoes.Infraestrutura.Email;
using Verum.Modules.Notificacoes.Infraestrutura.Persistencia;

namespace Verum.Modules.Notificacoes;

public static class DependencyInjection
{
  public static IServiceCollection AddNotificacoesPersistencia(this IServiceCollection services, IConfiguration configuration) =>
    services.AddOptions<EmailOptions>().Bind(configuration.GetSection("Notificacoes:Email"))
      .Validate(x => x.LoteEnvio is >= 1 and <= 500 && x.AdiamentoSilencioMinutos is >= 1 and <= 1440,
        "Configure lote de envio e adiamento do horário silencioso válidos.")
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
