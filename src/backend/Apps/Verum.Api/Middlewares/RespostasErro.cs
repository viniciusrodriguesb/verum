using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Verum.Api.Middlewares;

internal static class RespostasErro
{
  internal static void Completar(HttpContext context, ProblemDetails problem)
  {
    var status = problem.Status ?? context.Response.StatusCode;

    problem.Status = status;

    problem.Title = ReasonPhrases.GetReasonPhrase(status);

    problem.Type = "about:blank";

    problem.Instance = context.Request.Path;

    problem.Extensions.TryAdd("code", status switch
    {
      400 => "REQUISICAO_INVALIDA",
      401 => "NAO_AUTENTICADO",
      403 => "ACESSO_NEGADO",
      404 => "NAO_ENCONTRADO",
      405 => "METODO_NAO_PERMITIDO",
      415 => "CONTEUDO_NAO_SUPORTADO",
      _ => $"HTTP_{status}"
    });

    problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
  }
}
