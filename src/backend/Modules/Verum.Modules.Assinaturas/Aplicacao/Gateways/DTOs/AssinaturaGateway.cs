namespace Verum.Modules.Assinaturas.Aplicacao.Gateways.DTOs;

internal sealed record AssinaturaGateway(string Id, Guid Referencia, string Status, decimal Valor, string Moeda, string? UrlCheckout);
