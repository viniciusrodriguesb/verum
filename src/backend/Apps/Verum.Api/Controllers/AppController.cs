using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Contas.Contratos.DTOs;

namespace Verum.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public abstract class AppController : ControllerBase
{
  protected string ObterSujeitoIdentidade() =>
    User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("Identidade ausente no token.");

  protected string? ObterEmail() => User.FindFirst("email")?.Value;

  protected string? ObterUsername() => User.FindFirst("preferred_username")?.Value;

  protected string? ObterNome() => User.FindFirst("name")?.Value;

  protected bool EmailConfirmado() => User.FindFirst("email_verified")?.Value == "true";

  protected Task<ContaAutenticada> SincronizarContaAsync(IContaService contas, CancellationToken cancellationToken)
  {
    var email = ObterEmail();

    if (!EmailConfirmado() || string.IsNullOrWhiteSpace(email))
      throw new ErroAplicacaoException("EMAIL_NAO_CONFIRMADO", "Confirme seu e-mail para acessar sua conta.", TipoErro.AcessoNegado);

    return contas.SincronizarAsync(ObterSujeitoIdentidade(), email, ObterNome() ?? ObterUsername() ?? email, cancellationToken);
  }
}
