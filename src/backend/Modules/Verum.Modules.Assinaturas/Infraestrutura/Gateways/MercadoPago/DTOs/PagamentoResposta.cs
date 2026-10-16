namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.DTOs;

internal sealed record PagamentoResposta(long Id, string Status, string ExternalReference, decimal TransactionAmount,
  decimal TransactionAmountRefunded, string CurrencyId, long CollectorId, bool LiveMode,
  DateTimeOffset? DateApproved, DateTimeOffset? DateLastUpdated, DateTimeOffset DateCreated);
