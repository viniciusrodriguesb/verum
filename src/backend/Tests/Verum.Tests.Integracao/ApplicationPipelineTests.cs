extern alias VerumApi;

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class ApplicationPipelineTests
{
  [Fact]
  public async Task ApiExpoeHealthOpenApiECorsEmDesenvolvimento()
  {
    // Evita carregar as conexões do Docker antes de ConfigureAppConfiguration
    // ser aplicado pelo WebApplicationFactory ao entry point minimal hosting.
    var contentRoot = Path.Combine(AppContext.BaseDirectory, "ApiIsolada");
    Directory.CreateDirectory(contentRoot);
    await File.WriteAllTextAsync(Path.Combine(contentRoot, "appsettings.json"), """
      {
        "RabbitMQ": { "Enabled": false },
        "Redis": { "Enabled": false },
        "Http": { "Resilience": { "Retry": { "MaxRetryAttempts": 3 } } },
        "Authentication": {
          "Authority": "http://localhost:8080/realms/verum",
          "Audience": "verum-api",
          "RequireHttpsMetadata": false
        },
        "Cors": { "Origins": [ "http://localhost:4200" ] }
      }
      """);
    await using var factory = new WebApplicationFactory<VerumApi::Program>().WithWebHostBuilder(builder =>
    {
      builder.UseContentRoot(contentRoot);
      builder.UseEnvironment("Development");
      builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Authentication:Authority"] = "http://localhost:8080/realms/verum",
        ["Authentication:Audience"] = "verum-api",
        ["Authentication:RequireHttpsMetadata"] = "false",
        ["RabbitMQ:Enabled"] = "false",
        ["Redis:Enabled"] = "false",
        ["Cors:Origins:0"] = "http://localhost:4200"
      }));
    });
    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/openapi/v1.json")).StatusCode);
    using var preflight = new HttpRequestMessage(HttpMethod.Options, "/health/live");
    preflight.Headers.Add("Origin", "http://localhost:4200");
    preflight.Headers.Add("Access-Control-Request-Method", "GET");
    using var response = await client.SendAsync(preflight);
    Assert.Contains("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin"));
  }
}
