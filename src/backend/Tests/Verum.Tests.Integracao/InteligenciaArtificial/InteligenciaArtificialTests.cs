using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.BuildingBlocks.InteligenciaArtificial;
using Verum.CrossCutting.InteligenciaArtificial;
using Verum.CrossCutting.Pipelines;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class InteligenciaArtificialTests
{
  private const string UrlProduto = "https://loja.example/produto";

  [Fact]
  public async Task PesquisaWebEnviaSchemaEDevolveCandidatosComMetadados()
  {
    var handler = new RespostaHandler(Envelope(ProdutoJson()));

    using var provider = CriarProvider(handler);

    using var scope = provider.CreateScope();

    var service = scope.ServiceProvider.GetRequiredService<IDescobertaProdutosIaService>();

    var resultado = await service.ConsultarAsync("Monitor 27 polegadas");

    Assert.Single(resultado.Resultado.Produtos);

    Assert.Equal(1299.90m, resultado.Resultado.Produtos[0].Preco);

    Assert.Equal("resp_teste", resultado.Execucao.IdResposta);

    Assert.Equal(100L, resultado.Execucao.TokensEntrada);

    Assert.Single(resultado.Execucao.Fontes);

    Assert.Equal("Bearer chave-teste", handler.Authorization);

    var body = handler.Corpo!;

    Assert.Equal("required", body["tool_choice"]!.GetValue<string>());

    Assert.Equal("web_search", body["tools"]![0]!["type"]!.GetValue<string>());

    Assert.True(body["text"]!["format"]!["strict"]!.GetValue<bool>());

    Assert.False(body["store"]!.GetValue<bool>());

    Assert.Contains("Verum", body["instructions"]!.GetValue<string>());

    Assert.Contains("Monitor 27 polegadas", body["input"]!.GetValue<string>());
  }

  [Theory]
  [InlineData("incomplete", "RESPOSTA_INCOMPLETA")]
  [InlineData("failed", "RESPOSTA_INCOMPLETA")]
  [InlineData("refusal", "RECUSA")]
  [InlineData("sem-web", "PESQUISA_NAO_EXECUTADA")]
  [InlineData("json", "JSON_INVALIDO")]
  public async Task FalhasDoProvedorNaoViramListaVazia(string cenario, string codigo)
  {
    var envelope = Envelope(ProdutoJson());

    if (cenario is "incomplete" or "failed") envelope["status"] = cenario;

    if (cenario == "refusal")
      envelope["output"]![1]!["content"] = new JsonArray(new JsonObject { ["type"] = "refusal", ["refusal"] = "segredo" });

    if (cenario == "sem-web") envelope["output"]!.AsArray().RemoveAt(0);

    if (cenario == "json") envelope["output"]![1]!["content"]![0]!["text"] = "não é JSON";

    using var provider = CriarProvider(new RespostaHandler(envelope));

    var ex = await Assert.ThrowsAsync<RespostaIaInvalidaException>(() =>
      provider.GetRequiredService<IConsultaIa>().ConsultarAsync(ConsultaGenerica()));

    Assert.Equal(codigo, ex.Codigo);

    Assert.DoesNotContain("segredo", ex.Message);
  }

  [Theory]
  [InlineData("sem-fonte")]
  [InlineData("preco-negativo")]
  [InlineData("moeda")]
  [InlineData("parcela")]
  [InlineData("propriedade-extra")]
  [InlineData("propriedade-ausente")]
  [InlineData("null")]
  [InlineData("url-local")]
  public async Task RejeitaCandidatoInconsistente(string cenario)
  {
    var json = ProdutoJson();

    var produto = json["produtos"]![0]!;

    if (cenario == "sem-fonte") produto["url"] = "https://outra.example/produto";

    if (cenario == "preco-negativo") produto["preco"] = -1;

    if (cenario == "moeda") produto["moeda"] = "USD";

    if (cenario == "parcela") produto["quantidadeParcelas"] = 12;

    if (cenario == "propriedade-extra") produto["inventado"] = true;

    if (cenario == "propriedade-ausente") produto.AsObject().Remove("nome");

    if (cenario == "null") json["produtos"] = null;

    if (cenario == "url-local") produto["url"] = "http://127.0.0.1/produto";

    using var provider = CriarProvider(new RespostaHandler(Envelope(json)));

    await Assert.ThrowsAsync<RespostaIaInvalidaException>(() =>
      provider.GetRequiredService<IDescobertaProdutosIaService>().ConsultarAsync("Monitor"));
  }

  [Fact]
  public async Task AusenciaLegitimaDeResultadosExigeMotivo()
  {
    var json = new JsonObject { ["consultaInterpretada"] = "Monitor", ["motivoSemResultado"] = "Nenhuma fonte localizada.", ["produtos"] = new JsonArray() };

    using var provider = CriarProvider(new RespostaHandler(Envelope(json)));

    var result = await provider.GetRequiredService<IDescobertaProdutosIaService>().ConsultarAsync("Monitor");

    Assert.Empty(result.Resultado.Produtos);

    Assert.NotNull(result.Resultado.MotivoSemResultado);
  }

  [Theory]
  [InlineData(401)]
  [InlineData(429)]
  [InlineData(500)]
  public async Task PostNaoRecebeRetryNemOcultaStatus(int status)
  {
    var handler = new RespostaHandler(Envelope(ProdutoJson()), (HttpStatusCode)status);

    using var provider = CriarProvider(handler);

    var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
      provider.GetRequiredService<IConsultaIa>().ConsultarAsync(ConsultaGenerica()));

    Assert.Equal((HttpStatusCode)status, ex.StatusCode);

    Assert.Equal(1, handler.Chamadas);
  }

  [Fact]
  public async Task DesabilitadoNaoChamaHttp()
  {
    var handler = new RespostaHandler(Envelope(ProdutoJson()));

    using var provider = CriarProvider(handler, new() { ["InteligenciaArtificial:Enabled"] = "false" });

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      provider.GetRequiredService<IConsultaIa>().ConsultarAsync(ConsultaGenerica()));

    Assert.Equal(0, handler.Chamadas);
  }

  [Fact]
  public async Task ChaveAusenteNaoChamaHttp()
  {
    var handler = new RespostaHandler(Envelope(ProdutoJson()));

    using var provider = CriarProvider(handler, new() { ["InteligenciaArtificial:OpenAI:ApiKey"] = "" });

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      provider.GetRequiredService<IConsultaIa>().ConsultarAsync(ConsultaGenerica()));

    Assert.Equal(0, handler.Chamadas);
  }

  [Fact]
  public async Task SelecionaOutroProvedorSemModificarServicoDeProdutos()
  {
    var handler = new RespostaHandler(Envelope(ProdutoJson()));

    using var provider = CriarProvider(handler, new() { ["InteligenciaArtificial:Produtos:Provedor"] = "Outro" },
      services => services.AddScoped<IProvedorIa, OutroProvedor>());

    var result = await provider.GetRequiredService<IDescobertaProdutosIaService>().ConsultarAsync("Monitor");

    Assert.Equal("Outro", result.Execucao.Provedor);

    Assert.Equal(0, handler.Chamadas);
  }

  [Fact]
  public async Task ProvedorInexistenteFalhaSemFallbackSilencioso()
  {
    var handler = new RespostaHandler(Envelope(ProdutoJson()));

    using var provider = CriarProvider(handler);

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      provider.GetRequiredService<IConsultaIa>().ConsultarAsync(ConsultaGenerica() with { Provedor = "Ausente" }));

    Assert.Equal(0, handler.Chamadas);
  }

  [Fact]
  public async Task CancelamentoDoChamadorPropaga()
  {
    using var provider = CriarProvider(new LentoHandler());

    using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
      provider.GetRequiredService<IConsultaIa>().ConsultarAsync(ConsultaGenerica(), cancel.Token));
  }

  private static ConsultaIa ConsultaGenerica() => new("Contexto", "Entrada", "teste",
    JsonSerializer.SerializeToElement(new { type = "object", properties = new { }, additionalProperties = false, required = Array.Empty<string>() }), true);

  private static ServiceProvider CriarProvider(HttpMessageHandler handler, Dictionary<string, string?>? ajustes = null, Action<IServiceCollection>? adicionar = null)
  {
    var values = new Dictionary<string, string?>
    {
      ["InteligenciaArtificial:Enabled"] = "true",
      ["InteligenciaArtificial:OpenAI:ApiKey"] = "chave-teste"
    };

    if (ajustes is not null)
      foreach (var item in ajustes) values[item.Key] = item.Value;

    var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    var services = new ServiceCollection().AddLogging();

    services.AddSingleton<IConfiguration>(configuration);

    services.AddVerumHttpClients(configuration);

    services.AddVerumInteligenciaArtificial(configuration);

    services.AddVerumServices(typeof(Verum.Modules.Busca.DependencyInjection).Assembly);

    services.AddHttpClient(OpenAiProvedor.ClienteHttp).ConfigurePrimaryHttpMessageHandler(() => handler);

    adicionar?.Invoke(services);

    return services.BuildServiceProvider();
  }

  private static JsonObject ProdutoJson() => JsonNode.Parse("""
    {
      "consultaInterpretada": "Monitor 27 polegadas",
      "motivoSemResultado": null,
      "produtos": [{
        "nome": "Monitor 27", "marca": null, "modelo": null, "variante": "27 polegadas",
        "gtin": null, "loja": "Loja", "vendedor": null, "url": "https://loja.example/produto",
        "imagemUrl": null, "preco": 1299.90, "precoPix": null, "precoAnterior": null,
        "quantidadeParcelas": null, "valorParcela": null, "condicaoPreco": null, "moeda": "BRL",
        "disponibilidade": "desconhecida", "condicaoProduto": "novo",
        "correspondencia": "exata", "evidencia": "Página anuncia o produto e preço; estoque não confirmado."
      }]
    }
    """)!.AsObject();

  private static JsonObject Envelope(JsonObject json) => new()
  {
    ["id"] = "resp_teste",
    ["model"] = "gpt-4.1-mini",
    ["status"] = "completed",
    ["usage"] = new JsonObject { ["input_tokens"] = 100, ["output_tokens"] = 200 },
    ["output"] = new JsonArray(
      new JsonObject
      {
        ["type"] = "web_search_call", ["status"] = "completed",
        ["action"] = new JsonObject { ["sources"] = new JsonArray(new JsonObject { ["type"] = "url", ["url"] = UrlProduto }) }
      },
      new JsonObject
      {
        ["type"] = "message",
        ["content"] = new JsonArray(new JsonObject
        {
          ["type"] = "output_text", ["text"] = json.ToJsonString(),
          ["annotations"] = new JsonArray(new JsonObject { ["type"] = "url_citation", ["url"] = UrlProduto, ["title"] = "Produto" })
        })
      })
  };

  private sealed class RespostaHandler(JsonObject envelope, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
  {
    public JsonNode? Corpo { get; private set; }

    public string? Authorization { get; private set; }

    public int Chamadas { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Chamadas++;

      Authorization = request.Headers.Authorization?.ToString();

      Corpo = JsonNode.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));

      return new HttpResponseMessage(status) { Content = new StringContent(envelope.ToJsonString()) };
    }
  }

  private sealed class LentoHandler : HttpMessageHandler
  {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      await Task.Delay(Timeout.Infinite, cancellationToken);

      return new HttpResponseMessage(HttpStatusCode.OK);
    }
  }

  private sealed class OutroProvedor : IProvedorIa
  {
    public string Nome => "Outro";

    public Task<RespostaIa> ConsultarAsync(ConsultaIa consulta, CancellationToken cancellationToken = default) =>
      Task.FromResult(new RespostaIa(JsonSerializer.SerializeToElement(ProdutoJson()), Nome, "modelo", "id",
        DateTimeOffset.UtcNow, null, null, [new FonteIa(UrlProduto, "Produto")]));
  }
}

