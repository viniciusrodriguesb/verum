using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Provedores;

internal static class AmazonProvider
{
  public const string Nome = "amazon";

  public static LojaPlaywrightOptions CriarConfiguracao() => new()
  {
    UrlBusca = "https://www.amazon.com.br/s?k={consulta}",

    HostsPermitidos = ["www.amazon.com.br", "amazon.com.br", "m.media-amazon.com", "images-na.ssl-images-amazon.com"],

    ResultadosProntos = "[data-component-type='s-search-result']:visible, .s-no-results:visible, [data-component-type='s-search-results'] :text-is('Nenhum resultado para sua consulta de pesquisa.'):visible",

    Itens = "[data-component-type='s-search-result'][data-asin]:not([data-asin=''])",

    SemResultados = ".s-no-results, [data-component-type='s-search-results'] :text-is('Nenhum resultado para sua consulta de pesquisa.')",

    Bloqueio = "#captchacharacters, form[action*='validateCaptcha'], #challenge-container",

    LinkProduto = "a:has(h2) >> nth=0",

    CaminhoProdutoPadrao = @"/dp/[A-Z0-9]{10}(?=/|\?|$)",

    ProximaPagina = "a.s-pagination-next:not(.s-pagination-disabled)",

    MaxPaginas = 2,

    MaxItens = 100,

    Campos = new()
    {
      ["nome"] = new() { Seletor = "h2 >> nth=0", Obrigatorio = true },

      ["preco"] = new() { Seletor = ".a-price:not(.a-text-price) .a-offscreen >> nth=0", Obrigatorio = true },

      ["imagem"] = new() { Seletor = "img.s-image >> nth=0", Atributo = "src" }
    }
  };
}
