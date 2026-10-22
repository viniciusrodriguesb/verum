using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Verum.BuildingBlocks.Erros;
using Verum.CrossCutting.Pipelines;
using Verum.Modules.Acesso;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Dominio;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;
using Verum.Modules.Catalogo.Contratos;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Notificacoes;
using Verum.Modules.Notificacoes.Contratos;
using Verum.Modules.Notificacoes.Contratos.DTOs;
using Verum.Modules.Notificacoes.Dominio;
using Verum.Modules.Notificacoes.Infraestrutura.Persistencia;
using Verum.Modules.Ofertas.Contratos.Radar;
using Verum.Modules.Ofertas.Contratos.Radar.DTOs;
using Verum.Modules.Radar;
using Verum.Modules.Radar.Contratos;
using Verum.Modules.Radar.Contratos.DTOs;
using Verum.Modules.Radar.Infraestrutura.Persistencia;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class FluxoRadarTests
{
  [PostgreSqlFact]
  public async Task FluxoCompletoIsolaContaDeduplicaAgrupaERecuperaAvisoPersistido()
  {
    await using var banco = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync();

    await using var original = banco.Host.Services.CreateAsyncScope();

    var conexao = original.ServiceProvider.GetRequiredService<AcessoDbContext>().Database.GetConnectionString();

    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["ConnectionStrings:PostgreSQL"] = conexao,
      ["Radar:Habilitado"] = "true",
      ["Radar:LimiteMonitoramentos"] = "1",
      ["Notificacoes:Email:Habilitado"] = "true",
      ["Notificacoes:Email:Host"] = "localhost",
      ["Notificacoes:Email:Remetente"] = "radar@example.test"
    }).Build();

    var fonte = new Fonte();

    var email = new Email();

    var services = new ServiceCollection().AddLogging();

    services.AddRadarPersistencia(config).AddNotificacoesPersistencia(config).AddAcessoPersistencia(config);

    services.AddVerumServices(typeof(Verum.Modules.Radar.DependencyInjection).Assembly,
      typeof(Verum.Modules.Notificacoes.DependencyInjection).Assembly, typeof(Verum.Modules.Acesso.DependencyInjection).Assembly);

    services.AddSingleton<IOfertaRadarService>(fonte);

    services.AddSingleton<ICatalogoDescobertaService>(new Catalogo());

    services.AddSingleton<IContatoContaService>(new Contato());

    services.Replace(ServiceDescriptor.Singleton<IEnvioEmail>(email));

    await using var provider = services.BuildServiceProvider();

    var conta = Guid.NewGuid();

    var outra = Guid.NewGuid();

    await using (var scope = provider.CreateAsyncScope())
    {
      await scope.ServiceProvider.GetRequiredService<RadarDbContext>().Database.MigrateAsync();

      await scope.ServiceProvider.GetRequiredService<NotificacoesDbContext>().Database.MigrateAsync();

      var acessoDb = scope.ServiceProvider.GetRequiredService<AcessoDbContext>();

      var free = new PacoteAcesso("FREE", "Free");

      acessoDb.Pacotes.Add(free);

      acessoDb.Concessoes.Add(new ConcessaoPacote(conta, free.Id, OrigemConcessao.Cadastro, DateTimeOffset.UtcNow));

      await acessoDb.SaveChangesAsync();

      Assert.False(await scope.ServiceProvider.GetRequiredService<IAcessoRadarService>().PermitidoAsync(conta, default));

      var buscaPlus = new PacoteAcesso("BUSCA_PLUS", "Busca Plus");

      acessoDb.Pacotes.Add(buscaPlus);

      acessoDb.Concessoes.Add(new ConcessaoPacote(conta, buscaPlus.Id, OrigemConcessao.Assinatura, DateTimeOffset.UtcNow));

      await acessoDb.SaveChangesAsync();

      Assert.False(await scope.ServiceProvider.GetRequiredService<IAcessoRadarService>().PermitidoAsync(conta, default));

      var plus = new PacoteAcesso("VERUM_PLUS", "Verum Plus");

      acessoDb.Pacotes.Add(plus);

      acessoDb.Concessoes.AddRange(new ConcessaoPacote(conta, plus.Id, OrigemConcessao.Assinatura, DateTimeOffset.UtcNow),
        new ConcessaoPacote(outra, plus.Id, OrigemConcessao.Assinatura, DateTimeOffset.UtcNow));

      await acessoDb.SaveChangesAsync();
    }

    var pedido = new CriarMonitoramento(fonte.Atual.Id, fonte.Atual.VarianteId, 900, "duas-abas", true, true);

    async Task<MonitoramentoConsulta> Criar(Guid idConta)
    {
      await using var scope = provider.CreateAsyncScope();

      return await scope.ServiceProvider.GetRequiredService<IRadarService>().CriarAsync(idConta, pedido, default);
    }

    var concorrentes = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Criar(conta)));

    Assert.Single(concorrentes.Select(x => x.Id).Distinct());

    var primeiro = concorrentes[0];

    await Criar(outra);

    fonte.Atual = fonte.Atual with { ValidaAte = DateTimeOffset.UtcNow.AddSeconds(-1) };

    Assert.Equal(primeiro.Id, (await Criar(conta)).Id);

    fonte.Atual = fonte.Atual with { ValidaAte = DateTimeOffset.UtcNow.AddMinutes(15) };

    await using (var scope = provider.CreateAsyncScope())
    {
      var radar = scope.ServiceProvider.GetRequiredService<IRadarService>();

      Assert.Single(await radar.ListarAsync(conta, default));

      var limite = await Assert.ThrowsAsync<ErroAplicacaoException>(() => radar.CriarAsync(conta,
        pedido with { ChaveIdempotencia = "nova-chave" }, default));

      Assert.Equal(TipoErro.LimiteExcedido, limite.Tipo);

      var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => radar.AlterarAsync(Guid.NewGuid(), primeiro.Id, 800, "editar", default));

      Assert.Equal(TipoErro.AcessoNegado, erro.Tipo);

      erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => radar.AlterarAsync(outra, primeiro.Id, 900, "pausar", default));

      Assert.Equal(TipoErro.NaoEncontrado, erro.Tipo);

      Assert.False((await scope.ServiceProvider.GetRequiredService<INotificacoesRadarService>()
        .PreferenciasAsync(conta, default)).EmailHabilitado);

      await scope.ServiceProvider.GetRequiredService<INotificacoesRadarService>()
        .SalvarPreferenciasAsync(conta, new(true, 0, 0, "UTC"), default);

      await scope.ServiceProvider.GetRequiredService<INotificacoesRadarService>()
        .SalvarPreferenciasAsync(outra, new(true, 0, 0, "UTC"), default);
    }

    fonte.Atual = fonte.Atual with { Preco = 800 };

    for (var i = 0; i < 2; i++)
    {
      await using var scope = provider.CreateAsyncScope();

      await scope.ServiceProvider.GetRequiredService<IProcessamentoRadarService>().VerificarAsync(primeiro.Id, default);
    }

    Assert.Equal(1, fonte.Consultas);

    // Novo escopo equivale à retomada após reinício: o registro do aviso é idempotente.
    for (var i = 0; i < 2; i++)
    {
      await using var scope = provider.CreateAsyncScope();

      await scope.ServiceProvider.GetRequiredService<IProcessamentoRadarService>().EncaminharAvisosAsync(default);
    }

    await using (var scope = provider.CreateAsyncScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<NotificacoesDbContext>();

      Assert.Equal(2, await db.Notificacoes.CountAsync());

      await scope.ServiceProvider.GetRequiredService<INotificacoesRadarService>().EnviarPendentesAsync(default);

      Assert.Contains(await db.Entregas.ToArrayAsync(), x => x.Status == StatusEntrega.AguardandoNovaTentativa);

      foreach (var entrega in await db.Entregas.ToArrayAsync()) entrega.Adiar(DateTimeOffset.UtcNow.AddMinutes(-1));

      await db.SaveChangesAsync();
    }

    await using (var scope = provider.CreateAsyncScope())
    {
      await scope.ServiceProvider.GetRequiredService<INotificacoesRadarService>().EnviarPendentesAsync(default);

      Assert.All(await scope.ServiceProvider.GetRequiredService<NotificacoesDbContext>().Entregas.ToArrayAsync(),
        x => Assert.Equal(StatusEntrega.Entregue, x.Status));

      Assert.Equal(2, email.Entregues.Count);

      var acessoDb = scope.ServiceProvider.GetRequiredService<AcessoDbContext>();

      foreach (var concessao in await acessoDb.Concessoes.Where(x => x.ContaId == conta && x.Origem == OrigemConcessao.Assinatura).ToArrayAsync())
      {
        acessoDb.Entry(concessao).Property(x => x.ValidaDe).CurrentValue = DateTimeOffset.UtcNow.AddDays(-2);

        acessoDb.Entry(concessao).Property(x => x.ValidaAte).CurrentValue = DateTimeOffset.UtcNow.AddSeconds(-1);

      }

      await acessoDb.SaveChangesAsync();

      var lista = await scope.ServiceProvider.GetRequiredService<IRadarService>().ListarAsync(conta, default);

      Assert.Equal("SuspensoPorAcesso", lista.Single().Estado);

      await Assert.ThrowsAsync<ErroAplicacaoException>(() => scope.ServiceProvider.GetRequiredService<IRadarService>()
        .AlterarAsync(conta, primeiro.Id, 850, "retomar", default));

      await scope.ServiceProvider.GetRequiredService<IRadarService>().AlterarAsync(conta, primeiro.Id, 900, "encerrar", default);

      Assert.Empty(await scope.ServiceProvider.GetRequiredService<IRadarService>().ListarAsync(conta, default));
    }
  }

  private sealed class Fonte : IOfertaRadarService
  {
    public int Consultas;

    public OfertaRadar Atual = new(Guid.NewGuid(), Guid.NewGuid(), 1, "iPhone 15 128GB", "https://loja.example/iphone",
      1000, "À vista no PIX", "kabum", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(15));

    public Task<OfertaRadar?> ObterAsync(Guid ofertaId, CancellationToken cancellationToken) => Task.FromResult<OfertaRadar?>(Atual);

    public Task<OfertaRadar?> AtualizarAsync(Guid ofertaId, CancellationToken cancellationToken)
    {
      Consultas++;

      return Task.FromResult<OfertaRadar?>(Atual);
    }
  }

  private sealed class Catalogo : ICatalogoDescobertaService
  {
    public Task<Guid?> ObterProdutoAsync(Guid varianteId, CancellationToken cancellationToken) => Task.FromResult<Guid?>(Guid.NewGuid());

    public Task<Guid[]> ResolverAsync(string consulta, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<Guid> RegistrarAsync(string consulta, string nome, string url, CancellationToken cancellationToken) => throw new NotSupportedException();
  }

  private sealed class Contato : IContatoContaService
  {
    public Task<string?> EmailAtivoAsync(Guid contaId, CancellationToken cancellationToken) => Task.FromResult<string?>("teste@example.test");
  }

  private sealed class Email : IEnvioEmail
  {
    private bool falhou;

    public HashSet<Guid> Entregues { get; } = [];

    public Task EnviarAsync(Guid id, string destinatario, string assunto, string texto, CancellationToken cancellationToken)
    {
      if (!falhou)
      {
        falhou = true;

        throw new IOException("SMTP indisponível no primeiro envio.");
      }

      Assert.True(Entregues.Add(id));

      return Task.CompletedTask;
    }
  }
}

