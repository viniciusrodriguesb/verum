using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Ofertas;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Xunit;
using Xunit.Abstractions;

namespace Verum.Tests.Integracao;

public sealed class VarejistasFactAttribute : FactAttribute
{
  public VarejistasFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_VAREJISTAS") != "1")
      Skip = "Configure VERUM_TEST_VAREJISTAS=1 para consultar os novos varejistas reais.";
  }
}

public sealed class VarejistasReaisTests(ITestOutputHelper output)
{
  [VarejistasFact]
  public async Task ComparaLatenciaECoberturaDasDuasFontesAnterioresComAsCincoAtuais()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",

      ["Playwright:TimeoutAcaoSegundos"] = "12"
    }).Build());

    await using var provider = services.BuildServiceProvider();

    var relatorios = new List<object>();

    foreach (var termo in new[] { "O homem mais rico da Babilônia", "bola volei mikassa", "cabo SCSI HD68" })
    {
      foreach (var lojas in new[] { new[] { "kabum", "amazon" }, new BuscaOptions().Lojas })
      {
        var fontes = new ConsultaFontesService(provider.GetRequiredService<IConsultaLoja>(),
          new FluxoFontesTests.IaFake { Disponivel = false }, Options.Create(new BuscaOptions { Lojas = lojas }),
          NullLogger<ConsultaFontesService>.Instance);

        var tempo = Stopwatch.StartNew();

        var coleta = await fontes.ConsultarAsync(termo, 0, CancellationToken.None);

        relatorios.Add(new { termo, lojas, segundos = tempo.Elapsed.TotalSeconds, ofertas = coleta.Ofertas.Count,
          fontes = coleta.Fontes, amostra = coleta.Ofertas.Take(3).Select(x => x.Nome).ToArray() });

        Assert.Equal(lojas.Length, coleta.Fontes.Count(x => !x.OrigemIa));

        Assert.True(tempo.Elapsed < TimeSpan.FromSeconds(60), "A coleta deve respeitar os prazos das fontes.");
      }
    }

    var json = JsonSerializer.Serialize(relatorios, new JsonSerializerOptions { WriteIndented = true });

    output.WriteLine(json);

    var destino = Environment.GetEnvironmentVariable("VERUM_RELATORIO_PERFORMANCE");

    if (!string.IsNullOrWhiteSpace(destino)) await File.WriteAllTextAsync(destino, json);
  }

  [VarejistasFact]
  public async Task PerfisReaisExtraemCategoriasVariadasSemParcelasNemRecomendacoesIncompativeis()
  {
    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",

      ["Playwright:TimeoutAcaoSegundos"] = "12",

      ["Playwright:TimeoutTotalSegundos"] = "25"
    }).Build());

    await using var provider = services.BuildServiceProvider();

    var consulta = provider.GetRequiredService<IConsultaLoja>();

    var relatorios = new List<object>();

    var falhas = new List<string>();

    foreach (var (loja, termo) in new[]
    {
      ("carrefour", "O homem mais rico da Babilônia"),
      ("carrefour", "torneira cozinha"),
      ("kalunga", "grampeador spiral"),
      ("kalunga", "papel chamex a4"),
      ("decathlon", "bola volei"),
      ("decathlon", "barraca camping"),
      ("carrefour", "torneirra cozinha"),
      ("carrefour", "torneira coznha"),
      ("kalunga", "grampeadro spiral"),
      ("decathlon", "bola volei mikassa"),
      ("carrefour", "verumproduto inexistente928477"),
      ("kalunga", "verumproduto inexistente928477"),
      ("decathlon", "verumproduto inexistente928477")
    })
    {
      var inicio = Stopwatch.StartNew();

      try
      {
        var resultado = await consulta.ConsultarAsync(loja, termo);

        var validos = resultado.Itens.Select(x => NormalizacaoOferta.DaLoja(x, loja, termo, resultado.ConsultadaEm))
          .OfType<Verum.Modules.Ofertas.Contratos.Descoberta.DTOs.OfertaDescoberta>().ToArray();

        relatorios.Add(new { loja, termo, segundos = inicio.Elapsed.TotalSeconds, extraidos = resultado.Itens.Count,
          descartados = resultado.Descartados.Count, validos = validos.Select(x => new { x.Nome, x.Preco, x.Url }).ToArray() });

        if (termo.Contains("inexistente", StringComparison.Ordinal))
        {
          if (validos.Length != 0) falhas.Add($"{loja}: termo inexistente produziu ofertas.");
        }
        else if (termo != "torneirra cozinha" && validos.Length == 0) falhas.Add($"{loja}: {termo} não produziu ofertas correspondentes.");

        Assert.True(inicio.Elapsed < TimeSpan.FromSeconds(30), "A fonte excedeu o orçamento de teste.");
      }
      catch (Exception erro) when (erro is not Xunit.Sdk.XunitException)
      {
        relatorios.Add(new { loja, termo, segundos = inicio.Elapsed.TotalSeconds, erro = erro.GetType().Name, mensagem = erro.Message });

        falhas.Add($"{loja}: {termo}: {erro.GetType().Name}");
      }
    }

    var json = JsonSerializer.Serialize(relatorios, new JsonSerializerOptions { WriteIndented = true });

    output.WriteLine(json);

    var destino = Environment.GetEnvironmentVariable("VERUM_RELATORIO_VAREJISTAS");

    if (!string.IsNullOrWhiteSpace(destino)) await File.WriteAllTextAsync(destino, json);

    Assert.True(falhas.Count == 0, string.Join(Environment.NewLine, falhas));
  }
}
