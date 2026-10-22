using System.Text;
using System.Text.RegularExpressions;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial.DTOs;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Verum.Modules.Ofertas.Contratos.Descoberta;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class NormalizacaoOferta
{
  public static string Normalizar(string texto) =>
    Regex.Replace(Regex.Replace(Regex.Replace(texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD), @"\p{Mn}", "")
      .Normalize(NormalizationForm.FormC), @"\bps\s*5\b", "playstation 5"), @"\b(televisao|televisor|televisores)\b", "tv");

  private static string[] Tokens(string texto) =>
    Regex.Matches(Regex.Replace(Regex.Replace(Normalizar(texto), @"\b(playstation|iphone)\s*(\d{1,2})\b", "$1$2"),
      @"\b(\d+)\s+(tb|gb|mb|mhz|ghz|hz|mm|cm|k)\b", "$1$2"), @"[a-z0-9]+")
      .Select(x => x.Value).ToArray();

  public static bool ConsultaEspecifica(string consulta) => Tokens(consulta).Length >= 2
    || Regex.IsMatch(Normalizar(consulta), @"\b(?:playstation|iphone)\s*\d{1,2}\b");

  public static bool Corresponde(string consulta, string nome)
  {
    var tokens = Tokens(consulta);

    var titulo = Tokens(nome).ToHashSet();

    return tokens.Length > 0 && tokens.All(titulo.Contains);
  }

  public static decimal Relevancia(string consulta, string nome)
  {
    if (!Corresponde(consulta, nome)) return 0m;

    const string acessorios = @"\b(capas?|capinhas?|peliculas?|controles?|suportes?|cabos?|adaptadores?|case|carregador|fone|caneta|protetor|pen\s*drive|pendrive|cartao\s+de\s+memoria|stand|bases?|disc\s+drive|disk\s+drive|coolers?|ventoinhas?|tampas?|leitor(?:es)?(?:\s+(?:de\s+)?disco)?|unidade\s+de\s+disco|drive\s+de\s+disco|fontes?(?:\s+(?:de\s+)?alimentacao)?)\b";

    var titulo = Normalizar(nome);

    var pedido = Normalizar(consulta);

    // Compatibilidade não prova identidade. Não mistura condição ou edição não solicitada.
    if (Regex.IsMatch(pedido, @"\b(iphone|smartphone|celular)\b")
      && Regex.IsMatch(titulo, @"\b(pen\s*drive|pendrive|capinha|pelicula|capa|adaptador)\b")
      && !Regex.IsMatch(pedido, @"\b(pen\s*drive|pendrive|capinha|pelicula|capa|adaptador)\b")) return 0m;

    if (Regex.IsMatch(titulo, @"\b(seminovo|usado|recondicionado|vitrine)\b")
      && !Regex.IsMatch(pedido, @"\b(seminovo|usado|recondicionado|vitrine)\b")) return 0m;

    if (Regex.IsMatch(pedido, @"\b(iphone|samsung)\b")
      && new[] { "pro", "plus", "max", "mini", "ultra", "fe", "air" }.Any(x => Regex.IsMatch(titulo, $@"\b{x}\b")
        && !Regex.IsMatch(pedido, $@"\b{x}\b"))) return 0m;

    var acessorio = Regex.Match(titulo, acessorios);

    var principal = Regex.Match(titulo, @"\b(smartphone|celular|console|tv)\b");

    // Marcas podem preceder o acessório. Um produto principal identificado antes dele preserva kits de console/celular.
    var acessorioNaoSolicitado = acessorio.Success && (!principal.Success || acessorio.Index < principal.Index);

    if (acessorioNaoSolicitado && !Regex.IsMatch(Normalizar(consulta), acessorios)) return 30m;

    return 100m - Math.Min(20, Math.Max(0, Tokens(nome).Length - Tokens(consulta).Length));
  }

  public static string? UrlCanonica(string? valor)
  {
    if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri) || uri.Scheme != "https"
      || uri.HostNameType != UriHostNameType.Dns || !uri.Host.Contains('.') || uri.IsLoopback
      || valor!.Length > 8192 || !string.IsNullOrEmpty(uri.UserInfo)) return null;

    return new UriBuilder(uri) { Fragment = "" }.Uri.AbsoluteUri;
  }

  public static OfertaDescoberta? DaLoja(ItemConsultaLoja item, string fonte, string consulta, DateTimeOffset observadaEm)
  {
    item.Campos.TryGetValue("nome", out var nome);

    item.Campos.TryGetValue("precoPix", out var texto);

    var pix = !string.IsNullOrWhiteSpace(texto);

    if (!pix) item.Campos.TryGetValue("preco", out texto);

    item.Campos.TryGetValue("imagem", out var imagem);

    var url = UrlCanonica(item.Url);

    if (url is null || string.IsNullOrWhiteSpace(nome) || nome.Length > 300 || Relevancia(consulta, nome) <= 30m
      || !NormalizacaoPreco.Tentar(texto, out var preco)
      || preco <= 0 || preco > 9999999999.99m) return null;

    return new(nome, url, preco, pix ? "À vista no PIX; confirme as condições na loja." : "Preço anunciado; confirme vendedor e condições na loja.",
      UrlCanonica(imagem), fonte, false, pix ? "Preço PIX observado no card público da loja." : "Preço total anunciado no card público da loja.", observadaEm);
  }

  public static OfertaDescoberta? DaIa(ProdutoCandidatoIa item, string consulta, DateTimeOffset observadaEm)
  {
    var url = UrlCanonica(item.Url);

    var preco = item.PrecoPix ?? item.Preco;

    if (url is null || item.Nome.Length > 300 || Relevancia(consulta, item.Nome) <= 30m
      || preco is null or <= 0 or > 9999999999.99m || decimal.Round(preco.Value, 2) != preco.Value
      || item.Moeda != "BRL" || item.Disponibilidade != "disponivel"
      || item.CondicaoProduto != "novo" || item.Correspondencia != "exata"
      || item.CondicaoPreco?.Length > 300 || string.IsNullOrWhiteSpace(item.Evidencia)) return null;

    return new(item.Nome, url, preco.Value,
      item.PrecoPix.HasValue ? "À vista no PIX; confirme na loja." : item.CondicaoPreco ?? "Preço informado pela fonte; confirme na loja.",
      UrlCanonica(item.ImagemUrl), "ia", true, item.Evidencia, observadaEm);
  }
}
