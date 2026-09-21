using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Verum.CrossCutting.Pipelines;

public static class ObservabilidadePipeline
{
  public static IServiceCollection AddVerumObservabilidade(this IServiceCollection services, IConfiguration configuration, string serviceName)
  {
    var export = configuration.GetValue<bool>("Observability:OtlpEnabled");

    var telemetry = services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService(serviceName));

    telemetry.WithTracing(trace =>
    {
      trace.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource("MassTransit");
      if (export) trace.AddOtlpExporter();
    });
    telemetry.WithMetrics(metrics =>
    {
      metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
        .AddMeter("MassTransit", "System.Net.Http", "Microsoft.AspNetCore.Hosting");
      if (export) metrics.AddOtlpExporter();
    });
    services.AddLogging(logging => logging.AddOpenTelemetry(options =>
    {
      options.IncludeScopes = true;
      options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
      if (export) options.AddOtlpExporter();
    }));

    services.AddHealthChecks();

    return services;
  }
}

