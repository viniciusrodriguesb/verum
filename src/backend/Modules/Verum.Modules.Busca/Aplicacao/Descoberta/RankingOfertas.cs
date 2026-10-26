using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using static Verum.Modules.Busca.Contratos.Configuracoes.CriteriosRanking;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class RankingOfertas
{
  public const string Versao = "ranking-v1.3";

  public static IReadOnlyList<OfertaResultado> Classificar(IEnumerable<OfertaConsultada> candidatas, int limite, string consulta)
  {
    var agora = DateTimeOffset.UtcNow;

    var validas = candidatas.Where(x => x.ValidaAte > agora && x.Preco > 0 && x.ObservadaEm <= agora
      && NormalizacaoOferta.Relevancia(consulta, x.Nome) > 30m)
                            .OrderByDescending(x => x.ObservadaEm).DistinctBy(x => x.Url)
                            .ToArray();

    return validas.Select(x => Avaliar(x, validas, agora))
      .OrderByDescending(x => x.Pontuacao).ThenByDescending(x => x.PontuacaoConfianca)
      .ThenBy(x => x.Preco).ThenByDescending(x => x.ObservadaEm).ThenBy(x => x.Url, StringComparer.Ordinal).Take(limite)
      .Select((x, i) => x with { Posicao = i + 1 }).ToArray();
  }

  private static OfertaResultado Avaliar(OfertaConsultada x, OfertaConsultada[] validas, DateTimeOffset agora)
  {
    var calculo = CalcularPreco(x, validas, agora);

    var idade = Math.Max(0, (agora - x.ObservadaEm).TotalMinutes);

    var atualizacao = (decimal)(100d * Math.Pow(2, -idade / MeiaVidaMinutos));

    var confianca = Math.Clamp((x.ConfiancaLoja ?? x.Confianca) * PesoConfiancaLoja
      + (x.ConfiancaDado ?? x.Confianca) * PesoConfiancaDado, 0m, 100m);

    var pontuacao = decimal.Round(calculo.Nota * PesoPreco + confianca * PesoConfianca + atualizacao * PesoAtualizacao, 2);

    return new OfertaResultado(x.Id, x.VarianteId, x.Nome, x.Url, x.Preco, x.CondicaoPreco, x.ImagemUrl,
      x.Fonte, x.ObservadaEm, x.ValidaAte, 0, pontuacao,
      calculo.Fundamentos.PrecoProvisorio ? "Preço provisório: dados insuficientes para avaliar preço justo. Confiança inicial da loja/fonte; vendedor não confirmado."
        : "Preço calculado por percentis de ofertas comparáveis e histórico suficiente quando disponível. Confiança inicial da loja/fonte; vendedor não confirmado.")
    {
      PontuacaoPreco = decimal.Round(calculo.Nota, 2),

      PontuacaoConfianca = decimal.Round(confianca, 2),

      PontuacaoAtualizacao = decimal.Round(atualizacao, 2),

      Fundamentos = calculo.Fundamentos
    };
  }

  private static (decimal Nota, FundamentosRanking Fundamentos) CalcularPreco(OfertaConsultada x, OfertaConsultada[] validas, DateTimeOffset agora)
  {
    var pix = x.CondicaoPreco.Contains("PIX", StringComparison.OrdinalIgnoreCase);

    var mercado = validas.Where(y => y.VarianteId == x.VarianteId
      && y.CondicaoPreco.Contains("PIX", StringComparison.OrdinalIgnoreCase) == pix)
      .GroupBy(y => y.LojaId == Guid.Empty ? y.Fonte : y.LojaId.ToString()).Select(y => y.Min(z => z.Preco)).ToArray();

    var historico = x.Historico.Where(y => y.Pix == pix && y.Preco > 0 && y.ObservadaEm >= agora.AddDays(-JanelaHistoricoDias)
      && y.ObservadaEm <= agora).GroupBy(y => new { y.LojaId, Dia = y.ObservadaEm.UtcDateTime.Date })
      .Select(y => y.MinBy(z => z.Preco)!).ToArray();

    var dias = historico.Select(y => y.ObservadaEm.UtcDateTime.Date).Distinct().Count();

    var lojas = historico.Select(y => y.LojaId).Distinct().Count();

    var suficiente = historico.Length >= MinimoObservacoesHistorico && lojas >= MinimoLojasHistorico;

    var precoMercado = mercado.Length >= MinimoLojasMercado ? 100m * (1m - EstatisticasRanking.Percentil(x.Preco, mercado)) : NotaProvisoria;

    var percentil = historico.Length == 0 ? (decimal?)null : EstatisticasRanking.Percentil(x.Preco, historico.Select(y => y.Preco).ToArray());

    var preco = suficiente ? 100m * (1m - percentil!.Value) : precoMercado;

    if (suficiente && mercado.Length >= MinimoLojasMercado) preco = preco * PesoHistorico + precoMercado * PesoMercado;

    var idade = Math.Max(0, (agora - x.ObservadaEm).TotalMinutes);

    var fundamentos = new FundamentosRanking(Versao, historico.Length >= ObservacoesConfiancaAlta && dias >= DiasConfiancaAlta && lojas >= LojasConfiancaAlta ? "Alta"
      : suficiente ? "Media" : "Baixa", !suficiente && mercado.Length < MinimoLojasMercado, mercado.Length, historico.Length, dias, lojas,
      mercado.Length > 0 ? EstatisticasRanking.Mediana(mercado) : null,
      historico.Length > 0 ? EstatisticasRanking.Mediana(historico.Select(y => y.Preco)) : null, percentil, Math.Round(idade, 2));

    return (preco, fundamentos);
  }
}
