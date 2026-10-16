namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.DTOs;

internal sealed record FaturaResposta(long Id, string PreapprovalId, string ExternalReference, string CurrencyId,
  decimal TransactionAmount, DateTimeOffset? DebitDate, PagamentoFaturaResposta? Payment);
