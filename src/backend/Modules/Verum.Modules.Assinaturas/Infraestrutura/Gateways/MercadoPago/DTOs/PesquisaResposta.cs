namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.DTOs;

internal sealed record PesquisaResposta<T>(List<T> Results, PaginacaoResposta? Paging);
