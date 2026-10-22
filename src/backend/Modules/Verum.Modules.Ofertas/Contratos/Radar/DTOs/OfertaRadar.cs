namespace Verum.Modules.Ofertas.Contratos.Radar.DTOs;

public sealed record OfertaRadar(Guid Id, Guid VarianteId, long ObservacaoId, string Nome, string Url,
  decimal Preco, string Condicao, string Fonte, string? ImagemUrl, DateTimeOffset ObservadaEm, DateTimeOffset ValidaAte);
