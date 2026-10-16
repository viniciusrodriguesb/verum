using Microsoft.AspNetCore.Mvc;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Contas.Contratos.DTOs;

namespace Verum.Api.Controllers;

public sealed class ContasController(IContaService contas, IAcessoBuscaService acesso) : AppController
{
  [HttpPost("me")]
  public async Task<ActionResult<ContaAutenticada>> Sincronizar(CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl = "no-store";

    var conta = await SincronizarContaAsync(contas, cancellationToken);

    await acesso.PrepararAsync(AtorBusca.Conta(conta.Id), cancellationToken);

    return Ok(conta);
  }
}
