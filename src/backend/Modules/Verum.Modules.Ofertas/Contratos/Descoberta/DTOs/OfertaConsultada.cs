namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record OfertaConsultada(
  Guid Id, Guid VarianteId, string Nome, string Url, decimal Preco, string CondicaoPreco,
  string? ImagemUrl, string Fonte, decimal Confianca, DateTimeOffset ObservadaEm, DateTimeOffset ValidaAte)
{
  public Guid LojaId { get; init; }

  public decimal? ConfiancaLoja { get; init; }

  public decimal? ConfiancaDado { get; init; }

  public IReadOnlyList<ObservacaoPrecoRanking> Historico { get; init; } = [];
}
