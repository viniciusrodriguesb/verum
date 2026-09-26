using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Ofertas.Contratos.Descoberta;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright;

internal static class ConfiguracaoPlaywright
{
  public static IServiceCollection Registrar(IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<PlaywrightOptions>()
      .Bind(configuration.GetSection("Playwright"))
      .Validate(x => x.ConsultasSimultaneas is >= 1 and <= 16
        && x.TimeoutTotalSegundos is >= 1 and <= 300
        && x.TimeoutAcaoSegundos >= 1 && x.TimeoutAcaoSegundos <= x.TimeoutTotalSegundos,
        "Limites de execução do Playwright inválidos.")
      .Validate(x => x.Lojas.All(loja => Valida(loja.Key, loja.Value)), "Configuração de loja Playwright inválida.")
      .ValidateOnStart();

    services.AddSingleton<NavegadorPlaywright>();

    services.AddScoped<IConsultaLoja, ConsultaLojaPlaywright>();

    return services;
  }

  private static bool Valida(string nome, LojaPlaywrightOptions loja) =>
    !string.IsNullOrWhiteSpace(nome)
    && Uri.TryCreate(loja.UrlBusca.Replace("{consulta}", "teste", StringComparison.Ordinal), UriKind.Absolute, out var uri)
    && ConsultaLojaPlaywright.Permitida(uri.AbsoluteUri, loja)
    && (loja.CampoBusca is not null || loja.UrlBusca.Contains("{consulta}", StringComparison.Ordinal))
    && (loja.BotaoBusca is null || !string.IsNullOrWhiteSpace(loja.CampoBusca))
    && !string.IsNullOrWhiteSpace(loja.ResultadosProntos)
    && !string.IsNullOrWhiteSpace(loja.Itens)
    && loja.MaxPaginas is >= 1 and <= 10
    && loja.MaxItens is >= 1 and <= 100
    && loja.Campos.Count > 0
    && loja.Campos.All(campo => !string.IsNullOrWhiteSpace(campo.Key));
}
