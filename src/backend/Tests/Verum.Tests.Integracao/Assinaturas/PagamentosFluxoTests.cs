extern alias VerumApi;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.Identidade;
using Verum.CrossCutting.Pipelines;
using Verum.Modules.Acesso;
using Verum.Modules.Acesso.Aplicacao;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Dominio;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;
using Verum.Modules.Assinaturas;
using Verum.Modules.Assinaturas.Aplicacao;
using Verum.Modules.Assinaturas.Aplicacao.Configuracoes;
using Verum.Modules.Assinaturas.Contratos;
using Verum.Modules.Assinaturas.Contratos.DTOs;
using Verum.Modules.Assinaturas.Dominio;
using Verum.Modules.Assinaturas.Infraestrutura.Gateways.MercadoPago.Configuracoes;
using Verum.Modules.Assinaturas.Infraestrutura.Persistencia;
using Verum.Modules.Assinaturas.Infraestrutura.Processadores;
using Xunit;
using Xunit.Abstractions;

namespace Verum.Tests.Integracao;

public sealed class PagamentosFluxoTests(ITestOutputHelper output)
{
  [PostgreSqlFact]
  public async Task MigrationRecusaDescartarCamposAntigosPreenchidosEPreservaPagamento()
  {
    await using var ambiente = await CriarAsync();

    await CriarAssinaturaAsync(ambiente, Guid.NewGuid());

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.StatusPagamento = "approved";

    estado.PagamentoId = 1234;

    await ReceberAsync(ambiente, estado.Id, "subscription_preapproval", "migration-segura");

    await ProcessarAsync(ambiente);

    await using var scope = ambiente.Services.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>();

    var migrator = db.GetService<IMigrator>();

    await migrator.MigrateAsync(db.Database.GetMigrations().SkipLast(1).Last());

    await db.Database.ExecuteSqlRawAsync("UPDATE assinaturas.\"PAGAMENTO\" SET \"METODO\" = 1");

    var erro = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync());

    Assert.Equal("P0001", erro.SqlState);

    Assert.Equal(1, await db.Pagamentos.CountAsync());

    Assert.Equal(1, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM assinaturas.\"PAGAMENTO\" WHERE \"METODO\" = 1").SingleAsync());

    await db.Database.ExecuteSqlRawAsync("UPDATE assinaturas.\"PAGAMENTO\" SET \"METODO\" = NULL");

    await migrator.MigrateAsync();

    Assert.Equal(1, await db.Pagamentos.CountAsync());
  }

  [PostgreSqlFact]
  public async Task ConsultaAoGatewayNaoMantemTransacaoAbertaEAprovacaoContinuaAtomica()
  {
    await using var ambiente = await CriarAsync();

    var conta = Guid.NewGuid();

    await CriarAssinaturaAsync(ambiente, conta);

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.StatusPagamento = "approved";

    estado.PagamentoId = 1234;

    await ReceberAsync(ambiente, estado.Id, "subscription_preapproval", "sem-transacao-http");

    await using var scope = ambiente.Services.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>();

    var chamadas = 0;

    ambiente.Http.AntesEnviarAsync = () =>
    {
      Assert.Null(db.Database.CurrentTransaction);

      chamadas++;

      return Task.CompletedTask;
    };

    Assert.True(await scope.ServiceProvider.GetRequiredService<ProcessamentoPagamentos>().ProcessarProximoAsync(default));

    Assert.True(chamadas > 0);

    Assert.Equal(30, (await AcessoAsync(ambiente, conta)).Limite);
  }

  [PostgreSqlFact]
  public async Task VerumPlusSoLiberaRadarAposConfirmacaoDoPagamento()
  {
    await using var ambiente = await CriarAsync(radar: true);

    var conta = Guid.NewGuid();

    await using (var scope = ambiente.Services.CreateAsyncScope())
    {
      await scope.ServiceProvider.GetRequiredService<IAssinaturasService>()
        .CriarAsync(conta, "conta@example.test", new("VERUM_PLUS", "radar"), default);

      Assert.False(await scope.ServiceProvider.GetRequiredService<IAcessoRadarService>().PermitidoAsync(conta, default));
    }

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.PagamentoId = 801;

    estado.StatusPagamento = "pending";

    await ReceberAsync(ambiente, "801", "payment", "radar-pendente");

    await ProcessarAsync(ambiente);

    await using (var scope = ambiente.Services.CreateAsyncScope())
      Assert.False(await scope.ServiceProvider.GetRequiredService<IAcessoRadarService>().PermitidoAsync(conta, default));

    estado.StatusPagamento = "approved";

    await ReceberAsync(ambiente, "801", "payment", "radar-aprovado");

    await ProcessarAsync(ambiente);

    await using var verificar = ambiente.Services.CreateAsyncScope();

    Assert.True(await verificar.ServiceProvider.GetRequiredService<IAcessoRadarService>().PermitidoAsync(conta, default));
  }

  [PostgreSqlFact]
  public async Task FalhaDepoisDeGravarConcessaoDesfazPagamentoEBeneficioNaMesmaTransacao()
  {
    await using var ambiente = await CriarAsync(falhaAcesso: true);

    var conta = Guid.NewGuid();

    await CriarAssinaturaAsync(ambiente, conta);

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.PagamentoId = 501;

    estado.StatusPagamento = "approved";

    await ReceberAsync(ambiente, "501", "payment", "rollback");

    await ProcessarAsync(ambiente);

    await using var scope = ambiente.Services.CreateAsyncScope();

    Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>().Pagamentos.CountAsync());

    Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AcessoDbContext>().Concessoes.CountAsync(x => x.Origem == OrigemConcessao.Assinatura));

    Assert.Equal(StatusEventoGateway.Falhou, (await scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>().EventosGateway.SingleAsync()).Status);
  }

  [PostgreSqlFact]
  public async Task ApiExigeContaIsolaAssinaturaEValidaWebhookAntesDePersistir()
  {
    await using var ambiente = await CriarAsync();

    await using var factory = ambiente.Banco.CriarApi().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
    {
      services.AddAuthentication("IdentidadeTeste").AddScheme<AuthenticationSchemeOptions, AcessoBuscaApiTests.IdentidadeTesteHandler>("IdentidadeTeste", _ => { });

      services.Configure<AssinaturasOptions>(x =>
      {
        x.Habilitado = true;

        x.PrecosMensais = new() { ["BUSCA_PLUS"] = 19.90m };
      });

      services.Configure<MercadoPagoOptions>(x =>
      {
        x.AccessToken = "teste";

        x.WebhookSecret = "segredo-teste";

        x.CollectorId = 123;
      });

      services.AddHttpClient("MercadoPago").ConfigurePrimaryHttpMessageHandler(() => ambiente.Http);

      var worker = services.Single(x => x.ImplementationType == typeof(PagamentosWorker));

      services.Remove(worker);
    }));

    using var navegador = factory.CreateClient();

    Assert.Equal(HttpStatusCode.Unauthorized, (await navegador.PostAsJsonAsync("/api/v1/assinaturas", new SolicitarAssinatura("BUSCA_PLUS", "api"))).StatusCode);

    navegador.DefaultRequestHeaders.Add("X-Identidade-Teste", Guid.NewGuid().ToString());

    var pedido = await navegador.PostAsJsonAsync("/api/v1/assinaturas", new SolicitarAssinatura("BUSCA_PLUS", "api"));

    Assert.Equal(HttpStatusCode.OK, pedido.StatusCode);

    var assinatura = (await pedido.Content.ReadFromJsonAsync<AssinaturaConsulta>())!;

    Assert.Single((await navegador.GetFromJsonAsync<AssinaturaConsulta[]>("/api/v1/assinaturas"))!);

    var recurso = ambiente.Http.Assinaturas.Keys.Single();

    using var semAssinatura = await navegador.PostAsJsonAsync("/api/v1/webhooks/mercadopago?data.id=" + recurso,
      new { type = "subscription_preapproval", data = new { id = recurso } });

    Assert.Equal(HttpStatusCode.Unauthorized, semAssinatura.StatusCode);

    ambiente.Http.Indisponivel = true;

    var manifesto = $"id:{recurso};request-id:api-assinada;ts:1704908010;";

    var assinaturaWebhook = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("segredo-teste"), Encoding.UTF8.GetBytes(manifesto)));

    navegador.DefaultRequestHeaders.Add("x-request-id", "api-assinada");

    navegador.DefaultRequestHeaders.Add("x-signature", "ts=1704908010,v1=" + assinaturaWebhook);

    // O recebimento não depende da disponibilidade do gateway.
    using var assinada = await navegador.PostAsJsonAsync("/api/v1/webhooks/mercadopago?data.id=" + recurso,
      new { type = "subscription_preapproval", data = new { id = recurso } });

    Assert.Equal(HttpStatusCode.OK, assinada.StatusCode);

    ambiente.Http.Indisponivel = false;

    navegador.DefaultRequestHeaders.Remove("X-Identidade-Teste");

    navegador.DefaultRequestHeaders.Add("X-Identidade-Teste", Guid.NewGuid().ToString());

    Assert.Equal(HttpStatusCode.NotFound, (await navegador.GetAsync("/api/v1/assinaturas/" + assinatura.Id)).StatusCode);

    Assert.Equal(HttpStatusCode.NotFound, (await navegador.PostAsJsonAsync("/api/v1/assinaturas/" + assinatura.Id + "/cancelamento", new { })).StatusCode);
  }

  [PostgreSqlFact]
  public async Task RenovacaoCriaNovoPeriodoSemDuplicarEConcessaoRespeitaVigencia()
  {
    await using var ambiente = await CriarAsync();

    var conta = Guid.NewGuid();

    var pedido = await CriarAssinaturaAsync(ambiente, conta);

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.PagamentoId = 301;

    estado.StatusPagamento = "approved";

    await ReceberAsync(ambiente, "301", "payment", "primeiro-periodo");

    await ProcessarAsync(ambiente);

    estado.PagamentoId = 302;

    estado.Inicio = estado.Inicio.AddMonths(1);

    estado.AtualizadaEm = DateTimeOffset.UtcNow.AddSeconds(1);

    // A cobrança futura agendada ainda não é pagamento.
    estado.StatusPagamento = "pending";

    await ReceberAsync(ambiente, "302", "payment", "renovacao-pendente");

    await ProcessarAsync(ambiente);

    await using (var scope = ambiente.Services.CreateAsyncScope())
      Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<AcessoDbContext>().Concessoes.CountAsync(x => x.Origem == OrigemConcessao.Assinatura));

    estado.StatusPagamento = "approved";

    // O provedor confirma antecipadamente; início do benefício continua na data da fatura.
    estado.Inicio = DateTimeOffset.UtcNow.AddMinutes(1);

    await ReceberAsync(ambiente, "302", "payment", "renovacao-aprovada");

    await ProcessarAsync(ambiente);

    await using var verificar = ambiente.Services.CreateAsyncScope();

    Assert.Equal(2, await verificar.ServiceProvider.GetRequiredService<AssinaturasDbContext>().Pagamentos.CountAsync());

    Assert.Equal(2, await verificar.ServiceProvider.GetRequiredService<AcessoDbContext>().Concessoes.CountAsync(x => x.Origem == OrigemConcessao.Assinatura));

    Assert.Equal(30, (await AcessoAsync(ambiente, conta)).Limite);

    Assert.Equal(1, ambiente.Http.Criacoes);
  }

  [PostgreSqlFact]
  public async Task CargaDeWebhooksConcorrentesLiberaUmaConcessaoSemDuplicarPagamento()
  {
    await using var ambiente = await CriarAsync();

    var conta = Guid.NewGuid();

    var pedido = await CriarAssinaturaAsync(ambiente, conta);

    var estado = ambiente.Http.Assinaturas.Values.Single();

    // Autorização sem cobrança aprovada não libera recursos.
    estado.Status = "authorized";

    await ReceberAsync(ambiente, estado.Id, "subscription_preapproval", "autorizada");

    await ProcessarAsync(ambiente);

    Assert.Equal("Free", (await AcessoAsync(ambiente, conta)).Contexto);

    estado.PagamentoId = 777;

    estado.StatusPagamento = "approved";

    var cronometro = Stopwatch.StartNew();

    await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => ReceberAsync(ambiente, "777", "subscription_authorized_payment", "duplicada")));

    await using (var scope = ambiente.Services.CreateAsyncScope())
      Assert.Equal(2, await scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>().EventosGateway.CountAsync());

    await Task.WhenAll(Enumerable.Range(0, 100).Select(x => ReceberAsync(ambiente, "777", "subscription_authorized_payment", "evento-" + x)));

    await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => ProcessarAsync(ambiente)));

    output.WriteLine("Carga local: 200 entregas, quatro processadores, duração {0:F2}s.", cronometro.Elapsed.TotalSeconds);

    await using var verificar = ambiente.Services.CreateAsyncScope();

    var assinaturas = verificar.ServiceProvider.GetRequiredService<AssinaturasDbContext>();

    var acesso = verificar.ServiceProvider.GetRequiredService<AcessoDbContext>();

    Assert.Equal(1, await assinaturas.Pagamentos.CountAsync());

    Assert.Equal(1, await acesso.Concessoes.CountAsync(x => x.ContaId == conta && x.Origem == OrigemConcessao.Assinatura));

    Assert.Equal(102, await assinaturas.EventosGateway.CountAsync(x => x.Status == StatusEventoGateway.Processado));

    Assert.Equal(30, (await AcessoAsync(ambiente, conta)).Limite);

    Assert.Equal(pedido.Id, (await CriarAssinaturaAsync(ambiente, conta)).Id);

    Assert.Equal(1, ambiente.Http.Criacoes);

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => verificar.ServiceProvider.GetRequiredService<IAssinaturasService>()
      .ObterAsync(Guid.NewGuid(), pedido.Id, default));
  }

  [PostgreSqlFact]
  public async Task FalhaRetentaEstornoRevogaECancelamentoPreservaPeriodoPago()
  {
    await using var ambiente = await CriarAsync();

    var conta = Guid.NewGuid();

    var pedido = await CriarAssinaturaAsync(ambiente, conta);

    await using (var scope = ambiente.Services.CreateAsyncScope())
    {
      var acesso = scope.ServiceProvider.GetRequiredService<AcessoDbContext>();

      for (var i = 0; i < 3; i++)
        acesso.RegistrosUso.Add(new RegistroUso("BUSCA_EXECUTAR", 1, DateTimeOffset.UtcNow, contaId: conta, referenciaId: Guid.NewGuid()));

      await acesso.SaveChangesAsync();
    }

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.PagamentoId = 101;

    estado.StatusPagamento = "approved";

    ambiente.Http.Indisponivel = true;

    await ReceberAsync(ambiente, "101", "payment", "falha");

    await ProcessarAsync(ambiente);

    Assert.Equal("Free", (await AcessoAsync(ambiente, conta)).Contexto);

    ambiente.Http.Indisponivel = false;

    await using (var scope = ambiente.Services.CreateAsyncScope())
      await scope.ServiceProvider.GetRequiredService<AssinaturasDbContext>().Database.ExecuteSqlRawAsync(
        """UPDATE assinaturas."EVENTO_GATEWAY" SET "PROXIMA_TENTATIVA_EM" = now() WHERE "STATUS" = 3""");

    await ProcessarAsync(ambiente);

    Assert.Equal("Pago", (await AcessoAsync(ambiente, conta)).Contexto);

    Assert.Equal(27, (await AcessoAsync(ambiente, conta)).Restante);

    await using (var scope = ambiente.Services.CreateAsyncScope())
      await scope.ServiceProvider.GetRequiredService<IAssinaturasService>().CancelarAsync(conta, pedido.Id, default);

    await ProcessarAsync(ambiente);

    Assert.Equal("cancelled", estado.Status);

    Assert.Equal("Pago", (await AcessoAsync(ambiente, conta)).Contexto);

    estado.StatusPagamento = "refunded";

    estado.AtualizadaEm = DateTimeOffset.UtcNow.AddSeconds(1);

    await ReceberAsync(ambiente, "101", "payment", "estorno");

    await ProcessarAsync(ambiente);

    Assert.Equal("Free", (await AcessoAsync(ambiente, conta)).Contexto);

    Assert.Equal(0, (await AcessoAsync(ambiente, conta)).Restante);

    // Mesmo uma leitura antiga de aprovação não reativa um pagamento já estornado.
    estado.StatusPagamento = "approved";

    estado.AtualizadaEm = DateTimeOffset.UtcNow.AddMinutes(-1);

    await ReceberAsync(ambiente, "101", "payment", "atrasada");

    await ProcessarAsync(ambiente);

    Assert.Equal("Free", (await AcessoAsync(ambiente, conta)).Contexto);
  }

  [PostgreSqlFact]
  public async Task CriacaoAmbiguaERecuperadaSemSegundoPostEInconsistenciasNaoLiberamAcesso()
  {
    await using var ambiente = await CriarAsync();

    var conta = Guid.NewGuid();

    ambiente.Http.PerderRespostaCriacao = true;

    await Assert.ThrowsAsync<HttpRequestException>(() => CriarAssinaturaAsync(ambiente, conta));

    ambiente.Http.PerderRespostaCriacao = false;

    var recuperada = await CriarAssinaturaAsync(ambiente, conta);

    Assert.NotNull(recuperada.UrlCheckout);

    Assert.Equal(1, ambiente.Http.Criacoes);

    var estado = ambiente.Http.Assinaturas.Values.Single();

    estado.PagamentoId = 202;

    estado.StatusPagamento = "approved";

    ambiente.Http.ValorIncorreto = true;

    await ReceberAsync(ambiente, "202", "subscription_authorized_payment", "valor-incorreto");

    await ProcessarAsync(ambiente);

    Assert.Equal("Free", (await AcessoAsync(ambiente, conta)).Contexto);

    ambiente.Http.ValorIncorreto = false;

    ambiente.Http.ColetorIncorreto = true;

    await ReceberAsync(ambiente, "202", "payment", "coletor-incorreto");

    await ProcessarAsync(ambiente);

    Assert.Equal("Free", (await AcessoAsync(ambiente, conta)).Contexto);

    await using var scope = ambiente.Services.CreateAsyncScope();

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => scope.ServiceProvider.GetRequiredService<IWebhookPagamentoService>()
      .ReceberAsync("MercadoPago", "202", "payment", "ts=1,v1=00", "falsa", default));

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => scope.ServiceProvider.GetRequiredService<IAssinaturasService>()
      .CriarAsync(conta, "conta@example.test", new("VERUM_PLUS", "chave"), default));
  }

  private static async Task<AssinaturaConsulta> CriarAssinaturaAsync(Ambiente ambiente, Guid conta)
  {
    await using var scope = ambiente.Services.CreateAsyncScope();

    return await scope.ServiceProvider.GetRequiredService<IAssinaturasService>()
      .CriarAsync(conta, "conta@example.test", new("BUSCA_PLUS", "chave"), default);
  }

  private static async Task<Verum.Modules.Acesso.Contratos.DTOs.AcessoBusca> AcessoAsync(Ambiente ambiente, Guid conta)
  {
    await using var scope = ambiente.Services.CreateAsyncScope();

    return await scope.ServiceProvider.GetRequiredService<IAcessoBuscaService>().ObterAsync(AtorBusca.Conta(conta), default);
  }

  private static async Task ReceberAsync(Ambiente ambiente, string recurso, string tipo, string request)
  {
    await using var scope = ambiente.Services.CreateAsyncScope();

    var manifesto = $"id:{recurso};request-id:{request};ts:1704908010;";

    var hash = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("segredo-teste"), Encoding.UTF8.GetBytes(manifesto)));

    await scope.ServiceProvider.GetRequiredService<IWebhookPagamentoService>()
      .ReceberAsync("MercadoPago", recurso, tipo, "ts=1704908010,v1=" + hash, request, default);
  }

  private static async Task ProcessarAsync(Ambiente ambiente)
  {
    for (var i = 0; i < 200; i++)
    {
      await using var scope = ambiente.Services.CreateAsyncScope();

      if (!await scope.ServiceProvider.GetRequiredService<ProcessamentoPagamentos>().ProcessarProximoAsync(default)) return;
    }

    throw new InvalidOperationException("Fila não esvaziou.");
  }

  private static async Task<Ambiente> CriarAsync(bool falhaAcesso = false, bool radar = false)
  {
    var banco = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync();

    await using var scope = banco.Host.Services.CreateAsyncScope();

    var conexao = scope.ServiceProvider.GetRequiredService<AcessoDbContext>().Database.GetConnectionString();

    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["ConnectionStrings:PostgreSQL"] = conexao,
      ["Assinaturas:Habilitado"] = "true",
      ["Assinaturas:PrecosMensais:BUSCA_PLUS"] = "19.90",
      ["Assinaturas:PrecosMensais:VERUM_PLUS"] = "39.90",
      ["Assinaturas:MercadoPago:AccessToken"] = "teste",
      ["Assinaturas:MercadoPago:WebhookSecret"] = "segredo-teste",
      ["Assinaturas:MercadoPago:CollectorId"] = "123",
      ["Assinaturas:MercadoPago:UrlRetorno"] = "https://verum.example.test/",
      ["Assinaturas:MercadoPago:UrlWebhook"] = "https://verum.example.test/api/v1/webhooks/mercadopago"
    }).Build();

    var http = new MercadoPagoHttpFake();

    var services = new ServiceCollection();

    services.AddLogging();

    services.AddAcessoPersistencia(config);

    services.AddAssinaturasPersistencia(config);

    services.AddAssinaturasProcessamento();

    services.Configure<Verum.Modules.Radar.Contratos.Configuracoes.RadarOptions>(x => x.Habilitado = radar);

    services.AddVerumServices(typeof(Verum.Modules.Acesso.DependencyInjection).Assembly, typeof(Verum.Modules.Assinaturas.DependencyInjection).Assembly);

    if (falhaAcesso)
      services.AddScoped<IConcessoesAssinaturaService>(provider => new FalhaConcessao(provider.GetRequiredService<ConcessoesAssinaturaService>()));

    services.AddHttpClient("MercadoPago").ConfigurePrimaryHttpMessageHandler(() => http);

    var provider = services.BuildServiceProvider();

    try
    {
      await using var migracao = provider.CreateAsyncScope();

      await migracao.ServiceProvider.GetRequiredService<AssinaturasDbContext>().Database.MigrateAsync();

      return new(banco, provider, http);
    }
    catch
    {
      await provider.DisposeAsync();

      await banco.DisposeAsync();

      throw;
    }
  }

  private sealed record Ambiente(FluxoBuscaPersistenciaTests.Ambiente Banco, ServiceProvider Services, MercadoPagoHttpFake Http) : IAsyncDisposable
  {
    public async ValueTask DisposeAsync()
    {
      await Services.DisposeAsync();

      await Banco.DisposeAsync();
    }
  }

  private sealed class FalhaConcessao(IConcessoesAssinaturaService real) : IConcessoesAssinaturaService
  {
    public async Task AplicarAsync(Guid contaId, Guid pagamentoId, string pacote, DateTimeOffset inicio, DateTimeOffset fim,
      bool aprovado, System.Data.Common.DbConnection conexao, System.Data.Common.DbTransaction transacao, CancellationToken cancellationToken)
    {
      await real.AplicarAsync(contaId, pagamentoId, pacote, inicio, fim, aprovado, conexao, transacao, cancellationToken);

      throw new InvalidOperationException("Falha simulada após gravar a concessão.");
    }
  }
}
