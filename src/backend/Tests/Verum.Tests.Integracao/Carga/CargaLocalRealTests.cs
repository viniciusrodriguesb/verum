extern alias VerumApi;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Verum.Modules.Acesso.Contratos.DTOs;
using Verum.Modules.Acesso.Dominio;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;
using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Contas.Infraestrutura.Persistencia;
using Verum.Modules.Notificacoes.Infraestrutura.Persistencia;
using Verum.Modules.Ofertas;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Infraestrutura.Persistencia;
using Verum.Modules.Radar.Contratos.Configuracoes;
using Verum.Modules.Radar.Contratos.DTOs;
using Verum.Modules.Radar.Infraestrutura.Persistencia;
using Verum.Tests.Integracao.Carga.Configuracoes;
using Verum.Tests.Integracao.Carga.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace Verum.Tests.Integracao;

public sealed class CargaLocalRealTests(ITestOutputHelper output)
{
  [CargaLocalFact]
  public async Task CinquentaConsultasReaisComOutboxRabbitMqEAlertasConcorrentes()
  {
    var resultados = new ConcurrentBag<ResultadoCarga>();

    await using var ambiente = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync(rabbit: true, configurar: ConfigurarFontes);

    await using var factory = ambiente.CriarApi().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
    {
      services.AddAuthentication("IdentidadeTeste")
        .AddScheme<AuthenticationSchemeOptions, AcessoBuscaApiTests.IdentidadeTesteHandler>("IdentidadeTeste", _ => { });

      services.PostConfigure<RadarOptions>(options => options.Habilitado = true);
    }));

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      await scope.ServiceProvider.GetRequiredService<RadarDbContext>().Database.MigrateAsync();

      await scope.ServiceProvider.GetRequiredService<NotificacoesDbContext>().Database.MigrateAsync();

      var acesso = scope.ServiceProvider.GetRequiredService<AcessoDbContext>();

      acesso.Pacotes.Add(new PacoteAcesso("VERUM_PLUS", "Verum Plus"));

      await acesso.SaveChangesAsync();
    }

    await ambiente.Host.StartAsync();

    var tempo = Stopwatch.StartNew();

    try
    {
      await Parallel.ForEachAsync(ConsultasCarga.Termos.Select((termo, indice) => (termo, indice)),
        new ParallelOptions { MaxDegreeOfParallelism = 2 }, async (cenario, cancellationToken) =>
      {
        var cronometro = Stopwatch.StartNew();

        try
        {
          var resultado = await ExecutarAsync(factory, cenario.termo, cenario.indice, cancellationToken);

          resultados.Add(resultado with { Segundos = cronometro.Elapsed.TotalSeconds });

          output.WriteLine("{0}/50 {1}: {2}, {3} ofertas, {4:F2}s", cenario.indice + 1, cenario.termo,
            resultado.Status, resultado.Ofertas.Length, cronometro.Elapsed.TotalSeconds);
        }
        catch (Exception erro)
        {
          resultados.Add(new(cenario.termo, "Erro", cronometro.Elapsed.TotalSeconds, false, 0, 0, 0, [],
            erro.GetType().Name + ": " + erro.Message));
        }
      });
    }
    finally
    {
      await ambiente.Host.StopAsync();

      var ordenados = resultados.OrderBy(x => x.Consulta).ToArray();

      await using var scope = factory.Services.CreateAsyncScope();

      var execucoes = await scope.ServiceProvider.GetRequiredService<OfertasDbContext>().ExecucoesConsultaFonte
        .AsNoTracking().Select(x => new { Fonte = x.Fonte.Codigo, x.Status, x.CodigoErro,
          x.QuantidadeItensEncontrados, x.QuantidadeItensAceitos }).ToArrayAsync();

      var destino = Environment.GetEnvironmentVariable("VERUM_RELATORIO_CARGA")
        ?? Path.Combine(Path.GetTempPath(), "verum-carga-local.json");

      await File.WriteAllTextAsync(destino, JsonSerializer.Serialize(new
      {
        ExecutadaEm = DateTimeOffset.UtcNow,
        DuracaoSegundos = tempo.Elapsed.TotalSeconds,
        ConcorrenciaBuscas = 2,
        Autenticacao = "Handler exclusivo de teste; não valida o SSO real",
        Infraestrutura = "PostgreSQL, RabbitMQ, Outbox e lojas reais; IA/pagamentos/e-mail externo desativados",
        Fontes = execucoes.GroupBy(x => new { x.Fonte, Status = x.Status.ToString(), x.CodigoErro })
          .Select(x => new { x.Key, Consultas = x.Count(), Encontrados = x.Sum(y => y.QuantidadeItensEncontrados),
            Aceitos = x.Sum(y => y.QuantidadeItensAceitos) }),
        Resultados = ordenados
      }, new JsonSerializerOptions { WriteIndented = true }));

      output.WriteLine("Relatório: {0}", destino);
    }

    Assert.Equal(50, resultados.Count);

    Assert.DoesNotContain(resultados, x => x.Erro is not null);

    Assert.DoesNotContain(resultados, x => x.Status == "Falhou");

    Assert.Contains(resultados, x => x.Alertas > 0);
  }

  private static void ConfigurarFontes(IServiceCollection services)
  {
    services.RemoveAll<IConsultaLoja>();

    services.AddOfertasDescoberta(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Playwright:Enabled"] = "true",
      ["Playwright:ConsultasSimultaneas"] = "2"
    }).Build());

    services.PostConfigure<BuscaOptions>(options => options.Lojas = new BuscaOptions().Lojas);
  }

  private static async Task<ResultadoCarga> ExecutarAsync(WebApplicationFactory<VerumApi::Program> factory,
    string termo, int indice, CancellationToken cancellationToken)
  {
    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    var contexto = indice % 3;

    if (contexto != 0) client.DefaultRequestHeaders.Add("X-Identidade-Teste", Guid.NewGuid().ToString());

    var acesso = await client.GetFromJsonAsync<AcessoBusca>("/api/v1/buscas/acesso", cancellationToken);

    Assert.Equal(contexto == 0 ? "Visitante" : "Free", acesso!.Contexto);

    if (contexto == 2) await ConcederAsync(factory, client);

    var pedido = new SolicitarBusca(termo, "carga-" + Guid.NewGuid().ToString("N"));

    using var resposta = await client.PostAsJsonAsync("/api/v1/buscas", pedido, cancellationToken);

    Assert.Equal(HttpStatusCode.Accepted, resposta.StatusCode);

    var aceita = (await resposta.Content.ReadFromJsonAsync<BuscaAceita>(cancellationToken))!;

    using var repetida = await client.PostAsJsonAsync("/api/v1/buscas", pedido, cancellationToken);

    Assert.Equal(aceita.Id, (await repetida.Content.ReadFromJsonAsync<BuscaAceita>(cancellationToken))!.Id);

    using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    prazo.CancelAfter(TimeSpan.FromMinutes(4));

    BuscaConsulta? resultado;

    do
    {
      await Task.Delay(500, prazo.Token);

      resultado = await client.GetFromJsonAsync<BuscaConsulta>($"/api/v1/buscas/{aceita.Id}", prazo.Token);
    } while (resultado!.Status is "Recebida" or "Processando");

    Assert.Contains(resultado.Status, new[] { "Concluida", "ConcluidaSemResultado", "Falhou" });

    Assert.InRange(resultado.Ofertas.Count, 0, 12);

    Assert.Equal(resultado.Ofertas.Count, resultado.Ofertas.Select(x => x.OfertaId).Distinct().Count());

    Assert.All(resultado.Ofertas, oferta =>
    {
      Assert.True(oferta.Preco > 0);

      Assert.True(NormalizacaoOferta.Relevancia(termo, oferta.Nome) > 30);
    });

    using var outro = factory.CreateClient();

    Assert.Equal(HttpStatusCode.NotFound, (await outro.GetAsync($"/api/v1/buscas/{aceita.Id}", cancellationToken)).StatusCode);

    var alertas = contexto == 2 && resultado.Ofertas.Count > 0
      ? await CriarAlertasAsync(client, resultado.Ofertas[0], cancellationToken) : 0;

    return new(termo, resultado.Status, 0, resultado.Parcial, resultado.FontesComSucesso,
      resultado.FontesComFalha, alertas, resultado.Ofertas.Select(x => x.Nome).ToArray(), null);
  }

  private static async Task ConcederAsync(WebApplicationFactory<VerumApi::Program> factory, HttpClient client)
  {
    await using var scope = factory.Services.CreateAsyncScope();

    var sujeito = client.DefaultRequestHeaders.GetValues("X-Identidade-Teste").Single();

    var conta = await scope.ServiceProvider.GetRequiredService<ContasDbContext>().Contas.SingleAsync(x => x.SujeitoIdentidade == sujeito);

    var db = scope.ServiceProvider.GetRequiredService<AcessoDbContext>();

    var pacote = await db.Pacotes.SingleAsync(x => x.Codigo == "VERUM_PLUS");

    db.Concessoes.Add(new ConcessaoPacote(conta.Id, pacote.Id, OrigemConcessao.Administracao,
      DateTimeOffset.UtcNow, validaAte: DateTimeOffset.UtcNow.AddHours(1)));

    await db.SaveChangesAsync();
  }

  private static async Task<int> CriarAlertasAsync(HttpClient client, OfertaResultado oferta, CancellationToken cancellationToken)
  {
    var pedido = new CriarMonitoramento(oferta.OfertaId, oferta.VarianteId, decimal.Round(oferta.Preco * 0.9m, 2),
      "alerta-" + Guid.NewGuid().ToString("N"), true, false);

    var ids = await Task.WhenAll(Enumerable.Range(0, 5).Select(async _ =>
    {
      using var resposta = await client.PostAsJsonAsync("/api/v1/radar", pedido, cancellationToken);

      resposta.EnsureSuccessStatusCode();

      return (await resposta.Content.ReadFromJsonAsync<MonitoramentoConsulta>(cancellationToken))!.Id;
    }));

    var id = Assert.Single(ids.Distinct());

    Assert.Single((await client.GetFromJsonAsync<MonitoramentoConsulta[]>("/api/v1/radar", cancellationToken))!);

    return id == Guid.Empty ? 0 : 1;
  }

}
