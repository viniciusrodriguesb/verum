using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial.DTOs;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class NormalizacaoOferta
{
  public static string Normalizar(string texto) =>
    Regex.Replace(Regex.Replace(Regex.Replace(texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD), @"\p{Mn}", "")
      .Normalize(NormalizationForm.FormC), @"\bps\s*5\b", "playstation 5"), @"\b(televisao|televisor|televisores)\b", "tv");

  private static string[] Tokens(string texto) =>
    Regex.Matches(Regex.Replace(Normalizar(texto), @"\b(\d+)\s+(tb|gb|mb|mhz|ghz|hz|mm|cm|k)\b", "$1$2"), @"[a-z0-9]+")
      .Select(x => x.Value).ToArray();

  public static bool ConsultaEspecifica(string consulta) => Tokens(consulta).Length >= 2;

  public static bool Corresponde(string consulta, string nome)
  {
    var tokens = Tokens(consulta);

    var titulo = Tokens(nome).ToHashSet();

    return tokens.Length > 0 && tokens.All(titulo.Contains);
  }

  public static decimal Relevancia(string consulta, string nome)
  {
    if (!Corresponde(consulta, nome)) return 0m;

    const string acessorios = @"\b(capa|capinha|pelicula|controle|suporte|cabo|adaptador|case|carregador|fone|caneta|protetor)\b";

    var titulo = Normalizar(nome);

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

    if (url is null || string.IsNullOrWhiteSpace(nome) || nome.Length > 300 || !Corresponde(consulta, nome)
      || !TentarPreco(texto, out var preco)
      || preco <= 0 || preco > 9999999999.99m) return null;

    return new(nome, url, preco, pix ? "À vista no PIX; confirme as condições na loja." : "Preço anunciado; confirme vendedor e condições na loja.",
      UrlCanonica(imagem), fonte, false, pix ? "Preço PIX observado no card público da loja." : "Preço total anunciado no card público da loja.", observadaEm);
  }

  private static bool TentarPreco(string? texto, out decimal preco)
  {
    preco = 0;

    if (texto is null) return false;

    texto = Regex.Replace(texto.Trim(), @"^R\$\s*", "");

    var brasileiro = Regex.IsMatch(texto, @"^(?:\d{1,3}(?:\.\d{3})*|\d+),\d{2}$");

    var internacional = Regex.IsMatch(texto, @"^(?:\d{1,3}(?:,\d{3})*|\d+)\.\d{2}$");

    return (brasileiro || internacional) && decimal.TryParse(texto, NumberStyles.Number,
      CultureInfo.GetCultureInfo(brasileiro ? "pt-BR" : "en-US"), out preco);
  }

  public static OfertaDescoberta? DaIa(ProdutoCandidatoIa item, string consulta, DateTimeOffset observadaEm)
  {
    var url = UrlCanonica(item.Url);

    var preco = item.PrecoPix ?? item.Preco;

    if (url is null || item.Nome.Length > 300 || !Corresponde(consulta, item.Nome)
      || preco is null or <= 0 or > 9999999999.99m || decimal.Round(preco.Value, 2) != preco.Value
      || item.Moeda != "BRL" || item.Disponibilidade != "disponivel"
      || item.CondicaoProduto != "novo" || item.Correspondencia != "exata"
      || item.CondicaoPreco?.Length > 300 || string.IsNullOrWhiteSpace(item.Evidencia)) return null;

    return new(item.Nome, url, preco.Value,
      item.PrecoPix.HasValue ? "À vista no PIX; confirme na loja." : item.CondicaoPreco ?? "Preço informado pela fonte; confirme na loja.",
      UrlCanonica(item.ImagemUrl), "ia", true, item.Evidencia, observadaEm);
  }
}
