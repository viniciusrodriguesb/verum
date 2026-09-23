extern alias VerumApi;

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Verum.Api.Middlewares;
using Verum.BuildingBlocks.Erros;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class TratamentoErrosTests
{
  [Theory]
  [InlineData("dominio", 400, "VALIDACAO")]
  [InlineData("conflito", 409, "CONFLITO_TESTE")]
  [InlineData("ausente", 404, "REGISTRO_AUSENTE")]
  [InlineData("negado", 403, "OPERACAO_NEGADA")]
  [InlineData("timeout", 504, "DEPENDENCIA_TIMEOUT")]
  [InlineData("circuito", 503, "DEPENDENCIA_INDISPONIVEL")]
  [InlineData("http", 502, "DEPENDENCIA_FALHOU")]
  [InlineData("interno", 500, "ERRO_INTERNO")]
  [InlineData("argumento", 500, "ERRO_INTERNO")]
  public async Task MiddlewareClassificaFalhasSemExporDetalhesInternos(string tipo, int status, string codigo)
  {
    await using var factory = CriarApi();

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    using var response = await client.GetAsync($"/__testes/erros/{tipo}?segredo=nao-expor");

    var body = await response.Content.ReadAsStringAsync();

    Assert.Equal(status, (int)response.StatusCode);

    Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);

    Assert.DoesNotContain("segredo", body);

    Assert.DoesNotContain("stackTrace", body);

    using var json = JsonDocument.Parse(body);

    Assert.Equal(codigo, json.RootElement.GetProperty("code").GetString());

    Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));

    Assert.Equal($"/__testes/erros/{tipo}", json.RootElement.GetProperty("instance").GetString());

    if (tipo == "dominio") Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("Nome", out _));
  }

  [Fact]
  public async Task ModelBindingE404UsamMesmoContratoDeErro()
  {
    await using var factory = CriarApi();

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    using var validation = await client.PostAsJsonAsync("/__testes/erros", new { Nome = "", Quantidade = "segredo-recebido" });

    var body = await validation.Content.ReadAsStringAsync();

    Assert.Equal(400, (int)validation.StatusCode);

    Assert.DoesNotContain("segredo-recebido", body);

    using var json = JsonDocument.Parse(body);

    Assert.Equal("VALIDACAO", json.RootElement.GetProperty("code").GetString());

    Assert.True(json.RootElement.TryGetProperty("errors", out _));

    using var notFound = await client.GetAsync("/__testes/nao-existe");

    using var missing = JsonDocument.Parse(await notFound.Content.ReadAsStringAsync());

    Assert.Equal(404, (int)notFound.StatusCode);

    Assert.Equal("NAO_ENCONTRADO", missing.RootElement.GetProperty("code").GetString());
  }

  [Fact]
  public async Task CancelamentoDoClienteNaoEscreveRespostaDeErro()
  {
    var context = new DefaultHttpContext();

    context.Response.Body = new MemoryStream();

    context.RequestAborted = new CancellationToken(true);

    var handler = new TratamentoErrosHandler(NullLogger<TratamentoErrosHandler>.Instance);

    Assert.True(await handler.TryHandleAsync(context, new OperationCanceledException(), CancellationToken.None));

    Assert.Equal(499, context.Response.StatusCode);

    Assert.Equal(0, context.Response.Body.Length);
  }

  private static WebApplicationFactory<VerumApi::Program> CriarApi()
  {
    var root = Path.Combine(AppContext.BaseDirectory, "ApiErrosIsolada");

    Directory.CreateDirectory(root);

    File.WriteAllText(Path.Combine(root, "appsettings.json"), """
      {
        "Redis": { "Enabled": false },
        "RabbitMQ": { "Enabled": false },
        "Authentication": { "Authority": "https://identity.invalid/realms/test", "Audience": "verum-api" }
      }
      """);

    return new WebApplicationFactory<VerumApi::Program>().WithWebHostBuilder(builder =>
    {
      builder.UseContentRoot(root);

      builder.UseEnvironment("Development");

      builder.ConfigureServices(services => services.AddControllers().AddApplicationPart(typeof(ErrosTesteController).Assembly));
    });
  }
}

// Disponível apenas no host de teste, nunca registrado pela API de produção.
[ApiController]
[Route("__testes/erros")]
public sealed class ErrosTesteController : ControllerBase
{
  [HttpGet("{tipo}")]
  public IActionResult Get(string tipo)
  {
    if (tipo == "dominio")
    {
      _ = new Modules.Catalogo.Dominio.Categoria(" ", "teste");

      return Ok();
    }

    throw tipo switch
    {
      "conflito" => new ErroAplicacaoException("CONFLITO_TESTE", "Registro já existente.", TipoErro.Conflito),
      "ausente" => new ErroAplicacaoException("REGISTRO_AUSENTE", "Registro não encontrado.", TipoErro.NaoEncontrado),
      "negado" => new ErroAplicacaoException("OPERACAO_NEGADA", "Operação não permitida.", TipoErro.AcessoNegado),
      "timeout" => new TimeoutRejectedException("segredo"),
      "circuito" => new BrokenCircuitException("segredo"),
      "http" => new HttpRequestException("segredo"),
      "argumento" => new ArgumentException("segredo"),
      _ => new InvalidOperationException("segredo")
    };
  }

  [HttpPost]
  public IActionResult Post(EntradaTeste entrada) => Ok(entrada);
}

public sealed record EntradaTeste([Required] string Nome, int Quantidade);
