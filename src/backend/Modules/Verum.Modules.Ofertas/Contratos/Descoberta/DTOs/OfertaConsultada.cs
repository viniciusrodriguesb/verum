namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record OfertaConsultada(
  Guid Id, Guid VarianteId, string Nome, string Url, decimal Preco, string CondicaoPreco,
  string? ImagemUrl, string Fonte, decimal Confianca, DateTimeOffset ObservadaEm, DateTimeOffset ValidaAte);
