using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Provedores;

internal static class DecathlonProvider
{
  public const string Nome = "decathlon";

  public static LojaPlaywrightOptions CriarConfiguracao() => new()
  {
    UrlBusca = "https://www.decathlon.com.br/pesquisa/?q={consulta}",

    HostsPermitidos = ["www.decathlon.com.br", "decathlonstore.vtexassets.com", "contents.mediadecathlon.com"],

    ResultadosProntos = "[data-testid='product-card']:visible, :text-is('Nenhum produto encontrado'):visible",

    Itens = "[data-testid='product-card']",

    SemResultados = ":text-is('Nenhum produto encontrado')",

    LinkProduto = "a[data-testid='product-link']",

    MaxPaginas = 1,

    MaxItens = 48,

    Campos = new()
    {
      ["nome"] = new() { Seletor = "h2", Obrigatorio = true },

      // Somente o campo de preço principal: nunca extrai preço de parcela ou cashback.
      ["preco"] = new() { Seletor = ".product-card__content--price > p:not(.line-through) >> nth=0", PadraoExtracao = @"^R\$\s*[\d.,]+(?=\s|$)", Obrigatorio = true },

      ["imagem"] = new() { Seletor = "img[data-store-image] >> nth=0", Atributo = "src" }
    }
  };
}
