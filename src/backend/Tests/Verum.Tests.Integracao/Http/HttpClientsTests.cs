using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Verum.CrossCutting.Pipelines;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class HttpClientsTests
{
  [Theory]
  [InlineData("GET", 3)]
  [InlineData("POST", 1)]
  public async Task HttpRepeteSomenteMetodosSeguros(string method, int expectedAttempts)
  {
    var handler = new FalhaTemporariaHandler();

    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:Resilience:Retry:MaxRetryAttempts"] = "2",
      ["Http:Resilience:Retry:Delay"] = "00:00:00.001",
      ["Http:Resilience:Retry:UseJitter"] = "false"
    }));

    services.AddHttpClient("teste").ConfigurePrimaryHttpMessageHandler(() => handler);

    using var provider = services.BuildServiceProvider();

    using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("teste");

    using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "https://verum.invalid/"));

    Assert.Equal(expectedAttempts, handler.Attempts);

    Assert.Equal(method == "GET" ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable, response.StatusCode);
  }

  private sealed class FalhaTemporariaHandler : HttpMessageHandler
  {
    public int Attempts { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Attempts++;

      return Task.FromResult(new HttpResponseMessage(Attempts < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
    }
  }
}

