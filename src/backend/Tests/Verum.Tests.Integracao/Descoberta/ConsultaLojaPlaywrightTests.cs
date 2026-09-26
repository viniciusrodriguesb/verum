using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Verum.Modules.Ofertas;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class PlaywrightFactAttribute : FactAttribute
{
  public PlaywrightFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_PLAYWRIGHT") != "1")
      Skip = "Configure VERUM_TEST_PLAYWRIGHT=1 após instalar Chromium.";
  }
}

public sealed class ConsultaLojaPlaywrightTests
{
  [PlaywrightFact]
  public async Task NavegadorRespeitaConcorrenciaEReabreAposDesconexao()
  {
    await using var provider = Provider("http://127.0.0.1");

    var navegador = provider.GetRequiredService<NavegadorPlaywright>();

    var ativas = 0;

    var pico = 0;

    var trava = new object();

    await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => navegador.ExecutarAsync(async (context, token) =>
    {
      lock (trava)
      {
        ativas++;

        pico = Math.Max(pico, ativas);
      }

      await Task.Delay(100, token);

      lock (trava) ativas--;

      return 1;
    }, CancellationToken.None)));

    Assert.Equal(2, pico);

    await navegador.ExecutarAsync(async (context, token) =>
    {
      await context.Browser!.CloseAsync();

      return 1;
    }, CancellationToken.None);

    Assert.True(await navegador.ExecutarAsync((context, token) => Task.FromResult(context.Browser!.IsConnected), CancellationToken.None));
  }

  [PlaywrightFact]
  public async Task LayoutInesperadoNaoViraResultadoVazio()
  {
    await using var site = await SiteLocal.CriarAsync();

    await using var provider = Provider(site.Url, new()
    {
      ["Playwright:Lojas:teste:UrlBusca"] = site.Url + "/form?q={consulta}",
      ["Playwright:Lojas:teste:ResultadosProntos"] = "form"
    });

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      provider.GetRequiredService<IConsultaLoja>().ConsultarAsync("teste", "monitor"));
  }

  [PlaywrightFact]
  public async Task ExtraiPaginaDinamicaPaginaSeguinteEDescartaCardIncompleto()
  {
    await using var site = await SiteLocal.CriarAsync();

    await using var provider = Provider(site.Url);

    var resultado = await provider.GetRequiredService<IConsultaLoja>().ConsultarAsync("teste", "monitor & 27");

    Assert.Equal(2, resultado.Itens.Count);

    Assert.Equal("R$ 1.299,90", resultado.Itens[0].Campos["preco"]);

    Assert.Equal("monitor & 27", resultado.Itens[0].Campos["consulta"]);

    Assert.Single(resultado.Descartados);

    Assert.False(resultado.LimiteAtingido);
  }

  [PlaywrightFact]
  public async Task FormularioEContextosSaoIsoladosEntreConsultas()
  {
    await using var site = await SiteLocal.CriarAsync();

    await using var provider = Provider(site.Url, new()
    {
      ["Playwright:Lojas:teste:UrlBusca"] = site.Url + "/form",
      ["Playwright:Lojas:teste:CampoBusca"] = "#consulta",
      ["Playwright:Lojas:teste:BotaoBusca"] = "button",
      ["Playwright:Lojas:teste:ProximaPagina"] = null
    });

    var service = provider.GetRequiredService<IConsultaLoja>();

    var resultados = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => service.ConsultarAsync("teste", "monitor")));

    Assert.All(resultados, x => Assert.Equal("novo", x.Itens[0].Campos["sessao"]));
  }

  [PlaywrightFact]
  public async Task AusenciaDeResultadosExigeMarcadorExplicito()
  {
    await using var site = await SiteLocal.CriarAsync();

    await using var provider = Provider(site.Url, new()
    {
      ["Playwright:Lojas:teste:UrlBusca"] = site.Url + "/vazio?q={consulta}",
      ["Playwright:Lojas:teste:ResultadosProntos"] = "#vazio"
    });

    var result = await provider.GetRequiredService<IConsultaLoja>().ConsultarAsync("teste", "monitor");

    Assert.Empty(result.Itens);
  }

  [PlaywrightFact]
  public async Task CancelamentoLiberaVagaENaoInterrompeProximaConsulta()
  {
    await using var site = await SiteLocal.CriarAsync();

    await using var provider = Provider(site.Url, new() { ["Playwright:ConsultasSimultaneas"] = "1" });

    var service = provider.GetRequiredService<IConsultaLoja>();

    using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(1));

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ConsultarAsync("teste", "lento", cancel.Token));

    Assert.NotEmpty((await service.ConsultarAsync("teste", "monitor")).Itens);
  }

  [PlaywrightFact]
  public async Task PrazoTotalFechaPaginaELimiteDeItensEhExplicito()
  {
    await using var site = await SiteLocal.CriarAsync();

    await using var provider = Provider(site.Url, new()
    {
      ["Playwright:TimeoutTotalSegundos"] = "2",
      ["Playwright:TimeoutAcaoSegundos"] = "2",
      ["Playwright:Lojas:teste:MaxItens"] = "1"
    });

    var service = provider.GetRequiredService<IConsultaLoja>();

    await Assert.ThrowsAnyAsync<TimeoutException>(() => service.ConsultarAsync("teste", "lento"));

    var result = await service.ConsultarAsync("teste", "monitor");

    Assert.Single(result.Itens);

    Assert.True(result.LimiteAtingido);
  }

  [Fact]
  public async Task DesabilitadoNaoPrecisaDeBrowser()
  {
    await using var provider = Provider("http://127.0.0.1", new() { ["Playwright:Enabled"] = "false" });

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      provider.GetRequiredService<IConsultaLoja>().ConsultarAsync("teste", "monitor"));
  }

  [Theory]
  [InlineData("https://loja.example/item", true)]
  [InlineData("https://loja.example.evil.test/item", false)]
  [InlineData("file:///etc/passwd", false)]
  [InlineData("https://usuario@loja.example/item", false)]
  public void HostsSaoComparadosExatamente(string url, bool esperado)
  {
    Assert.Equal(esperado, ConsultaLojaPlaywright.Permitida(url,
      new LojaPlaywrightOptions { HostsPermitidos = ["loja.example"] }));
  }

  private static ServiceProvider Provider(string url, Dictionary<string, string?>? overrides = null)
  {
    var values = new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",
      ["Playwright:Lojas:teste:UrlBusca"] = url + "/busca?q={consulta}",
      ["Playwright:Lojas:teste:HostsPermitidos:0"] = "127.0.0.1",
      ["Playwright:Lojas:teste:ResultadosProntos"] = "#pronto",
      ["Playwright:Lojas:teste:Itens"] = ".produto",
      ["Playwright:Lojas:teste:SemResultados"] = "#vazio",
      ["Playwright:Lojas:teste:LinkProduto"] = "a",
      ["Playwright:Lojas:teste:ProximaPagina"] = "a.proxima",
      ["Playwright:Lojas:teste:Campos:nome:Seletor"] = ".nome",
      ["Playwright:Lojas:teste:Campos:nome:Obrigatorio"] = "true",
      ["Playwright:Lojas:teste:Campos:preco:Seletor"] = ".preco",
      ["Playwright:Lojas:teste:Campos:consulta:Seletor"] = ".consulta",
      ["Playwright:Lojas:teste:Campos:sessao:Seletor"] = ".sessao"
    };

    if (overrides is not null)
      foreach (var item in overrides) values[item.Key] = item.Value;

    var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(configuration);

    return services.BuildServiceProvider();
  }

  private sealed class SiteLocal(WebApplication app, string url) : IAsyncDisposable
  {
    public string Url => url;

    public static async Task<SiteLocal> CriarAsync()
    {
      var builder = WebApplication.CreateBuilder();

      builder.Logging.ClearProviders();

      builder.WebHost.UseUrls("http://127.0.0.1:0");

      var app = builder.Build();

      app.MapGet("/form", () => Results.Content("<form action='/busca'><input id='consulta' name='q'><button>Buscar</button></form>", "text/html"));

      app.MapGet("/vazio", () => Results.Content("<div id='vazio'>Sem resultados</div>", "text/html"));

      app.MapGet("/busca", async (HttpContext context) =>
      {
        var consulta = context.Request.Query["q"].ToString();

        if (consulta == "lento") await Task.Delay(30000, context.RequestAborted);

        var pagina = context.Request.Query["p"] == "2" ? 2 : 1;

        var sessao = context.Request.Cookies.ContainsKey("sessao") ? "reutilizada" : "novo";

        context.Response.Cookies.Append("sessao", "ativa");

        var texto = System.Net.WebUtility.HtmlEncode(consulta);

        var proxima = pagina == 1 ? "<a class='proxima' href='/busca?p=2'>Próxima</a>" : "";

        var invalido = pagina == 1 ? "<article class='produto'><span class='nome'>Sem link</span></article>" : "";

        var html = $$"""
          <html><body><div id="conteudo" hidden>
          <article class="produto">
          <a href="/produto/{{pagina}}"><span class="nome">Monitor {{pagina}}</span></a>
          <span class="preco">R$ 1.299,90</span><span class="consulta">{{texto}}</span><span class="sessao">{{sessao}}</span>
          </article>{{invalido}}{{proxima}}</div>
          <script>setTimeout(() => { document.querySelector('#conteudo').hidden=false; document.querySelector('#conteudo').id='pronto'; }, 80);</script>
          </body></html>
          """;

        return Results.Content(html, "text/html");
      });

      await app.StartAsync();

      var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();

      return new SiteLocal(app, address);
    }

    public ValueTask DisposeAsync() => app.DisposeAsync();
  }
}


