using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Provedores;

// Seletores verificados em 30/09/2026. O executor compartilhado faz navegação e extração.
internal static class KabumProvider
{
  public const string Nome = "kabum";

  public static LojaPlaywrightOptions CriarConfiguracao() => new()
  {
    UrlBusca = "https://www.kabum.com.br/busca/{consulta}",

    HostsPermitidos = ["www.kabum.com.br", "kabum.com.br", "images.kabum.com.br", "images4.kabum.com.br",
      "static.kabum.com.br", "servicespub.prod.api.aws.grupokabum.com.br"],

    ResultadosProntos = "main a[href^='/produto/']:has(> span.line-clamp-2):visible, #listingEmpty:visible",

    Itens = "main a[href^='/produto/']:has(> span.line-clamp-2)",

    SemResultados = "#listingEmpty",

    LinkProduto = "",

    ProximaPagina = "#listingPagination a.nextLink[aria-disabled='false']",

    PaginacaoPorClique = true,

    MaxPaginas = 2,

    MaxItens = 100,

    Campos = new()
    {
      ["nome"] = new() { Seletor = ":scope > span.line-clamp-2", Obrigatorio = true },

      ["precoPix"] = new() { Seletor = "span.font-semibold[aria-hidden='false'] >> nth=1", Obrigatorio = true },

      ["condicaoPreco"] = new() { Seletor = "span.text-xs[aria-hidden='false']" },

      ["imagem"] = new() { Seletor = "img", Atributo = "src" }
    }
  };
}
