using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Verum.CrossCutting.Pipelines;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class HttpClientsTests
{
  [Fact]
  public async Task TimeoutTotalLimitaConjuntoDeTentativas()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:Resilience:TotalRequestTimeout:Timeout"] = "00:00:02",
      ["Http:Resilience:AttemptTimeout:Timeout"] = "00:00:01",
      ["Http:Resilience:Retry:MaxRetryAttempts"] = "5",
      ["Http:Resilience:Retry:Delay"] = "00:00:00.001"
    }));

    services.AddHttpClient("total").ConfigurePrimaryHttpMessageHandler(() => new LentoHandler());

    using var provider = services.BuildServiceProvider();

    using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("total");

    var exception = await Assert.ThrowsAsync<TimeoutRejectedException>(() => client.GetAsync("https://verum.invalid/"));

    Assert.Equal(TimeSpan.FromSeconds(2), exception.Timeout);
  }

  [Fact]
  public async Task ClientePodeDesativarRetryECircuitoSemDesativarTimeouts()
  {
    var handler = new RespostaHandler(HttpStatusCode.ServiceUnavailable);

    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:Resilience:CircuitBreaker:MinimumThroughput"] = "2",
      ["Http:Clients:sem-retry:RetryEnabled"] = "false",
      ["Http:Clients:sem-retry:CircuitBreakerEnabled"] = "false"
    }));

    services.AddHttpClient("sem-retry").ConfigurePrimaryHttpMessageHandler(() => handler);

    using var provider = services.BuildServiceProvider();

    using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("sem-retry");

    for (var tentativa = 0; tentativa < 4; tentativa++)
    {
      using var response = await client.GetAsync("https://verum.invalid/");

      Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    Assert.Equal(4, handler.Attempts);
  }

  [Fact]
  public async Task ConfiguracaoPorClienteSobrescrevePadraoSemEmpilharRetries()
  {
    var handler = new RespostaHandler(HttpStatusCode.ServiceUnavailable);

    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:Resilience:Retry:MaxRetryAttempts"] = "4",
      ["Http:Resilience:Retry:Delay"] = "00:00:00.001",
      ["Http:Clients:especial:Resilience:Retry:MaxRetryAttempts"] = "1"
    }));

    services.AddHttpClient("especial").ConfigurePrimaryHttpMessageHandler(() => handler);

    using var provider = services.BuildServiceProvider();

    using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("especial");

    using var response = await client.GetAsync("https://verum.invalid/");

    Assert.Equal(2, handler.Attempts);
  }

  [Fact]
  public async Task CircuitoAbertoFalhaRapidoESomenteAfetaOClienteCorrespondente()
  {
    var handler = new RespostaHandler(HttpStatusCode.ServiceUnavailable);

    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:RetryEnabled"] = "false",
      ["Http:Resilience:CircuitBreaker:MinimumThroughput"] = "2",
      ["Http:Resilience:CircuitBreaker:FailureRatio"] = "0.5"
    }));

    services.AddHttpClient("falhando").ConfigurePrimaryHttpMessageHandler(() => handler);

    services.AddHttpClient("outro").ConfigurePrimaryHttpMessageHandler(() => new RespostaHandler(HttpStatusCode.OK));

    using var provider = services.BuildServiceProvider();

    var factory = provider.GetRequiredService<IHttpClientFactory>();

    using var client = factory.CreateClient("falhando");

    using var first = await client.GetAsync("https://verum.invalid/");

    using var second = await client.GetAsync("https://verum.invalid/");

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() => client.GetAsync("https://verum.invalid/"));

    Assert.Equal(2, handler.Attempts);

    using var other = factory.CreateClient("outro");

    using var response = await other.GetAsync("https://verum.invalid/");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Theory]
  [InlineData(400, 1)]
  [InlineData(401, 1)]
  [InlineData(429, 3)]
  [InlineData(500, 3)]
  public async Task SomenteFalhasTransitoriasSaoRepetidas(int status, int tentativas)
  {
    var handler = new RespostaHandler((HttpStatusCode)status);

    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:Resilience:Retry:MaxRetryAttempts"] = "2",
      ["Http:Resilience:Retry:Delay"] = "00:00:00.001"
    }));

    services.AddHttpClient("teste").ConfigurePrimaryHttpMessageHandler(() => handler);

    using var provider = services.BuildServiceProvider();

    using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("teste");

    using var response = await client.GetAsync("https://verum.invalid/");

    Assert.Equal(tentativas, handler.Attempts);
  }

  [Fact]
  public async Task TimeoutDaTentativaECancelamentoDoChamadorSaoPreservados()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddVerumHttpClients(ConfiguracaoTests.Config(new()
    {
      ["Http:RetryEnabled"] = "false",
      ["Http:Resilience:AttemptTimeout:Timeout"] = "00:00:01"
    }));

    services.AddHttpClient("lento").ConfigurePrimaryHttpMessageHandler(() => new LentoHandler());

    using var provider = services.BuildServiceProvider();

    using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("lento");

    await Assert.ThrowsAsync<TimeoutRejectedException>(() => client.GetAsync("https://verum.invalid/"));

    using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetAsync("https://verum.invalid/", cancellation.Token));
  }

  [Theory]
  [InlineData("GET", 3)]
  [InlineData("POST", 1)]
  [InlineData("PUT", 1)]
  [InlineData("PATCH", 1)]
  [InlineData("DELETE", 1)]
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

  private sealed class RespostaHandler(HttpStatusCode status) : HttpMessageHandler
  {
    public int Attempts { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Attempts++;

      return Task.FromResult(new HttpResponseMessage(status));
    }
  }

  private sealed class LentoHandler : HttpMessageHandler
  {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

      return new HttpResponseMessage(HttpStatusCode.OK);
    }
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
