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

public sealed class VarejistasPerfisTests
{
  [PlaywrightFact]
  public async Task PerfisUsamPrecoIntegralExigemCamposEIsolamLinksDeOutrosHosts()
  {
    var builder = WebApplication.CreateBuilder();

    builder.Logging.ClearProviders();

    builder.WebHost.UseUrls("http://127.0.0.1:0");

    await using var app = builder.Build();

    app.MapGet("/{loja}", (string loja, string q) => q == "bloqueada" ? Results.StatusCode(403)
      : Results.Content(q == "vazia" ? loja == "carrefour"
        ? "<h2>oops, não encontramos resultados</h2>" : "<h2>Nenhum produto encontrado</h2>"
        : Pagina(loja), "text/html; charset=utf-8"));

    await app.StartAsync();

    var endereco = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(new ConfigurationBuilder().Build());

    services.Configure<PlaywrightOptions>(options =>
    {
      options.Enabled = true;

      foreach (var loja in new[] { "carrefour", "kalunga", "decathlon" })
      {
        options.Lojas[loja].UrlBusca = endereco + "/" + loja + "?q={consulta}";

        options.Lojas[loja].HostsPermitidos = ["127.0.0.1"];
      }
    });

    await using var provider = services.BuildServiceProvider();

    var consulta = provider.GetRequiredService<IConsultaLoja>();

    foreach (var loja in new[] { "carrefour", "kalunga", "decathlon" })
    {
      var resultado = await consulta.ConsultarAsync(loja, "produto teste");

      var item = Assert.Single(resultado.Itens);

      Assert.Equal("Produto Teste Completo", item.Campos["nome"]);

      Assert.True(NormalizacaoPreco.Tentar(item.Campos["preco"], out var preco));

      Assert.Equal(1299.90m, preco);

      Assert.Equal(2, resultado.Descartados.Count);

      if (loja == "carrefour") Assert.Equal("R$ 1.299,90", item.Campos["precoPix"]);

      Assert.Empty((await consulta.ConsultarAsync(loja, "vazia")).Itens);

      var erro = await Assert.ThrowsAsync<HttpRequestException>(() => consulta.ConsultarAsync(loja, "bloqueada"));

      Assert.Equal(System.Net.HttpStatusCode.Forbidden, erro.StatusCode);
    }
  }

  private static string Pagina(string loja)
  {
    var valido = loja switch
    {
      "carrefour" => """
        <a data-testid='search-product-card' href='/produto/teste-1'><h2>Produto Teste Completo</h2>
          <del>R$ 1.999,90</del><div><span class='text-default font-bold'>R$ 1.299,90</span><div>à vista no Pix</div></div>
          <p>10x de R$ 129,99</p></a>
        """,

      "kalunga" => """
        <div class='blocoproduto'><a class='blocoproduto__link' href='/prod/teste/1' title='Produto Teste Completo'><h2>Produto Teste...</h2></a>
          <del>R$ 1.999,90</del><span class='blocoproduto__price'>R$ 1.299,90</span><p>10x de R$ 129,99</p>
          <button onclick="Comprar('1')">Comprar</button></div>
        """,

      "decathlon" => """
        <div data-testid='product-card'><a data-testid='product-link' href='/produto-teste/p'><h2>Produto Teste Completo</h2></a>
          <div class='product-card__content--price'><p>R$ 1.299,90 a vista</p><p class='line-through'>R$ 1.999,90</p></div>
          <p data-testid='product-card-installments'>10x de R$ 129,99</p></div>
        """,

      _ => throw new ArgumentException("Perfil desconhecido.", nameof(loja))
    };

    // Um card sem preço e outro com link externo nunca entram no resultado.
    return valido + valido.Replace("R$ 1.299,90", "", StringComparison.Ordinal)
      + valido.Replace("href='/", "href='https://nao-autorizado.example/", StringComparison.Ordinal);
  }
}
