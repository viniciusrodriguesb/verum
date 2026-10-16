namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.DTOs;

internal sealed record PreapprovalResposta(string Id, string ExternalReference, string Status, string? InitPoint,
  RecorrenciaResposta AutoRecurring, long CollectorId);
