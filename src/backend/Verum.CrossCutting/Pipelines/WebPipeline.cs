using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class WebPipeline
{
  public const string CorsPolicy = "VerumFrontend";

  public static IServiceCollection AddVerumWeb(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddControllers();

    services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

    services.AddOpenApi();

    services.AddProblemDetails();

    services.Configure<ForwardedHeadersOptions>(options =>
    {
      options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

      foreach (var network in configuration.GetSection("ReverseProxy:KnownNetworks").Get<string[]>() ?? [])
        options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
    });

    var keysPath = configuration["DataProtection:KeysPath"];

    if (!string.IsNullOrWhiteSpace(keysPath))
      services.AddDataProtection()
        .SetApplicationName(configuration["DataProtection:ApplicationName"] ?? "Verum.Api")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

    var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

    if (origins.Contains("*"))
      throw new InvalidOperationException("Cors:Origins deve listar origens explícitas.");

    services.AddCors(cors => cors.AddPolicy(CorsPolicy, policy =>
    {
      if (origins.Length > 0)
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    }));

    return services.AddVerumAutenticacao(configuration);
  }
}
