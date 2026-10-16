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

public sealed class KabumSimulacaoTests
{
  [PlaywrightFact]
  public async Task PerfilKabumExtraiPixPaginaPorCliqueEDescartaIncompleto()
  {
    var builder = WebApplication.CreateBuilder();

    builder.Logging.ClearProviders();

    builder.WebHost.UseUrls("http://127.0.0.1:0");

    await using var app = builder.Build();

    app.MapGet("/busca/{termo}", (string termo) => Results.Content(termo == "vazio"
      ? "<div id='listingEmpty'>Nenhum produto encontrado</div>"
      : $$"""
        <main>
        <a href='/produto/1'><span class='line-clamp-2'>{{System.Net.WebUtility.HtmlEncode(termo)}}</span>
        <span class='font-semibold' aria-hidden='false'>R$</span>
        <span class='font-semibold' aria-hidden='false'>1.299,90</span>
        <span class='text-xs' aria-hidden='false'>No PIX ou 10x de R$ 144,43</span></a>
        <a href='/produto/2'><span class='line-clamp-2'>Sem preço</span></a>
        </main>
        <div id='listingPagination'><a role='button' class='nextLink' aria-disabled='false'>Próxima</a></div>
        <script>document.querySelector('.nextLink').onclick = function() {
          document.querySelector('main').innerHTML = '<div>Carregando resultados</div>';
          setTimeout(() => {
            document.querySelector('main').innerHTML = `<a href='/produto/3'><span class='line-clamp-2'>Página 2</span><span class='font-semibold' aria-hidden='false'>R$</span><span class='font-semibold' aria-hidden='false'>599,90</span></a>`;
            this.setAttribute('aria-disabled', 'true');
          }, 100);
        };</script>
        """, "text/html"));

    await app.StartAsync();

    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(new ConfigurationBuilder().Build());

    services.Configure<PlaywrightOptions>(options =>
    {
      options.Enabled = true;

      options.Lojas["kabum"].UrlBusca = address + "/busca/{consulta}";

      options.Lojas["kabum"].HostsPermitidos = ["127.0.0.1"];
    });

    await using var provider = services.BuildServiceProvider();

    await using var scope = provider.CreateAsyncScope();

    var consulta = scope.ServiceProvider.GetRequiredService<IConsultaLoja>();

    var resultado = await consulta.ConsultarAsync("kabum", "monitor & 27");

    Assert.Equal(2, resultado.Itens.Count);

    Assert.Equal("monitor & 27", resultado.Itens[0].Campos["nome"]);

    Assert.Equal("1.299,90", resultado.Itens[0].Campos["precoPix"]);

    Assert.Contains("10x", resultado.Itens[0].Campos["condicaoPreco"]);

    Assert.EndsWith("/produto/3", resultado.Itens[1].Url);

    Assert.Single(resultado.Descartados);

    Assert.False(resultado.LimiteAtingido);

    Assert.Empty((await consulta.ConsultarAsync("kabum", "vazio")).Itens);
  }
}
