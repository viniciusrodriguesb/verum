using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;
using Verum.Modules.Ofertas.Contratos.Descoberta;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class NormalizacaoOferta
{
  public static string Normalizar(string texto) =>
    Regex.Replace(texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD), @"\p{Mn}", "")
      .Normalize(NormalizationForm.FormC);

  private static string[] Tokens(string texto) =>
    Regex.Matches(Regex.Replace(Normalizar(texto), @"([a-z])([0-9])|([0-9])([a-z])", "$1$3 $2$4"), @"[a-z0-9]+")
      .Select(x => x.Value).ToArray();

  public static bool ConsultaEspecifica(string consulta) => Tokens(consulta).Length >= 2;

  public static bool Corresponde(string consulta, string nome)
  {
    var tokens = Tokens(consulta);

    var titulo = Tokens(nome).ToHashSet();

    return tokens.Length > 0 && tokens.All(titulo.Contains);
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

    item.Campos.TryGetValue("imagem", out var imagem);

    var url = UrlCanonica(item.Url);

    if (url is null || string.IsNullOrWhiteSpace(nome) || nome.Length > 300 || !Corresponde(consulta, nome)
      || texto is null || !Regex.IsMatch(texto, @"^(?:\d{1,3}(?:\.\d{3})*|\d+),\d{2}$")
      || !decimal.TryParse(texto, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out var preco)
      || preco <= 0 || preco > 9999999999.99m) return null;

    return new(nome, url, preco, "À vista no PIX; confirme as condições na loja.",
      UrlCanonica(imagem), fonte, false, "Preço PIX observado no card público da loja.", observadaEm);
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
