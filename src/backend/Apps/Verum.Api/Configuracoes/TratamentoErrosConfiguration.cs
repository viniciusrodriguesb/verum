using Microsoft.AspNetCore.Mvc;
using Verum.Api.Middlewares;

namespace Verum.Api.Configuracoes;

public static class TratamentoErrosConfiguration
{
  public static IServiceCollection AddVerumTratamentoErros(this IServiceCollection services)
  {
    services.AddExceptionHandler<TratamentoErrosHandler>();

    services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
      RespostasErro.Completar(context.HttpContext, context.ProblemDetails));

    services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
    {
      // Mensagens do model binder podem conter valores recebidos: não os devolver ao cliente.
      var errors = context.ModelState.Where(entry => entry.Value?.Errors.Count > 0)
        .ToDictionary(entry => entry.Key, _ => new[] { "Valor inválido ou obrigatório não informado." });

      var problem = new ValidationProblemDetails(errors)
      {
        Status = 400,
        Detail = "Verifique os campos informados."
      };

      problem.Extensions["code"] = "VALIDACAO";

      RespostasErro.Completar(context.HttpContext, problem);

      var result = new BadRequestObjectResult(problem);

      result.ContentTypes.Add("application/problem+json");

      return result;
    });

    return services;
  }
}
