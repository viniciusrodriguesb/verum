using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Verum.CrossCutting.Cache;
using Verum.CrossCutting.Pipelines;
using Verum.Modules.Acesso.Contratos.Cache;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Catalogo.Contratos.Cache;
using Verum.Modules.Ofertas.Contratos.Cache;
using Xunit;

namespace Verum.Tests.Integracao;

public sealed class CacheRedisTests
{
  [Fact]
  public async Task CacheDesabilitadoRecorreAOrigemMasNaoConcedeLimitesOuReservas()
  {
    using var provider = CriarProvider(new() { ["Redis:Enabled"] = "false" });

    var cache = provider.GetRequiredService<CacheRedis>();

    Assert.Null(await cache.GetAsync<string>("ausente"));

    Assert.False(await cache.SetAsync("teste", "valor", TimeSpan.FromMinutes(1)));

    Assert.False(await cache.ExistsAsync("teste"));

    Assert.False(await cache.RemoveAsync("teste"));

    Assert.Equal("origem", await cache.GetOrCreateAsync("teste", _ => Task.FromResult<string?>("origem"), TimeSpan.FromMinutes(1)));

    var acesso = provider.GetRequiredService<IAcessoCache>();

    Assert.Null(await acesso.ObterPermissoesAsync<string>(Guid.NewGuid()));

    await Assert.ThrowsAsync<InvalidOperationException>(() => acesso.TentarConsumirLimiteVisitanteAsync(
      Guid.NewGuid(), "busca", 1, DateTimeOffset.UtcNow.AddDays(1)));

    await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetRequiredService<IOfertasCache>()
      .TentarReservarAtualizacaoAsync(Guid.NewGuid(), Guid.NewGuid(), TimeSpan.FromMinutes(1)));
  }

  [Fact]
  public async Task CancelamentoValidacaoEFalhaDaOrigemNaoSaoOcultados()
  {
    using var provider = CriarProvider(new());

    var cache = provider.GetRequiredService<CacheRedis>();

    using var cancelamento = new CancellationTokenSource();

    cancelamento.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cache.GetAsync<string>("teste", cancelamento.Token));

    await Assert.ThrowsAsync<ArgumentException>(() => cache.GetAsync<string>(" "));

    await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cache.SetAsync("teste", "valor", TimeSpan.Zero));

    await Assert.ThrowsAsync<InvalidOperationException>(() => cache.GetOrCreateAsync<string>("teste",
      _ => throw new InvalidOperationException("Falha na origem"), TimeSpan.FromMinutes(1)));

    Assert.Equal("recuperado", await cache.GetOrCreateAsync("teste", _ => Task.FromResult<string?>("recuperado"), TimeSpan.FromMinutes(1)));
  }

  [Theory]
  [InlineData("Redis:Prefixo", "")]
  [InlineData("Redis:Prefixo", "{todos}")]
  [InlineData("Redis:TimeoutMilissegundos", "0")]
  [InlineData("Redis:OfertasTtl", "00:00:00")]
  public void ConfiguracoesInvalidasSaoRejeitadas(string chave, string valor)
  {
    using var provider = CriarProvider(new() { [chave] = valor });

    Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RedisOptions>>().Value);
  }

  [Fact]
  public void ContratosResolvemSemConexaoECompartilhamInstancias()
  {
    using var provider = CriarProvider(new() { ["Redis:Enabled"] = "true", ["ConnectionStrings:Redis"] = "localhost:6379" });

    using var scope = provider.CreateScope();

    Assert.Same(provider.GetRequiredService<CacheRedis>(), scope.ServiceProvider.GetRequiredService<CacheRedis>());

    Assert.NotNull(provider.GetRequiredService<IBuscaCache>());

    Assert.NotNull(provider.GetRequiredService<ICatalogoCache>());

    Assert.NotNull(provider.GetRequiredService<IOfertasCache>());

    Assert.NotNull(provider.GetRequiredService<IAcessoCache>());
  }

  internal static ServiceProvider CriarProvider(Dictionary<string, string?> values)
  {
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    return new ServiceCollection().AddVerumCache(configuration)
      .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
  }
}
