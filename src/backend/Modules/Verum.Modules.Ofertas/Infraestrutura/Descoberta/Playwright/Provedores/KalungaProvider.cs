using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Provedores;

internal static class KalungaProvider
{
  public const string Nome = "kalunga";

  public static LojaPlaywrightOptions CriarConfiguracao() => new()
  {
    UrlBusca = "https://www.kalunga.com.br/busca/{consulta}",

    HostsPermitidos = ["www.kalunga.com.br", "static.kalunga.com.br", "img.kalunga.com.br"],

    ResultadosProntos = ".blocoproduto:visible, :text-is('Nenhum produto encontrado'):visible",

    Itens = ".blocoproduto",

    SemResultados = ":text-is('Nenhum produto encontrado')",

    LinkProduto = "a.blocoproduto__link:has(h2)",

    MaxPaginas = 1,

    MaxItens = 48,

    Campos = new()
    {
      ["nome"] = new() { Seletor = "a.blocoproduto__link:has(h2)", Atributo = "title", Obrigatorio = true },

      ["preco"] = new() { Seletor = ".blocoproduto__price", Obrigatorio = true },

      ["disponivel"] = new() { Seletor = "button[onclick^='Comprar('] >> nth=0", Atributo = "onclick", Obrigatorio = true },

      ["imagem"] = new() { Seletor = "img.blocoproduto__image >> nth=0", Atributo = "data-src" }
    }
  };
}
