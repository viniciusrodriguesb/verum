namespace Verum.Modules.Assinaturas.Contratos.DTOs;

public sealed record AssinaturaConsulta(Guid Id, string Plano, string Status, decimal Valor, string Moeda,
  string? UrlCheckout, DateTimeOffset? PeriodoTerminaEm, bool CancelamentoSolicitado);
