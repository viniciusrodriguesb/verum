namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal static class EstatisticasRanking
{
  public static decimal Mediana(IEnumerable<decimal> precos)
  {
    var ordenados = precos.Order().ToArray();

    return ordenados.Length % 2 == 0
      ? (ordenados[ordenados.Length / 2 - 1] + ordenados[ordenados.Length / 2]) / 2
      : ordenados[ordenados.Length / 2];
  }

  public static decimal Percentil(decimal preco, decimal[] precos)
  {
    var tolerancia = preco * 0.005m;

    var menores = precos.Count(x => x < preco - tolerancia);

    var empates = precos.Count(x => Math.Abs(x - preco) <= tolerancia);

    return (menores + empates * 0.5m) / precos.Length;
  }
}
