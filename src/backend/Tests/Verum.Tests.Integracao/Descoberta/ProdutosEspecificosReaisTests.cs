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

public sealed class ProdutosEspecificosFactAttribute : FactAttribute
{
  public ProdutosEspecificosFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_PRODUTOS_ESPECIFICOS") != "1")
      Skip = "Configure VERUM_TEST_PRODUTOS_ESPECIFICOS=1 para consultar livros e itens específicos nas lojas reais.";
  }
}

public sealed class ProdutosEspecificosReaisTests(ITestOutputHelper output)
{
  [ProdutosEspecificosFact]
  public async Task LivrosItensRarosEControlePreservamCorrespondenciaEDiagnosticos()
  {
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",

      ["Playwright:TimeoutAcaoSegundos"] = "30"
    }).Build();

    var services = new ServiceCollection().AddLogging();

    services.AddOfertasDescoberta(configuration);

    await using var provider = services.BuildServiceProvider();

    var fontes = new ConsultaFontesService(provider.GetRequiredService<IConsultaLoja>(),
      new FluxoFontesTests.IaFake { Disponivel = false }, Options.Create(new BuscaOptions()),
      NullLogger<ConsultaFontesService>.Instance);

    var relatorios = new List<object>();

    foreach (var consulta in new[] { "O homem mais rico da Babilônia", "1984 George Orwell", "cabo SCSI HD68", "ssd kingston nv3 1tb", "verumproduto inexistente928477" })
    {
      var tempo = Stopwatch.StartNew();

      var coleta = await fontes.ConsultarAsync(consulta, 0, CancellationToken.None);

      relatorios.Add(new
      {
        consulta,

        segundos = Math.Round(tempo.Elapsed.TotalSeconds, 2),

        fontes = coleta.Fontes,

        ofertas = coleta.Ofertas.Select(x => new { x.Nome, x.Fonte, x.Preco, x.Url }).ToArray()
      });

      Assert.All(coleta.Ofertas, x => Assert.True(NormalizacaoOferta.Corresponde(consulta, x.Nome)));

      Assert.True(tempo.Elapsed < TimeSpan.FromSeconds(60), "As fontes devem respeitar o prazo configurado.");

      Assert.Equal(2, coleta.Fontes.Count(x => !x.OrigemIa));

      if (consulta == "O homem mais rico da Babilônia") Assert.NotEmpty(coleta.Ofertas);

      if (consulta == "verumproduto inexistente928477")
      {
        Assert.Empty(coleta.Ofertas);

        Assert.All(coleta.Fontes.Where(x => !x.OrigemIa), x => Assert.Equal("Concluida", x.Status));
      }
    }

    var json = JsonSerializer.Serialize(relatorios, new JsonSerializerOptions { WriteIndented = true });

    output.WriteLine(json);

    var arquivo = Environment.GetEnvironmentVariable("VERUM_RELATORIO_ESPECIFICOS");

    if (!string.IsNullOrWhiteSpace(arquivo)) await File.WriteAllTextAsync(arquivo, json);
  }
}
