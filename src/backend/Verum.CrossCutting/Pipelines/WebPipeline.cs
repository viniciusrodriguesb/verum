using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class WebPipeline
{
  public const string CorsPolicy = "VerumFrontend";

  public static IServiceCollection AddVerumWeb(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddControllers();

    services.AddOpenApi();

    services.AddProblemDetails();

    var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

    if (origins.Contains("*"))
      throw new InvalidOperationException("Cors:Origins deve listar origens explícitas.");

    services.AddCors(cors => cors.AddPolicy(CorsPolicy, policy =>
    {
      if (origins.Length > 0)
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    }));

    return services.AddVerumAutenticacao(configuration);
  }
}

