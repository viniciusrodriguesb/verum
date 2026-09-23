using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Verum.CrossCutting.Cache;
using Verum.Modules.Acesso.Contratos.Cache;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Catalogo.Contratos.Cache;
using Verum.Modules.Ofertas.Contratos.Cache;
using Xunit;

namespace Verum.Tests.Integracao;

// Execute com VERUM_TEST_REDIS=localhost:6379. Se configurado e indisponível, os testes falham.
public sealed class RedisFactAttribute : FactAttribute
{
  public RedisFactAttribute()
  {
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VERUM_TEST_REDIS")))
      Skip = "Configure VERUM_TEST_REDIS para executar contra Redis 6.2+ real.";
  }
}

public sealed class RedisRealTests
{
  [RedisFact]
  public async Task CrudJsonExpiracaoECompatibilidadeComIDistributedCache()
  {
    await using var cenario = await Cenario.CriarAsync();

    var cache = cenario.Primeiro.GetRequiredService<CacheRedis>();

    var outro = cenario.Segundo.GetRequiredService<CacheRedis>();

    var dto = new Amostra("ação", 12.34m);

    Assert.Null(await cache.GetAsync<Amostra>("dto"));

    Assert.True(await cache.SetAsync("dto", dto, TimeSpan.FromSeconds(10)));

    Assert.Equal(dto, await outro.GetAsync<Amostra>("dto"));

    Assert.True(await cache.ExistsAsync("dto"));

    Assert.True(await outro.SetAsync("dto", dto with { Valor = 42 }, TimeSpan.FromSeconds(10)));

    Assert.Equal(42, (await cache.GetAsync<Amostra>("dto"))!.Valor);

    var distributed = cenario.Primeiro.GetRequiredService<IDistributedCache>();

    await distributed.SetStringAsync("dto", "outra representação", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });

    Assert.Equal("outra representação", await distributed.GetStringAsync("dto"));

    Assert.Equal(42, (await cache.GetAsync<Amostra>("dto"))!.Valor);

    Assert.True(await cache.RemoveAsync("dto"));

    Assert.Null(await outro.GetAsync<Amostra>("dto"));

    Assert.True(await cache.SetAsync("expira", dto, TimeSpan.FromMilliseconds(200)));

    await Task.Delay(300);

    Assert.Null(await outro.GetAsync<Amostra>("expira"));

    await cenario.Banco.StringSetAsync(cenario.ChaveCache("corrompido"), "{json-invalido", TimeSpan.FromSeconds(10));

    Assert.Null(await cache.GetAsync<Amostra>("corrompido"));
  }

  [RedisFact]
  public async Task CacheAsideAgrupaConcorrenciaLocalENaoArmazenaNull()
  {
    await using var cenario = await Cenario.CriarAsync();

    var cache = cenario.Primeiro.GetRequiredService<CacheRedis>();

    var chamadas = 0;

    var resultados = await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => cache.GetOrCreateAsync("concorrente", async ct =>
    {
      Interlocked.Increment(ref chamadas);

      await Task.Delay(50, ct);

      return new Amostra("origem", 1);
    }, TimeSpan.FromSeconds(10))));

    Assert.Equal(1, chamadas);

    Assert.All(resultados, valor => Assert.Equal("origem", valor!.Nome));

    Assert.Null(await cache.GetOrCreateAsync<Amostra>("nulo", _ => Task.FromResult<Amostra?>(null), TimeSpan.FromSeconds(10)));

    Assert.False(await cache.ExistsAsync("nulo"));

    using var cancelamento = new CancellationTokenSource();

    var iniciou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    var primeira = cache.GetOrCreateAsync<Amostra>("cancelada", async ct =>
    {
      iniciou.SetResult();

      await Task.Delay(Timeout.InfiniteTimeSpan, ct);

      return null;
    }, TimeSpan.FromSeconds(10), cancelamento.Token);

    await iniciou.Task;

    cancelamento.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => primeira);

    Assert.NotNull(await cache.GetOrCreateAsync("cancelada", _ => Task.FromResult<Amostra?>(new("ok", 1)), TimeSpan.FromSeconds(10)));
  }

  [RedisFact]
  public async Task ReservaTemUmDonoEAntigoDonoNaoPodeApagarOuRenovarNovaReserva()
  {
    await using var cenario = await Cenario.CriarAsync();

    var primeiro = cenario.Primeiro.GetRequiredService<IOfertasCache>();

    var segundo = cenario.Segundo.GetRequiredService<IOfertasCache>();

    var variante = Guid.NewGuid();

    var fonte = Guid.NewGuid();

    var tentativas = await Task.WhenAll(Enumerable.Range(0, 30).Select(i => (i % 2 == 0 ? primeiro : segundo)
      .TentarReservarAtualizacaoAsync(variante, fonte, TimeSpan.FromSeconds(10))));

    var token = Assert.Single(tentativas, t => t is not null)!;

    Assert.False(await segundo.LiberarReservaAsync(variante, fonte, "outro-dono"));

    Assert.False(await segundo.RenovarReservaAsync(variante, fonte, "outro-dono", TimeSpan.FromSeconds(10)));

    Assert.True(await primeiro.RenovarReservaAsync(variante, fonte, token, TimeSpan.FromMilliseconds(200)));

    await Task.Delay(300);

    var novoToken = await segundo.TentarReservarAtualizacaoAsync(variante, fonte, TimeSpan.FromSeconds(10));

    Assert.NotNull(novoToken);

    Assert.False(await primeiro.LiberarReservaAsync(variante, fonte, token));

    Assert.False(await primeiro.RenovarReservaAsync(variante, fonte, token, TimeSpan.FromSeconds(10)));

    Assert.True(await segundo.LiberarReservaAsync(variante, fonte, novoToken!));
  }

  [RedisFact]
  public async Task LimiteAtomicoEntreInstanciasSeparaContaVisitanteRecursoEJanela()
  {
    await using var cenario = await Cenario.CriarAsync();

    var primeiro = cenario.Primeiro.GetRequiredService<IAcessoCache>();

    var segundo = cenario.Segundo.GetRequiredService<IAcessoCache>();

    var ator = Guid.NewGuid();

    var fim = DateTimeOffset.UtcNow.AddSeconds(20);

    var resultados = await Task.WhenAll(Enumerable.Range(0, 100).Select(i => (i % 2 == 0 ? primeiro : segundo)
      .TentarConsumirLimiteVisitanteAsync(ator, "busca", 7, fim)));

    Assert.Equal(7, resultados.Count(r => r.Permitido));

    Assert.All(resultados, r => Assert.InRange(r.Utilizado, 1, 7));

    var negado = await primeiro.TentarConsumirLimiteVisitanteAsync(ator, "busca", 7, fim);

    Assert.False(negado.Permitido);

    Assert.Equal(7, negado.Utilizado);

    Assert.Equal(0, negado.Restante);

    Assert.InRange(negado.TentarNovamenteEm, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(20));

    Assert.True((await primeiro.TentarConsumirLimiteContaAsync(ator, "busca", 7, fim)).Permitido);

    Assert.True((await primeiro.TentarConsumirLimiteVisitanteAsync(ator, "radar", 7, fim)).Permitido);

    Assert.True((await primeiro.TentarConsumirLimiteVisitanteAsync(ator, "busca", 7, fim.AddDays(1))).Permitido);
  }

  [RedisFact]
  public async Task CacheDosModulosRespeitaValidadeIsolamentoEInvalidacao()
  {
    await using var cenario = await Cenario.CriarAsync();

    var ofertas = cenario.Primeiro.GetRequiredService<IOfertasCache>();

    var busca = cenario.Primeiro.GetRequiredService<IBuscaCache>();

    var catalogo = cenario.Primeiro.GetRequiredService<ICatalogoCache>();

    var acesso = cenario.Primeiro.GetRequiredService<IAcessoCache>();

    var id = Guid.NewGuid();

    var dto = new Amostra("dto", 1);

    var fim = DateTimeOffset.UtcNow.AddSeconds(2);

    Assert.True(await ofertas.ArmazenarAtuaisAsync(id, dto, fim));

    var ttl = await cenario.Banco.KeyTimeToLiveAsync(cenario.ChaveCache($"ofertas:atuais:{id:N}"));

    Assert.NotNull(ttl);

    Assert.InRange(ttl.Value, TimeSpan.FromMilliseconds(1), fim - DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(50));

    Assert.True(await busca.ArmazenarStatusAsync(id, dto));

    Assert.True(await busca.ArmazenarResultadoAsync(id, dto, fim));

    Assert.True(await catalogo.ArmazenarVarianteAsync(id, dto));

    Assert.True(await catalogo.ArmazenarResolucaoAsync("iPhone 16", "v1", dto));

    Assert.NotNull(await catalogo.ObterResolucaoAsync<Amostra>("iPhone 16", "v1"));

    Assert.Null(await catalogo.ObterResolucaoAsync<Amostra>("iPhone 16", "v2"));

    Assert.True(await acesso.ArmazenarPermissoesAsync(id, dto, fim));

    Assert.Null(await acesso.ObterPermissoesAsync<Amostra>(Guid.NewGuid()));

    Assert.NotNull(await acesso.ObterPermissoesAsync<Amostra>(id));

    Assert.True(await acesso.InvalidarPermissoesAsync(id));

    Assert.Null(await acesso.ObterPermissoesAsync<Amostra>(id));

    await busca.InvalidarAsync(id);

    Assert.Null(await busca.ObterStatusAsync<Amostra>(id));

    Assert.Null(await busca.ObterResultadoAsync<Amostra>(id));

    Assert.NotNull(await catalogo.ObterVarianteAsync<Amostra>(id));

    Assert.True(await catalogo.InvalidarVarianteAsync(id));

    Assert.True(await catalogo.InvalidarResolucaoAsync("iPhone 16", "v1"));

    Assert.False(await ofertas.ArmazenarAtuaisAsync(id, dto, DateTimeOffset.UtcNow.AddSeconds(-1)));

    Assert.Null(await ofertas.ObterAtuaisAsync<Amostra>(id));
  }

  [RedisFact]
  public async Task IndisponibilidadeViraMissMasNaoAutorizaOperacaoCoordenada()
  {
    // Porta sem serviço: não interrompe nem reconfigura o Redis do desenvolvedor.
    using var provider = CacheRedisTests.CriarProvider(new()
    {
      ["Redis:Enabled"] = "true", ["ConnectionStrings:Redis"] = "127.0.0.1:1,abortConnect=false",
      ["Redis:TimeoutMilissegundos"] = "100"
    });

    var cache = provider.GetRequiredService<CacheRedis>();

    Assert.Null(await cache.GetAsync<Amostra>("ausente"));

    Assert.False(await cache.SetAsync("teste", new Amostra("dto", 1), TimeSpan.FromSeconds(10)));

    Assert.Equal("banco", (await cache.GetOrCreateAsync("origem", _ => Task.FromResult<Amostra?>(new("banco", 1)), TimeSpan.FromSeconds(10)))!.Nome);

    await Assert.ThrowsAnyAsync<RedisException>(() => provider.GetRequiredService<IAcessoCache>()
      .TentarConsumirLimiteContaAsync(Guid.NewGuid(), "busca", 1, DateTimeOffset.UtcNow.AddMinutes(1)));
  }

  public sealed record Amostra(string Nome, decimal Valor);

  private sealed class Cenario : IAsyncDisposable
  {
    private readonly string _prefixo = $"verum:tests:{Guid.NewGuid():N}:";

    public ServiceProvider Primeiro { get; private set; } = null!;

    public ServiceProvider Segundo { get; private set; } = null!;

    public IDatabase Banco { get; private set; } = null!;

    public static async Task<Cenario> CriarAsync()
    {
      var cenario = new Cenario();

      var values = new Dictionary<string, string?>
      {
        ["Redis:Enabled"] = "true", ["Redis:Prefixo"] = cenario._prefixo,
        ["ConnectionStrings:Redis"] = Environment.GetEnvironmentVariable("VERUM_TEST_REDIS")
      };

      cenario.Primeiro = CacheRedisTests.CriarProvider(values);

      cenario.Segundo = CacheRedisTests.CriarProvider(values);

      try
      {
        cenario.Banco = (await cenario.Primeiro.GetRequiredService<RedisConexao>().ObterAsync()).GetDatabase();

        await cenario.Banco.PingAsync();

        await (await cenario.Segundo.GetRequiredService<RedisConexao>().ObterAsync()).GetDatabase().PingAsync();

        return cenario;
      }
      catch
      {
        await cenario.Segundo.DisposeAsync();

        await cenario.Primeiro.DisposeAsync();

        throw;
      }
    }

    public RedisKey ChaveCache(string chave) => $"{_prefixo}v1:cache:{chave}";

    public async ValueTask DisposeAsync()
    {
      try
      {
        var conexao = await Primeiro.GetRequiredService<RedisConexao>().ObterAsync();

        foreach (var endpoint in conexao.GetEndPoints())
        {
          var server = conexao.GetServer(endpoint);

          if (server.IsReplica) continue;

          // SCAN apenas do prefixo aleatório deste cenário; nunca FLUSHDB/KEYS globais.
          await foreach (var key in server.KeysAsync(Banco.Database, $"{_prefixo}*"))
            await Banco.KeyDeleteAsync(key);
        }
      }
      finally
      {
        await Segundo.DisposeAsync();

        await Primeiro.DisposeAsync();
      }
    }
  }
}
