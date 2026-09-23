using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.BuildingBlocks.InteligenciaArtificial;
using Verum.CrossCutting.InteligenciaArtificial;

namespace Verum.CrossCutting.Pipelines;

public static class InteligenciaArtificialPipeline
{
  public static IServiceCollection AddVerumInteligenciaArtificial(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<InteligenciaArtificialOptions>()
      .Bind(configuration.GetSection("InteligenciaArtificial"))
      .Validate(x => !string.IsNullOrWhiteSpace(x.ProvedorPadrao), "Informe o provedor padrão de IA.")
      .ValidateOnStart();

    services.AddOptions<OpenAiOptions>()
      .Bind(configuration.GetSection("InteligenciaArtificial:OpenAI"))
      .Validate(x => x.MaxOutputTokens is >= 256 and <= 32000 && x.MaxToolCalls is >= 1 and <= 10,
        "Limites de IA inválidos.")
      .Validate(x => !string.IsNullOrWhiteSpace(x.Modelo), "Informe o modelo OpenAI.")
      .ValidateOnStart();

    services.AddHttpClient(OpenAiProvedor.ClienteHttp)
      .ConfigureHttpClient(client => client.MaxResponseContentBufferSize = 2 * 1024 * 1024)
      .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

    services.AddScoped<IProvedorIa, OpenAiProvedor>();

    services.AddScoped<IConsultaIa, ConsultaIaService>();

    return services;
  }
}
