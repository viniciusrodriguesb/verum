namespace Verum.Modules.Busca.Contratos.DTOs;

public sealed record OfertaResultado(
  Guid OfertaId, Guid VarianteId, string Nome, string Url, decimal Preco, string CondicaoPreco,
  string? ImagemUrl, string Fonte, DateTimeOffset ObservadaEm, DateTimeOffset ValidaAte,
  int Posicao, decimal Pontuacao, string Explicacao)
{
  public decimal PontuacaoPreco { get; init; }

  public decimal PontuacaoConfianca { get; init; }

  public decimal PontuacaoAtualizacao { get; init; }
}
