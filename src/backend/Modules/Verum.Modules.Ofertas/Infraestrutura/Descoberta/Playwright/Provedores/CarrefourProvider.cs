using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Provedores;

internal static class CarrefourProvider
{
  public const string Nome = "carrefour";

  public static LojaPlaywrightOptions CriarConfiguracao() => new()
  {
    UrlBusca = "https://www.carrefour.com.br/busca/{consulta}",

    HostsPermitidos = ["www.carrefour.com.br", "carrefourbr.vtexassets.com", "statics.carrefour.com.br"],

    ResultadosProntos = "a[data-testid='search-product-card']:visible, :text-is('oops, não encontramos resultados'):visible",

    Itens = "a[data-testid='search-product-card']",

    SemResultados = ":text-is('oops, não encontramos resultados')",

    MaxPaginas = 1,

    MaxItens = 48,

    Campos = new()
    {
      ["nome"] = new() { Seletor = "h2", Obrigatorio = true },

      ["preco"] = new() { Seletor = "span.text-default.font-bold >> nth=0", Obrigatorio = true },

      ["precoPix"] = new() { Seletor = "div:has(> span.text-default.font-bold):has(:text-is('à vista no Pix')) > span.text-default.font-bold" },

      ["imagem"] = new() { Seletor = "img >> nth=0", Atributo = "src" }
    }
  };
}
