namespace Verum.Modules.Radar.Contratos.DTOs;

public sealed record CriarMonitoramento(Guid OfertaId, Guid VarianteId, decimal PrecoAlvo,
  string ChaveIdempotencia, bool IdentidadeConfirmada, bool EmailAutorizado);
