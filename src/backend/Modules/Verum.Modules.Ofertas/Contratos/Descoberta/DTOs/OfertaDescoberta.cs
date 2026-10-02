namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record OfertaDescoberta(
  string Nome, string Url, decimal Preco, string CondicaoPreco, string? ImagemUrl,
  string Fonte, bool OrigemIa, string Evidencia, DateTimeOffset ObservadaEm);
