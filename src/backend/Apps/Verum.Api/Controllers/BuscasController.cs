using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Contratos.DTOs;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Contas.Contratos;

namespace Verum.Api.Controllers;

[AllowAnonymous]
public sealed class BuscasController(IBuscaService buscas, IDataProtectionProvider protection,
  IContaService contas, IAcessoBuscaService acesso) : AppController
{
  private const string CookieNome = "verum.visitante";

  [HttpGet("visitante")]
  public IActionResult PrepararVisitante()
  {
    if (User.Identity?.IsAuthenticated != true && Request.Headers.ContainsKey("Authorization"))
      throw new UnauthorizedAccessException("Sessão inválida.");

    if (User.Identity?.IsAuthenticated != true) Visitante(criar: true);

    Response.Headers.CacheControl = "no-store";

    return NoContent();
  }

  [HttpPost]
  public async Task<IActionResult> Solicitar([FromBody] SolicitarBusca pedido, CancellationToken cancellationToken)
  {
    var resultado = await buscas.SolicitarAsync(await ObterAtorAsync(true, cancellationToken), pedido, cancellationToken);

    return AcceptedAtAction(nameof(Obter), new { id = resultado.Id }, resultado);
  }

  [HttpGet("{id:guid}")]
  [HttpGet("{id:guid}/resultado")]
  public async Task<ActionResult<BuscaConsulta>> Obter(Guid id, CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    return Ok(await buscas.ObterAsync(await ObterAtorAsync(false, cancellationToken), id, cancellationToken));
  }

  [HttpGet("historico")]
  public async Task<ActionResult<IReadOnlyList<BuscaHistorico>>> ObterHistorico(CancellationToken cancellationToken)
  {
    if (User.Identity?.IsAuthenticated != true) return Unauthorized();

    Response.Headers.CacheControl = "no-store";

    return Ok(await buscas.ObterHistoricoAsync(await ObterAtorAsync(false, cancellationToken), cancellationToken));
  }

  [HttpGet("acesso")]
  public async Task<ActionResult<AcessoBusca>> ObterAcesso(CancellationToken cancellationToken)
  {
    var ator = await ObterAtorAsync(true, cancellationToken);

    await acesso.PrepararAsync(ator, cancellationToken);

    Response.Headers.CacheControl = "no-store";

    return Ok(await acesso.ObterAsync(ator, cancellationToken));
  }

  private async Task<AtorBusca> ObterAtorAsync(bool criarVisitante, CancellationToken cancellationToken)
  {
    if (User.Identity?.IsAuthenticated == true)
    {
      var conta = await SincronizarContaAsync(contas, cancellationToken);

      return AtorBusca.Conta(conta.Id);
    }

    if (Request.Headers.ContainsKey("Authorization")) throw new UnauthorizedAccessException("Sessão inválida.");

    var visitante = Visitante(criarVisitante);

    if (visitante == Guid.Empty)
      throw new Verum.BuildingBlocks.Erros.ErroAplicacaoException("BUSCA_NAO_ENCONTRADA", "Busca não encontrada.", Verum.BuildingBlocks.Erros.TipoErro.NaoEncontrado);

    return AtorBusca.Visitante(visitante);
  }

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
