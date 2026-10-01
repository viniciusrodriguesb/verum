using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Verum.Modules.Ofertas;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Xunit;
using Xunit.Abstractions;

namespace Verum.Tests.Integracao;

public sealed class KabumRealFactAttribute : FactAttribute
{
  public KabumRealFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_KABUM") != "1")
      Skip = "Configure VERUM_TEST_KABUM=1 para consultar a loja real.";
  }
}

public sealed class KabumProviderTests(ITestOutputHelper output)
{
  [KabumRealFact]
  public async Task ConsultaRealProdutosPaginacaoEAusenciaDeResultados()
  {
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",
      ["Playwright:ConsultasSimultaneas"] = "1",
      ["Playwright:TimeoutTotalSegundos"] = "120",
      ["Playwright:TimeoutAcaoSegundos"] = "30",
      ["Playwright:Lojas:kabum:MaxItens"] = "65"
    }).Build();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(configuration);

    await using var provider = services.BuildServiceProvider();

    await using var scope = provider.CreateAsyncScope();

    var consulta = scope.ServiceProvider.GetRequiredService<IConsultaLoja>();

    foreach (var termo in new[] { "monitor", "ssd kingston", "verumprodutozzinexistente928477" })
    {
      var resultado = await consulta.ConsultarAsync("kabum", termo);

      output.WriteLine($"Consulta: {termo}; itens: {resultado.Itens.Count}; descartados: {resultado.Descartados.Count}; limite: {resultado.LimiteAtingido}");

      if (termo.StartsWith("verumproduto", StringComparison.Ordinal))
      {
        Assert.Empty(resultado.Itens);

        Assert.Empty(resultado.Descartados);

        continue;
      }

      Assert.NotEmpty(resultado.Itens);

      if (termo == "monitor") Assert.True(resultado.Itens.Count > 60, "Deve alcançar a segunda página.");

      Assert.Equal(resultado.Itens.Count, resultado.Itens.Select(x => x.Url).Distinct().Count());

      Assert.All(resultado.Itens, item =>
      {
        Assert.Equal("www.kabum.com.br", new Uri(item.Url).Host);

        Assert.StartsWith("/produto/", new Uri(item.Url).AbsolutePath);

        Assert.False(string.IsNullOrWhiteSpace(item.Campos["nome"]));

        Assert.False(string.IsNullOrWhiteSpace(item.Campos["precoPix"]));
      });

      output.WriteLine($"Amostra: {resultado.Itens[0].Campos["nome"]}; preço PIX bruto: {resultado.Itens[0].Campos["precoPix"]}; condição: {resultado.Itens[0].Campos["condicaoPreco"]}");
    }
  }
}

