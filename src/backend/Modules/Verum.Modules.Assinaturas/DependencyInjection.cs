using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Assinaturas.Aplicacao;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Assinaturas.Aplicacao.Gateways;
using Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago;
using Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.Configuracoes;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;
using Verum.Modules.Assinaturas.Infraestrutura.Processadores;

namespace Verum.Modules.Assinaturas;

public static class DependencyInjection
{
  public static IServiceCollection AddAssinaturasPersistencia(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<AssinaturasOptions>().Bind(configuration.GetSection("Assinaturas"))
      .Validate(x => x.IntervaloProcessamentoSegundos is >= 1 and <= 60 && x.IntervaloReconciliacaoSegundos >= 30,
        "Configure os intervalos de processamento de assinaturas.")
      .Validate(x => x.PrecosMensais.All(p => p.Key is "BUSCA_PLUS" or "VERUM_PLUS"
        && p.Value >= 0 && p.Value <= 999999999999.99m && decimal.Round(p.Value, 2) == p.Value),
        "Configure preços mensais em BRL com duas casas decimais.").ValidateOnStart();

    services.AddOptions<MercadoPagoOptions>().Bind(configuration.GetSection("Assinaturas:MercadoPago"))
      .Validate(x => !configuration.GetValue<bool>("Assinaturas:Habilitado")
        || (!string.IsNullOrWhiteSpace(x.AccessToken) && !string.IsNullOrWhiteSpace(x.WebhookSecret) && x.CollectorId > 0
          && Uri.TryCreate(x.UrlRetorno, UriKind.Absolute, out var retorno) && retorno.Scheme == "https"
          && Uri.TryCreate(x.UrlWebhook, UriKind.Absolute, out var webhook) && webhook.Scheme == "https"),
        "Configure credenciais, CollectorId e URLs HTTPS do Mercado Pago.").ValidateOnStart();

    services.AddHttpClient("MercadoPago");

    services.AddScoped<IGatewayAssinaturas, MercadoPagoGateway>();

    services.AddScoped<SelecionadorGateway>();

    return services.AddDbContext<AssinaturasDbContext>(options =>
    {
      var connection = configuration.GetConnectionString("PostgreSQL");

      if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

      options.UseNpgsql(connection, postgres =>
        postgres.MigrationsHistoryTable("__EFMigrationsHistory", "assinaturas"));
    });
  }

  public static IServiceCollection AddAssinaturasProcessamento(this IServiceCollection services) =>
    services.AddScoped<ProcessamentoPagamentos>().AddHostedService<PagamentosWorker>();
}
