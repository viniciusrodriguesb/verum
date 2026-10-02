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
using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class NovosProvedoresTests
{
  [PlaywrightFact]
  public async Task PerfisExtraemPrecoTotalDescartamIncompletosEReconhecemBloqueio()
  {
    var builder = WebApplication.CreateBuilder();

    builder.Logging.ClearProviders();

    builder.WebHost.UseUrls("http://127.0.0.1:0");

    await using var app = builder.Build();

    app.MapGet("/{loja}", (string loja, string q) => q == "bloqueada"
      ? Results.StatusCode(403)
      : Results.Content(loja == "amazon" && q == "pagina2" ? """
        <div data-component-type='s-search-result' data-asin='B012345678'>
          <a href='/dp/B012345678?tracking=outra'><h2>Monitor Gamer 27</h2></a>
          <span class='a-price'><span class='a-offscreen'>R$ 1.299,90</span></span>
        </div>
        <div data-component-type='s-search-result' data-asin='B012345680'>
          <a href='/dp/B012345680'><h2>Monitor Gamer 32</h2></a>
          <span class='a-price'><span class='a-offscreen'>R$ 1.599,90</span></span>
        </div>
        """ : Pagina(loja, q == "vazia"), "text/html; charset=utf-8"));

    await app.StartAsync();

    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(new ConfigurationBuilder().Build());

    services.Configure<PlaywrightOptions>(options =>
    {
      options.Enabled = true;

      foreach (var loja in new[] { "amazon" })
      {
        options.Lojas[loja].UrlBusca = address + "/" + loja + "?q={consulta}";

        options.Lojas[loja].HostsPermitidos = ["127.0.0.1"];
      }
    });

    await using var provider = services.BuildServiceProvider();

    await using var scope = provider.CreateAsyncScope();

    var consulta = scope.ServiceProvider.GetRequiredService<IConsultaLoja>();

    foreach (var loja in new[] { "amazon" })
    {
      var resultado = await consulta.ConsultarAsync(loja, "monitor");

      Assert.True(resultado.Itens.Count == (loja == "amazon" ? 2 : 1),
        $"{loja}: {resultado.Itens.Count} aceitos, {resultado.Descartados.Count} descartados");

      Assert.Single(resultado.Descartados);

      Assert.Equal("Monitor Gamer 27", resultado.Itens[0].Campos["nome"]);

      Assert.Empty((await consulta.ConsultarAsync(loja, "vazia")).Itens);

      var erro = await Assert.ThrowsAsync<HttpRequestException>(() => consulta.ConsultarAsync(loja, "bloqueada"));

      Assert.Equal(System.Net.HttpStatusCode.Forbidden, erro.StatusCode);

      if (loja == "amazon") Assert.EndsWith("/dp/B012345678", resultado.Itens[0].Url);
    }
  }

  private static string Pagina(string loja, bool vazia) => loja switch
  {
    "amazon" => vazia ? "<div class='s-no-results'>Nenhum resultado</div>" : """
      <div data-component-type='s-search-result' data-asin='B012345678'>
        <a href='/monitor/dp/B012345678/ref=sr_1?tracking=123'><h2>Monitor Gamer 27</h2></a>
        <span class='a-price'><span class='a-offscreen'>R$ 1.299,90</span></span>
        <span class='a-price a-text-price'><span class='a-offscreen'>R$ 1.999,90</span></span>
      </div>
      <div data-component-type='s-search-result' data-asin='B012345679'><a href='/dp/B012345679'><h2>Sem preço</h2></a></div>
      <a class='s-pagination-next' href='/amazon?q=pagina2'>Próxima</a>
      """,

    _ => throw new ArgumentException("Perfil de teste desconhecido.", nameof(loja))
  };
}
