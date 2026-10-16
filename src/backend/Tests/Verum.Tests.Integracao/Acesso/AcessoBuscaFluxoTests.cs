using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Verum.BuildingBlocks.Erros;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Dominio;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;
using Verum.Modules.Busca.Aplicacao.Descoberta;
using Verum.Modules.Busca.Aplicacao.Descoberta.Configuracoes;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Busca.Infraestrutura.Persistencia;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class AcessoBuscaFluxoTests
{
  [PostgreSqlFact]
  public async Task ContextosConcorrenciaConcessoesRevogacaoEPropriedadeSaoIsolados()
  {
    await using var ambiente = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync(limite: 1);

    var identificador = Guid.NewGuid();

    var conta = AtorBusca.Conta(identificador);

    var visitante = AtorBusca.Visitante(identificador);

    async Task<Guid?> Solicitar(AtorBusca ator, string chave)
    {
      await using var scope = ambiente.Host.Services.CreateAsyncScope();

      try
      {
        return (await scope.ServiceProvider.GetRequiredService<IBuscaService>()
          .SolicitarAsync(ator, new("ssd kingston", chave), default)).Id;
      }
      catch (ErroAplicacaoException erro) when (erro.Tipo == TipoErro.LimiteExcedido)
      {
        return null;
      }
    }

    var aceitas = await Task.WhenAll(Enumerable.Range(0, 8).Select(x => Solicitar(conta, "concorrente-" + x)));

    Assert.Equal(3, aceitas.Count(x => x.HasValue));

    var primeira = aceitas.First(x => x.HasValue)!.Value;

    Assert.NotNull(await Solicitar(visitante, "visitante"));

    await using var verificar = ambiente.Host.Services.CreateAsyncScope();

    var acesso = verificar.ServiceProvider.GetRequiredService<IAcessoBuscaService>();

    var db = verificar.ServiceProvider.GetRequiredService<AcessoDbContext>();

    Assert.Equal(3, (await acesso.ObterAsync(conta, default)).Utilizado);

    Assert.Equal(1, (await acesso.ObterAsync(visitante, default)).Utilizado);

    Assert.Equal(1, await db.Concessoes.CountAsync(x => x.ContaId == identificador));

    var service = verificar.ServiceProvider.GetRequiredService<IBuscaService>();

    Assert.Equal(primeira, (await service.ObterAsync(conta, primeira, default)).Id);

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => service.ObterAsync(visitante, primeira, default));

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => service.ObterAsync(AtorBusca.Conta(Guid.NewGuid()), primeira, default));

    var pacote = await db.Pacotes.SingleAsync(x => x.Codigo == "BUSCA_PLUS");

    var concessao = new ConcessaoPacote(identificador, pacote.Id, OrigemConcessao.Administracao, DateTimeOffset.UtcNow);

    db.Concessoes.Add(concessao);

    await db.SaveChangesAsync();

    var pago = await acesso.ObterAsync(conta, default);

    Assert.Equal("Pago", pago.Contexto);

    Assert.Equal(30, pago.Limite);

    Assert.Equal(27, pago.Restante);

    var outroPago = await db.Pacotes.SingleAsync(x => x.Codigo == "VERUM_PLUS");

    var outraConcessao = new ConcessaoPacote(identificador, outroPago.Id, OrigemConcessao.Assinatura, DateTimeOffset.UtcNow);

    db.Concessoes.Add(outraConcessao);

    await db.SaveChangesAsync();

    Assert.Equal(30, (await acesso.ObterAsync(conta, default)).Limite);

    concessao.Revogar();

    outraConcessao.Revogar();

    await db.SaveChangesAsync();

    Assert.Equal("Free", (await acesso.ObterAsync(conta, default)).Contexto);

    Assert.Equal(0, (await acesso.ObterAsync(conta, default)).Restante);

    var free = await db.Concessoes.Include(x => x.Pacote).SingleAsync(x => x.ContaId == identificador && x.Pacote.Codigo == "FREE");

    free.Revogar();

    await db.SaveChangesAsync();

    await acesso.PrepararAsync(conta, default);

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => acesso.ObterAsync(conta, default));

    Assert.Equal(3, await db.Concessoes.CountAsync(x => x.ContaId == identificador));
  }

  [PostgreSqlFact]
  public async Task RollbackDaAceitacaoNaoConsomeCotaERepeticaoNaoConsomeDuasVezes()
  {
    await using var ambiente = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync(limite: 1);

    var ator = AtorBusca.Visitante(Guid.NewGuid());

    await using (var scope = ambiente.Host.Services.CreateAsyncScope())
    {
      var service = new BuscaService(scope.ServiceProvider.GetRequiredService<BuscaDbContext>(),
        scope.ServiceProvider.GetRequiredService<IOptions<BuscaOptions>>(), scope.ServiceProvider.GetRequiredService<IBuscaCache>(),
        scope.ServiceProvider.GetRequiredService<IAcessoBuscaService>(), new EnvioFalho());

      await Assert.ThrowsAsync<InvalidOperationException>(() => service.SolicitarAsync(ator, new("ssd kingston", "falha-envio"), default));
    }

    await using var verificar = ambiente.Host.Services.CreateAsyncScope();

    Assert.Equal(0, await verificar.ServiceProvider.GetRequiredService<AcessoDbContext>().RegistrosUso.CountAsync());

    Assert.Equal(0, await verificar.ServiceProvider.GetRequiredService<BuscaDbContext>().Buscas.CountAsync());

    var busca = verificar.ServiceProvider.GetRequiredService<IBuscaService>();

    var original = await busca.SolicitarAsync(ator, new("ssd kingston", "mesma-chave"), default);

    var repetida = await busca.SolicitarAsync(ator, new("ssd kingston", "mesma-chave"), default);

    Assert.Equal(original.Id, repetida.Id);

    Assert.Equal(1, await verificar.ServiceProvider.GetRequiredService<AcessoDbContext>().RegistrosUso.CountAsync());
  }

  [PostgreSqlFact]
  public async Task ConcessoesFuturasExpiradasEPacotesInativosNaoElevamLimiteERecursoBloqueia()
  {
    await using var ambiente = await FluxoBuscaPersistenciaTests.Ambiente.CriarAsync();

    await using var scope = ambiente.Host.Services.CreateAsyncScope();

    var ator = AtorBusca.Conta(Guid.NewGuid());

    var acesso = scope.ServiceProvider.GetRequiredService<IAcessoBuscaService>();

    await acesso.PrepararAsync(ator, default);

    var db = scope.ServiceProvider.GetRequiredService<AcessoDbContext>();

    var pacote = await db.Pacotes.SingleAsync(x => x.Codigo == "BUSCA_PLUS");

    db.Concessoes.Add(new ConcessaoPacote(ator.ContaId!.Value, pacote.Id, OrigemConcessao.Assinatura,
      DateTimeOffset.UtcNow.AddDays(-2), validaAte: DateTimeOffset.UtcNow.AddDays(-1)));

    db.Concessoes.Add(new ConcessaoPacote(ator.ContaId.Value, pacote.Id, OrigemConcessao.Assinatura, DateTimeOffset.UtcNow.AddDays(1)));

    await db.SaveChangesAsync();

    Assert.Equal("Free", (await acesso.ObterAsync(ator, default)).Contexto);

    db.Concessoes.Add(new ConcessaoPacote(ator.ContaId.Value, pacote.Id, OrigemConcessao.Administracao, DateTimeOffset.UtcNow));

    pacote.DefinirAtivo(false);

    await db.SaveChangesAsync();

    Assert.Equal(3, (await acesso.ObterAsync(ator, default)).Limite);

    var pacoteVisitante = await db.Pacotes.SingleAsync(x => x.Codigo == "VISITANTE");

    db.Concessoes.Add(new ConcessaoPacote(ator.ContaId.Value, pacoteVisitante.Id, OrigemConcessao.Administracao, DateTimeOffset.UtcNow));

    await db.SaveChangesAsync();

    Assert.Equal("Free", (await acesso.ObterAsync(ator, default)).Contexto);

    Assert.Equal(3, (await acesso.ObterAsync(ator, default)).Limite);

    var recurso = await db.Recursos.SingleAsync(x => x.Codigo == "BUSCA_EXECUTAR");

    recurso.DefinirAtivo(false);

    await db.SaveChangesAsync();

    await Assert.ThrowsAsync<ErroAplicacaoException>(() => acesso.ObterAsync(ator, default));
  }

  private sealed class EnvioFalho : ISendEndpointProvider
  {
    public ConnectHandle ConnectSendObserver(ISendObserver observer) => throw new NotSupportedException();

    public Task<ISendEndpoint> GetSendEndpoint(Uri address) => Task.FromException<ISendEndpoint>(new InvalidOperationException("Falha de envio simulada."));
  }
}
