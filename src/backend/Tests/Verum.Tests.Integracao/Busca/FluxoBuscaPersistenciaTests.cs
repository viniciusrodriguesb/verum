extern alias VerumApi;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Verum.BuildingBlocks.Erros;
using Verum.CrossCutting;
using Verum.Modules.Busca;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.DTOs;
using Verum.Modules.Busca.Contratos.InteligenciaArtificial;
using Verum.Modules.Busca.Infraestrutura.Persistencia;
using Verum.Modules.Catalogo.Infraestrutura.Persistencia;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Infraestrutura.Persistencia;
using Verum.Worker.Descoberta.Configuracoes;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class RabbitMqFluxoFactAttribute : FactAttribute
{
  public RabbitMqFluxoFactAttribute()
  {
    if (Environment.GetEnvironmentVariable("VERUM_TEST_RABBITMQ") != "1"
      || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VERUM_TEST_POSTGRES")))
      Skip = "Configure VERUM_TEST_RABBITMQ=1 e VERUM_TEST_POSTGRES para testar o broker local.";
  }
}

public sealed class FluxoBuscaPersistenciaTests
{
  [PostgreSqlFact]
  public async Task VisitantePreparadoAntesDoPostRecuperaPedidoAceitoSemConsumirOutraCota()
  {
    await using var ambiente = await Ambiente.CriarAsync(limite: 1);

    await using var factory = ambiente.CriarApi();

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    using var sessao = await client.GetAsync("/api/v1/buscas/visitante");

    Assert.Equal(204, (int)sessao.StatusCode);

    Assert.Contains(sessao.Headers.GetValues("Set-Cookie"), x => x.Contains("verum.visitante") && x.Contains("httponly"));

    Assert.True(sessao.Headers.CacheControl!.NoStore);

    var pedido = new SolicitarBusca("ssd kingston", "resposta-perdida");

    using var original = await client.PostAsJsonAsync("/api/v1/buscas", pedido);

    using var retomada = await client.PostAsJsonAsync("/api/v1/buscas", pedido);

    Assert.Equal(202, (int)original.StatusCode);

    Assert.Equal(202, (int)retomada.StatusCode);

    Assert.Equal((await original.Content.ReadFromJsonAsync<BuscaAceita>())!.Id,
      (await retomada.Content.ReadFromJsonAsync<BuscaAceita>())!.Id);

    await using var scope = ambiente.Host.Services.CreateAsyncScope();

    Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<BuscaDbContext>().Buscas.CountAsync());
  }

  [RabbitMqFluxoFact]
  public async Task RabbitMqGuardaPrimeiraSolicitacaoAntesDoWorkerExistir()
  {
    await using var ambiente = await Ambiente.CriarAsync(rabbit: true);

    await using var factory = ambiente.CriarApi();

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    using var response = await client.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("ssd kingston", "rabbit-1"));

    Assert.Equal(202, (int)response.StatusCode);

    var aceito = (await response.Content.ReadFromJsonAsync<BuscaAceita>())!;

    using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    while (true)
    {
      await using var scope = ambiente.Host.Services.CreateAsyncScope();

      if (!await scope.ServiceProvider.GetRequiredService<BuscaDbContext>().Set<OutboxMessage>().AnyAsync(prazo.Token)) break;

      await Task.Delay(100, prazo.Token);
    }

    // A Outbox entregou ao broker, mas o worker deste teste ainda não foi iniciado.
    Assert.Equal("Recebida", (await client.GetFromJsonAsync<BuscaConsulta>(response.Headers.Location, prazo.Token))!.Status);

    await ambiente.Host.StartAsync(prazo.Token);

    BuscaConsulta resultado;

    do
    {
      resultado = (await client.GetFromJsonAsync<BuscaConsulta>(response.Headers.Location, prazo.Token))!;

      if (resultado.Status is "Recebida" or "Processando") await Task.Delay(100, prazo.Token);
    }
    while (resultado.Status is "Recebida" or "Processando");

    Assert.Equal("Concluida", resultado.Status);

    Assert.Single(resultado.Ofertas);
  }

  [PostgreSqlFact]
  public async Task ApiRetorna202ProcessaResultadoEProtegeCookieCotaEPropriedade()
  {
    await using var ambiente = await Ambiente.CriarAsync(limite: 1);

    await using var factory = ambiente.CriarApi();

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    using var response = await client.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("ssd kingston", "http-1"));

    Assert.Equal(202, (int)response.StatusCode);

    var aceito = (await response.Content.ReadFromJsonAsync<BuscaAceita>())!;

    Assert.NotNull(response.Headers.Location);

    Assert.Contains(response.Headers.GetValues("Set-Cookie"), x => x.Contains("verum.visitante") && x.Contains("httponly") && x.Contains("secure"));

    BuscaConsulta? resultado = null;

    using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    do
    {
      resultado = await client.GetFromJsonAsync<BuscaConsulta>(response.Headers.Location, prazo.Token);

      if (resultado!.Status is "Recebida" or "Processando") await Task.Delay(100, prazo.Token);
    }
    while (resultado!.Status is "Recebida" or "Processando");

    Assert.Equal("Concluida", resultado.Status);

    Assert.Single(resultado.Ofertas);

    using var repetida = await client.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("ssd kingston", "http-1"));

    Assert.Equal(aceito.Id, (await repetida.Content.ReadFromJsonAsync<BuscaAceita>())!.Id);

    using var cota = await client.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca("ssd kingston", "http-2"));

    Assert.Equal(429, (int)cota.StatusCode);

    using var outro = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    using var privado = await outro.GetAsync($"/api/v1/buscas/{aceito.Id}/resultado");

    Assert.Equal(404, (int)privado.StatusCode);

    using var adulterado = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/buscas/{aceito.Id}");

    adulterado.Headers.Add("Cookie", "verum.visitante=valor-adulterado");

    using var proibido = await outro.SendAsync(adulterado);

    Assert.Equal(404, (int)proibido.StatusCode);

    using var invalida = await client.PostAsJsonAsync("/api/v1/buscas", new SolicitarBusca(new string('a', 501), "invalida"));

    Assert.Equal(400, (int)invalida.StatusCode);
  }

  [PostgreSqlFact]
  public async Task OutboxRetomaEntregaProcessaSnapshotEReentregaNaoDuplica()
  {
    await using var ambiente = await Ambiente.CriarAsync();

    var visitante = Guid.NewGuid();

    Guid id;

    await using (var scope = ambiente.Host.Services.CreateAsyncScope())
    {
      var service = scope.ServiceProvider.GetRequiredService<IBuscaService>();

      id = (await service.SolicitarAsync(visitante, new("ssd kingston", "pedido-1"), CancellationToken.None)).Id;

      var db = scope.ServiceProvider.GetRequiredService<BuscaDbContext>();

      Assert.Equal(1, await db.Set<OutboxMessage>().CountAsync());

      Assert.Equal("Recebida", (await service.ObterAsync(visitante, id, CancellationToken.None)).Status);
    }

    // Até aqui nenhum transporte/worker foi iniciado: a solicitação está durável.
    await ambiente.Host.StartAsync();

    var resultado = await ambiente.AguardarAsync(visitante, id);

    Assert.Equal("Concluida", resultado.Status);

    Assert.True(resultado.Parcial);

    Assert.Single(resultado.Ofertas);

    await using (var scope = ambiente.Host.Services.CreateAsyncScope())
    {
      var service = scope.ServiceProvider.GetRequiredService<IBuscaService>();

      Assert.Equal(id, (await service.SolicitarAsync(visitante, new("ssd kingston", "pedido-1"), CancellationToken.None)).Id);

      await Assert.ThrowsAsync<ErroAplicacaoException>(() =>
        service.SolicitarAsync(visitante, new("outro produto", "pedido-1"), CancellationToken.None));

      await Assert.ThrowsAsync<ErroAplicacaoException>(() =>
        service.ObterAsync(Guid.NewGuid(), id, CancellationToken.None));

      await scope.ServiceProvider.GetRequiredService<IProcessamentoBuscaService>().ProcessarAsync(id, CancellationToken.None);

      Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<BuscaDbContext>().Resultados.CountAsync());

      Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<OfertasDbContext>().Observacoes.CountAsync());
    }

    Assert.Equal(1, ambiente.Loja.Chamadas);
  }

  [PostgreSqlFact]
  public async Task CotaEIdempotenciaSaoAtomicasEntreSolicitacoesConcorrentes()
  {
    await using var ambiente = await Ambiente.CriarAsync(limite: 1);

    var visitante = Guid.NewGuid();

    async Task<Guid> Solicitar(string chave)
    {
      await using var scope = ambiente.Host.Services.CreateAsyncScope();

      return (await scope.ServiceProvider.GetRequiredService<IBuscaService>()
        .SolicitarAsync(visitante, new("ssd kingston", chave), CancellationToken.None)).Id;
    }

    var ids = await Task.WhenAll(Solicitar("igual"), Solicitar("igual"));

    Assert.Equal(ids[0], ids[1]);

    var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Solicitar("nova"));

    Assert.Equal(TipoErro.LimiteExcedido, erro.Tipo);
  }

  [PostgreSqlFact]
  public async Task FalhaTotalEVazioTemEstadosDiferentes()
  {
    await using var ambiente = await Ambiente.CriarAsync();

    await ambiente.Host.StartAsync();

    var visitante = Guid.NewGuid();

    ambiente.Loja.Falha = true;

    var falha = await ambiente.SolicitarEAguardarAsync(visitante, "falha");

    Assert.Equal("Falhou", falha.Status);

    ambiente.Loja.Falha = false;

    ambiente.Loja.Vazia = true;

    var vazio = await ambiente.SolicitarEAguardarAsync(visitante, "vazia");

    Assert.Equal("ConcluidaSemResultado", vazio.Status);

    Assert.Empty(vazio.Ofertas);
  }

  [PostgreSqlFact]
  public async Task SegundaBuscaReutilizaOfertasValidasSemConsultarFontes()
  {
    await using var ambiente = await Ambiente.CriarAsync(minimoOfertas: 1);

    await ambiente.Host.StartAsync();

    var visitante = Guid.NewGuid();

    var primeira = await ambiente.SolicitarEAguardarAsync(visitante, "primeira");

    ambiente.Loja.Falha = true;

    var segunda = await ambiente.SolicitarEAguardarAsync(visitante, "segunda");

    Assert.Equal("Concluida", segunda.Status);

    Assert.Equal(primeira.Ofertas.Single().OfertaId, segunda.Ofertas.Single().OfertaId);

    Assert.Equal(1, ambiente.Loja.Chamadas);

    Assert.Equal(0, segunda.FontesConsultadas);
  }

  [PostgreSqlFact]
  public async Task FalhaDefinitivaFinalizaBuscaEReentregaNaoSobrescreve()
  {
    await using var ambiente = await Ambiente.CriarAsync();

    var visitante = Guid.NewGuid();

    await using var scope = ambiente.Host.Services.CreateAsyncScope();

    var service = scope.ServiceProvider.GetRequiredService<IBuscaService>();

    var pedido = await service.SolicitarAsync(visitante, new("ssd kingston", "falha-definitiva"), CancellationToken.None);

    var falhas = scope.ServiceProvider.GetRequiredService<IFalhaBuscaService>();

    await falhas.RegistrarAsync(pedido.Id, CancellationToken.None);

    await scope.ServiceProvider.GetRequiredService<IProcessamentoBuscaService>().ProcessarAsync(pedido.Id, CancellationToken.None);

    var resultado = await service.ObterAsync(visitante, pedido.Id, CancellationToken.None);

    Assert.Equal("Falhou", resultado.Status);

    Assert.Equal("PROCESSAMENTO_FALHOU", resultado.CodigoErro);

    Assert.Equal(0, ambiente.Loja.Chamadas);
  }

  internal sealed class Ambiente(IHost host, string conexaoAdmin, string banco, FluxoFontesTests.LojaFake loja, string conexao, int limite, bool rabbit) : IAsyncDisposable
  {
    private string Fila => banco.Replace('_', '-');

    public IHost Host => host;

    public FluxoFontesTests.LojaFake Loja => loja;

    public static async Task<Ambiente> CriarAsync(int limite = 10, bool rabbit = false, int minimoOfertas = 3,
      Action<IServiceCollection>? configurar = null)
    {
      var cs = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("VERUM_TEST_POSTGRES"));

      var banco = "verum_fluxo_test_" + Guid.NewGuid().ToString("N");

      var admin = cs.ConnectionString;

      await using (var connection = new NpgsqlConnection(admin))
      {
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{banco}\"", connection);

        await command.ExecuteNonQueryAsync();
      }

      cs.Database = banco;

      var loja = new FluxoFontesTests.LojaFake();

      var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().ConfigureServices(services =>
      {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["ConnectionStrings:PostgreSQL"] = cs.ConnectionString,
          ["RabbitMQ:Enabled"] = "false",
          ["Redis:Enabled"] = "false",
          ["Busca:LimiteVisitanteDiario"] = limite.ToString(),
          ["Busca:MinimoOfertas"] = minimoOfertas.ToString(),
          ["Busca:FilaSolicitacoes"] = banco.Replace('_', '-')
        }).Build();

        services.AddVerumWorkerDescoberta(config);

        services.AddDbContext<Verum.Modules.Contas.Infraestrutura.Persistencia.ContasDbContext>(options => options.UseNpgsql(cs.ConnectionString));

        // Este cenário mede reutilização e idempotência com uma fonte, independente do catálogo de providers padrão.
        services.PostConfigure<Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes.BuscaOptions>(options => options.Lojas = ["kabum"]);

        services.Replace(ServiceDescriptor.Singleton<IConsultaLoja>(loja));

        services.Replace(ServiceDescriptor.Singleton<IDescobertaProdutosIaService>(new FluxoFontesTests.IaFake { Disponivel = false }));

        services.AddMassTransit(bus =>
        {
          bus.AddBuscaOutbox();

          bus.AddConsumidoresDescoberta(banco.Replace('_', '-'));

          ConfigurarTransporte(bus, rabbit);
        });

        configurar?.Invoke(services);
      }).ConfigureLogging(log => log.ClearProviders()).Build();

      var ambiente = new Ambiente(host, admin, banco, loja, cs.ConnectionString, limite, rabbit);

      try
      {
        await using var scope = host.Services.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<BuscaDbContext>().Database.MigrateAsync();

        await scope.ServiceProvider.GetRequiredService<CatalogoDbContext>().Database.MigrateAsync();

        await scope.ServiceProvider.GetRequiredService<OfertasDbContext>().Database.MigrateAsync();

        await scope.ServiceProvider.GetRequiredService<Verum.Modules.Acesso.Infraestrutura.Persistencia.AcessoDbContext>().Database.MigrateAsync();

        await scope.ServiceProvider.GetRequiredService<Verum.Modules.Contas.Infraestrutura.Persistencia.ContasDbContext>().Database.MigrateAsync();

        return ambiente;
      }
      catch
      {
        await ambiente.DisposeAsync();

        throw;
      }
    }

    public WebApplicationFactory<VerumApi::Program> CriarApi()
    {
      var root = Path.Combine(AppContext.BaseDirectory, banco);

      Directory.CreateDirectory(root);

      File.WriteAllText(Path.Combine(root, "appsettings.json"), JsonSerializer.Serialize(new
      {
        ConnectionStrings = new { PostgreSQL = conexao },
        RabbitMQ = new { Enabled = false },
        Redis = new { Enabled = false },
        Authentication = new { Authority = "https://identity.invalid/realms/test", Audience = "verum-api" },
        Busca = new { LimiteVisitanteDiario = limite, FilaSolicitacoes = Fila }
      }));

      return new WebApplicationFactory<VerumApi::Program>().WithWebHostBuilder(builder =>
      {
        builder.UseContentRoot(root);

        builder.UseEnvironment("Development");

        builder.ConfigureLogging(log => log.ClearProviders());

        builder.ConfigureServices(services =>
        {
          services.Replace(ServiceDescriptor.Singleton<IConsultaLoja>(loja));

          services.Replace(ServiceDescriptor.Singleton<IDescobertaProdutosIaService>(new FluxoFontesTests.IaFake { Disponivel = false }));

          services.AddMassTransit(bus =>
          {
            bus.AddBuscaOutbox();

            if (!rabbit) bus.AddConsumidoresDescoberta(Fila);

            ConfigurarTransporte(bus, rabbit);
          });
        });
      });
    }

    private static void ConfigurarTransporte(IBusRegistrationConfigurator bus, bool rabbit)
    {
      if (rabbit)
        bus.UsingRabbitMq((context, transport) =>
        {
          transport.Host("localhost", "/", connection =>
          {
            connection.Username("admin");

            connection.Password("admin123");
          });

          transport.ConfigureEndpoints(context);
        });
      else
        bus.UsingInMemory((context, transport) => transport.ConfigureEndpoints(context));
    }

    public async Task<BuscaConsulta> SolicitarEAguardarAsync(Guid visitante, string chave)
    {
      await using var scope = host.Services.CreateAsyncScope();

      var pedido = await scope.ServiceProvider.GetRequiredService<IBuscaService>()
        .SolicitarAsync(visitante, new("ssd kingston", chave), CancellationToken.None);

      return await AguardarAsync(visitante, pedido.Id);
    }

    public async Task<BuscaConsulta> AguardarAsync(Guid visitante, Guid id)
    {
      using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(30));

      while (true)
      {
        await using var scope = host.Services.CreateAsyncScope();

        var resultado = await scope.ServiceProvider.GetRequiredService<IBuscaService>().ObterAsync(visitante, id, prazo.Token);

        if (resultado.Status is not ("Recebida" or "Processando")) return resultado;

        await Task.Delay(100, prazo.Token);
      }
    }

    public async ValueTask DisposeAsync()
    {
      await host.StopAsync();

      host.Dispose();

      if (rabbit)
      {
        using var http = new HttpClient { BaseAddress = new Uri("http://localhost:15672") };

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
          Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:admin123")));

        // Remove somente a topologia exclusiva deste teste, nunca filas da aplicação.
        foreach (var sufixo in new[] { "", "-falhas", "_error", "_skipped", "-falhas_error", "-falhas_skipped" })
        {
          using var fila = await http.DeleteAsync("/api/queues/%2F/" + Fila + sufixo);

          if (!fila.IsSuccessStatusCode && fila.StatusCode != System.Net.HttpStatusCode.NotFound)
            fila.EnsureSuccessStatusCode();

          using var exchange = await http.DeleteAsync("/api/exchanges/%2F/" + Fila + sufixo);

          if (!exchange.IsSuccessStatusCode && exchange.StatusCode != System.Net.HttpStatusCode.NotFound)
            exchange.EnsureSuccessStatusCode();
        }
      }

      NpgsqlConnection.ClearAllPools();

      await using var connection = new NpgsqlConnection(conexaoAdmin);

      await connection.OpenAsync();

      // Nome gerado internamente, exclusivo deste teste; nunca remove o banco da aplicação.
      if (!banco.StartsWith("verum_fluxo_test_", StringComparison.Ordinal)) throw new InvalidOperationException();

      await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{banco}\" WITH (FORCE)", connection);

      await command.ExecuteNonQueryAsync();
    }
  }
}
