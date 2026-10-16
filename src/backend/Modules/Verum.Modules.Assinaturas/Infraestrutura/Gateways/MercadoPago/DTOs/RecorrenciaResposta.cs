namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.DTOs;

internal sealed record RecorrenciaResposta(decimal TransactionAmount, string CurrencyId, int Frequency, string FrequencyType);
