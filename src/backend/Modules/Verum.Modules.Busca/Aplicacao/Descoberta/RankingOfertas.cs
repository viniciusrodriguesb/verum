using Verum.Modules.Busca.Contratos;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class RankingOfertas
{
  public const string Versao = "relevancia-conservadora-v2";

  public static IReadOnlyList<OfertaResultado> Classificar(IEnumerable<OfertaConsultada> candidatas, int limite, string consulta)
  {
    var agora = DateTimeOffset.UtcNow;

    var validas = candidatas.Where(x => x.ValidaAte > agora && x.Preco > 0 && NormalizacaoOferta.Corresponde(consulta, x.Nome))
      .OrderByDescending(x => x.ObservadaEm).DistinctBy(x => x.Url).ToArray();

    var menores = validas.GroupBy(x => x.VarianteId).ToDictionary(x => x.Key, x => x.Min(y => y.Preco));

    return validas.Select(x =>
    {
      var menor = menores[x.VarianteId];

      var preco = menor / x.Preco * 100m;

      var atualizacao = Math.Clamp(100m - (decimal)(agora - x.ObservadaEm).TotalMinutes, 0m, 100m);

      var relevancia = NormalizacaoOferta.Relevancia(consulta, x.Nome);

      var pontuacao = decimal.Round(relevancia * 0.7m + preco * 0.09m + x.Confianca * 0.12m + atualizacao * 0.09m, 2);

      return new OfertaResultado(x.Id, x.VarianteId, x.Nome, x.Url, x.Preco, x.CondicaoPreco, x.ImagemUrl,
        x.Fonte, x.ObservadaEm, x.ValidaAte, 0, pontuacao,
        $"Correspondência com a busca priorizada ({relevancia:0}/100); acessórios ficam após produtos principais quando não solicitados. "
          + "Confiança inicial da loja/fonte e atualização. Preço comparado somente na mesma variante; nota máxima com uma oferta não comprova desconto.")
      {
        PontuacaoPreco = decimal.Round(preco, 2),

        PontuacaoConfianca = x.Confianca,

        PontuacaoAtualizacao = decimal.Round(atualizacao, 2)
      };
    }).OrderByDescending(x => x.Pontuacao).ThenBy(x => x.Url, StringComparer.Ordinal).Take(limite)
      .Select((x, i) => x with { Posicao = i + 1 }).ToArray();
  }
}
