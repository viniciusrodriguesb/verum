using Microsoft.AspNetCore.Mvc;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Notificacoes.Contratos.DTOs;

namespace Verum.Api.Controllers;

public sealed class NotificacoesController(INotificacoesRadarService notificacoes, IContaService contas) : AppController
{
  [HttpGet]
  public async Task<IActionResult> Listar(CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await notificacoes.ListarAsync(conta.Id, cancellationToken));
  }

  [HttpGet("preferencias")]
  public async Task<ActionResult<PreferenciasAviso>> Preferencias(CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    return Ok(await notificacoes.PreferenciasAsync(conta.Id, cancellationToken));
  }

  [HttpPut("preferencias")]
  public async Task<IActionResult> Salvar(PreferenciasAviso pedido, CancellationToken cancellationToken)
  {
    var conta = await SincronizarContaAsync(contas, cancellationToken);

    await notificacoes.SalvarPreferenciasAsync(conta.Id, pedido, cancellationToken);

    return NoContent();
  }
}
