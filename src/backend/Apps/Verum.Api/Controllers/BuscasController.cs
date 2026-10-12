using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.DTOs;

namespace Verum.Api.Controllers;

[AllowAnonymous]
public sealed class BuscasController(IBuscaService buscas, IDataProtectionProvider protection) : AppController
{
  private const string CookieNome = "verum.visitante";

  [HttpGet("visitante")]
  public IActionResult PrepararVisitante()
  {
    Visitante(criar: true);

    Response.Headers.CacheControl = "no-store";

    return NoContent();
  }

  [HttpPost]
  public async Task<IActionResult> Solicitar([FromBody] SolicitarBusca pedido, CancellationToken cancellationToken)
  {
    var resultado = await buscas.SolicitarAsync(Visitante(criar: true), pedido, cancellationToken);

    return AcceptedAtAction(nameof(Obter), new { id = resultado.Id }, resultado);
  }

  [HttpGet("{id:guid}")]
  [HttpGet("{id:guid}/resultado")]
  public async Task<ActionResult<BuscaConsulta>> Obter(Guid id, CancellationToken cancellationToken) =>
    Ok(await buscas.ObterAsync(Visitante(criar: false), id, cancellationToken));

  private Guid Visitante(bool criar)
  {
    var protetor = protection.CreateProtector("Verum.Busca.Visitante.v1").ToTimeLimitedDataProtector();

    if (Request.Cookies.TryGetValue(CookieNome, out var cookie))
    {
      try
      {
        if (Guid.TryParse(protetor.Unprotect(cookie), out var id) && id != Guid.Empty) return id;
      }
      catch (System.Security.Cryptography.CryptographicException) { }
    }

    if (!criar) return Guid.Empty;

    var visitante = Guid.CreateVersion7();

    Response.Cookies.Append(CookieNome, protetor.Protect(visitante.ToString(), TimeSpan.FromDays(30)), new CookieOptions
    {
      HttpOnly = true,
      Secure = Request.IsHttps,
      SameSite = SameSiteMode.Strict,
      MaxAge = TimeSpan.FromDays(30),
      IsEssential = true,
      Path = "/api/v1/buscas"
    });

    return visitante;
  }
}
