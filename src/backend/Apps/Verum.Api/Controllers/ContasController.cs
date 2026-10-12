using Microsoft.AspNetCore.Mvc;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Contas.Contratos.DTOs;

namespace Verum.Api.Controllers;

public sealed class ContasController(IContaService contas) : AppController
{
  [HttpPost("me")]
  public async Task<ActionResult<ContaAutenticada>> Sincronizar(CancellationToken cancellationToken)
  {
    var email = ObterEmail();

    if (!EmailConfirmado() || string.IsNullOrWhiteSpace(email))
      throw new ErroAplicacaoException("EMAIL_NAO_CONFIRMADO", "Confirme seu e-mail para acessar sua conta.", TipoErro.AcessoNegado);

    Response.Headers.CacheControl = "no-store";

    return Ok(await contas.SincronizarAsync(ObterSujeitoIdentidade(), email,
      ObterNome() ?? ObterUsername() ?? email, cancellationToken));
  }
}
