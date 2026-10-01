using Verum.Modules.Busca.Contratos;
using Verum.Modules.Ofertas.Contratos.Descoberta;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class RankingOfertas
{
  public const string Versao = "correspondencia-conservadora-v1";

  public static IReadOnlyList<OfertaResultado> Classificar(IEnumerable<OfertaConsultada> candidatas, int limite)
  {
    var agora = DateTimeOffset.UtcNow;

    var validas = candidatas.Where(x => x.ValidaAte > agora && x.Preco > 0).DistinctBy(x => x.Url).ToArray();

    var menores = validas.GroupBy(x => x.VarianteId).ToDictionary(x => x.Key, x => x.Min(y => y.Preco));

    return validas.Select(x =>
    {
      var menor = menores[x.VarianteId];

      var preco = menor / x.Preco * 100m;

      var atualizacao = Math.Clamp(100m - (decimal)(agora - x.ObservadaEm).TotalMinutes, 0m, 100m);

      var pontuacao = decimal.Round(preco * 0.3m + x.Confianca * 0.4m + atualizacao * 0.3m, 2);

      return new OfertaResultado(x.Id, x.VarianteId, x.Nome, x.Url, x.Preco, x.CondicaoPreco, x.ImagemUrl,
        x.Fonte, x.ObservadaEm, x.ValidaAte, 0, pontuacao,
        "Correspondência textual com a consulta; confiança da loja/fonte e atualização. Preço comparado somente na mesma variante.")
      {
        PontuacaoPreco = decimal.Round(preco, 2),

        PontuacaoConfianca = x.Confianca,

        PontuacaoAtualizacao = decimal.Round(atualizacao, 2)
      };
    }).OrderByDescending(x => x.Pontuacao).ThenBy(x => x.Url, StringComparer.Ordinal).Take(limite)
      .Select((x, i) => x with { Posicao = i + 1 }).ToArray();
  }
}
