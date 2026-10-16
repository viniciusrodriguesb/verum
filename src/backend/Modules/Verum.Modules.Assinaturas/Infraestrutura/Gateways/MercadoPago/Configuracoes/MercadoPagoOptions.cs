namespace Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.Configuracoes;

public sealed class MercadoPagoOptions
{
  public string AccessToken { get; set; } = "";

  public string WebhookSecret { get; set; } = "";

  public long CollectorId { get; set; }

  public bool Producao { get; set; }

  public string UrlRetorno { get; set; } = "";

  public string UrlWebhook { get; set; } = "";
}
