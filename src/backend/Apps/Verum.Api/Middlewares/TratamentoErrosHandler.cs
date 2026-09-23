using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Timeout;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.InteligenciaArtificial;

namespace Verum.Api.Middlewares;

// Executado pelo middleware nativo UseExceptionHandler, inclusive em Development.
public sealed class TratamentoErrosHandler(ILogger<TratamentoErrosHandler> logger) : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
  {
    if (context.Response.HasStarted) return false;

    if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
    {
      context.Response.StatusCode = 499;

      return true;
    }

    var (status, codigo, mensagem) = exception switch
    {
      ErroAplicacaoException erro => (erro.Tipo switch
      {
        TipoErro.Validacao => 400,
        TipoErro.NaoEncontrado => 404,
        TipoErro.Conflito => 409,
        TipoErro.AcessoNegado => 403,
        _ => 500
      }, erro.Codigo, erro.Message),
      BadHttpRequestException erro => (erro.StatusCode, "REQUISICAO_INVALIDA", "A requisição não pôde ser processada."),
      TimeoutRejectedException => (504, "DEPENDENCIA_TIMEOUT", "Um serviço externo excedeu o tempo de resposta."),
      BrokenCircuitException => (503, "DEPENDENCIA_INDISPONIVEL", "Um serviço externo está temporariamente indisponível."),
      RateLimiterRejectedException => (503, "DEPENDENCIA_OCUPADA", "O limite de chamadas simultâneas ao serviço externo foi atingido."),
      HttpRequestException => (502, "DEPENDENCIA_FALHOU", "Não foi possível concluir a comunicação com um serviço externo."),
      RespostaIaInvalidaException => (502, "IA_RESPOSTA_INVALIDA", "O serviço de IA não retornou uma resposta utilizável."),
      _ => (500, "ERRO_INTERNO", "Não foi possível concluir a operação. Informe o traceId ao suporte.")
    };

    var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

    if (status >= 500)
      logger.LogError(exception, "Falha na API. Código {Codigo}, TraceId {TraceId}", codigo, traceId);
    else
      logger.LogWarning("Requisição rejeitada. Código {Codigo}, TraceId {TraceId}", codigo, traceId);

    var problem = new ProblemDetails { Status = status, Detail = mensagem };

    problem.Extensions["code"] = codigo;

    if (exception is ErroAplicacaoException { Campo: not null } validation)
      problem.Extensions["errors"] = new Dictionary<string, string[]> { [validation.Campo] = [validation.Message] };

    RespostasErro.Completar(context, problem);

    context.Response.StatusCode = status;

    await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);

    return true;
  }
}
