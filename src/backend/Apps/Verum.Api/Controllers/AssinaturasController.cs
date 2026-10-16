using Microsoft.AspNetCore.Mvc;
using Verum.Modules.Assinaturas.Contratos;
using Verum.Modules.Assinaturas.Contratos.DTOs;
using Verum.Modules.Contas.Contratos;

namespace Verum.Api.Controllers;

public sealed class AssinaturasController(IAssinaturasService assinaturas, IContaService contas) : AppController
{
  [HttpGet("planos")]
  public async Task<ActionResult<IReadOnlyList<PlanoConsulta>>> Planos(CancellationToken cancellationToken) =>
    Ok(await assinaturas.ListarPlanosAsync(cancellationToken));

  [HttpPost]
  public async Task<ActionResult<AssinaturaConsulta>> Criar(SolicitarAssinatura pedido, CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await assinaturas.CriarAsync(conta.Id, ObterEmail()!, pedido, cancellationToken));
  }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<AssinaturaConsulta>>> Listar(CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await assinaturas.ListarAsync(conta.Id, cancellationToken));
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<AssinaturaConsulta>> Obter(Guid id, CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await assinaturas.ObterAsync(conta.Id, id, cancellationToken));
  }

  [HttpPost("{id:guid}/cancelamento")]
  public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
  {
    var conta = await SincronizarContaAsync(contas, cancellationToken);

    await assinaturas.CancelarAsync(conta.Id, id, cancellationToken);

    return Accepted();
  }
}
