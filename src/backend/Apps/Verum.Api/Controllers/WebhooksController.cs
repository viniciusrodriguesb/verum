using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verum.Modules.Assinaturas.Contratos;

namespace Verum.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/webhooks")]
public sealed class WebhooksController(IWebhookPagamentoService webhooks) : ControllerBase
{
  [HttpPost("mercadopago")]
  [RequestSizeLimit(16384)]
  public async Task<IActionResult> MercadoPago([FromQuery(Name = "data.id")] string recursoId,
    [FromBody] JsonElement corpo, CancellationToken cancellationToken)
  {
    if (corpo.ValueKind != JsonValueKind.Object || !corpo.TryGetProperty("type", out var tipo)
      || tipo.ValueKind != JsonValueKind.String || !corpo.TryGetProperty("data", out var dados)
      || dados.ValueKind != JsonValueKind.Object || !dados.TryGetProperty("id", out var id)
      || id.ToString() != recursoId) return BadRequest();

    await webhooks.ReceberAsync("MercadoPago", recursoId, tipo.GetString()!,
      Request.Headers["x-signature"].ToString(), Request.Headers["x-request-id"].ToString(), cancellationToken);

    return Ok();
  }
}
