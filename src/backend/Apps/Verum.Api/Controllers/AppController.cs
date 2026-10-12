using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
}
