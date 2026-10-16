using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Ofertas;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace Verum.Tests.Integracao;

public sealed class NovasLojasFactAttribute : FactAttribute
{
  public NovasLojasFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_NOVAS_LOJAS") != "1")
      Skip = "Configure VERUM_TEST_NOVAS_LOJAS=1 para consultar KaBuM e Amazon reais.";
  }
}

public sealed class NovasLojasReaisTests(ITestOutputHelper output)
{
  [NovasLojasFact]
  public async Task TresProdutosReaisCorrespondemESelecionamAteDozeOfertas()
  {
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",

      ["Playwright:TimeoutTotalSegundos"] = "90",

      ["Playwright:TimeoutAcaoSegundos"] = "30"
    }).Build();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(configuration);

    await using var provider = services.BuildServiceProvider();

    await using var scope = provider.CreateAsyncScope();

    var fontes = new ConsultaFontesService(scope.ServiceProvider.GetRequiredService<IConsultaLoja>(),
      new FluxoFontesTests.IaFake { Disponivel = false }, Options.Create(new BuscaOptions
      {
        Lojas = ["kabum", "amazon"],

        TimeoutFonteSegundos = 90
      }), NullLogger<ConsultaFontesService>.Instance);

    var relatorios = new List<object>();

    var falhas = new List<string>();

    foreach (var consulta in new[] { "televisão 4k", "iphone 17", "samsung s25" })
    {
      var coleta = await fontes.ConsultarAsync(consulta, 0, CancellationToken.None);

      var ranking = RankingOfertas.Classificar(coleta.Ofertas.Select(x => new OfertaConsultada(
        Guid.NewGuid(), Guid.NewGuid(), x.Nome, x.Url, x.Preco, x.CondicaoPreco, x.ImagemUrl,
        x.Fonte, 50m, x.ObservadaEm, x.ObservadaEm.AddMinutes(15))), 12, consulta);

      relatorios.Add(new
      {
        consulta,
        fontes = coleta.Fontes,
        encontrados = coleta.Ofertas.Count,
        selecionados = ranking.Select(x => new { x.Nome, x.Fonte, x.Preco, x.Url, x.Pontuacao }).ToArray()
      });

      if (ranking.Count == 0) falhas.Add(consulta + ": nenhum resultado correspondente");

      if (coleta.Fontes.Any(x => x.Fonte != "ia" && x.Status != "Concluida")) falhas.Add(consulta + ": fonte não concluiu");

      Assert.True(ranking.Count <= 12);

      Assert.All(ranking, x => Assert.True(NormalizacaoOferta.Corresponde(consulta, x.Nome)));

      Assert.All(ranking.Take(3), x => Assert.True(NormalizacaoOferta.Relevancia(consulta, x.Nome) > 30m));
    }

    var json = JsonSerializer.Serialize(relatorios, new JsonSerializerOptions { WriteIndented = true });

    output.WriteLine(json);

    var arquivo = Environment.GetEnvironmentVariable("VERUM_RELATORIO_BUSCAS");

    if (!string.IsNullOrWhiteSpace(arquivo)) await File.WriteAllTextAsync(arquivo, json);

    Assert.True(falhas.Count == 0, string.Join("; ", falhas));
  }
}
