namespace Verum.Modules.Assinaturas.Aplicacao.Gateways.DTOs;

internal sealed record CobrancaGateway(string Id, string AssinaturaId, Guid Referencia, string Status,
  decimal Valor, string Moeda, DateTimeOffset? PagaEm, DateTimeOffset PeriodoInicio, DateTimeOffset AtualizadaEm);
