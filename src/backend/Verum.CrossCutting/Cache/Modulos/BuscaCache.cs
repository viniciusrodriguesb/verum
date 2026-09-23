using Microsoft.Extensions.Options;
using Verum.Modules.Busca.Contratos.Cache;

namespace Verum.CrossCutting.Cache.Modulos;

internal sealed class BuscaCache(CacheRedis cache, IOptions<RedisOptions> options, TimeProvider relogio) : IBuscaCache
{
  public Task<T?> ObterStatusAsync<T>(Guid buscaId, CancellationToken cancellationToken = default) where T : class =>
    cache.GetAsync<T>(Status(buscaId), cancellationToken);

  public Task<bool> ArmazenarStatusAsync<T>(Guid buscaId, T status, CancellationToken cancellationToken = default) where T : class =>
    cache.SetAsync(Status(buscaId), status, options.Value.StatusBuscaTtl, cancellationToken);

  public Task<T?> ObterResultadoAsync<T>(Guid buscaId, CancellationToken cancellationToken = default) where T : class =>
    cache.GetAsync<T>(Resultado(buscaId), cancellationToken);

  public Task<bool> ArmazenarResultadoAsync<T>(Guid buscaId, T resultado, DateTimeOffset validoAte, CancellationToken cancellationToken = default) where T : class =>
    cache.SetAteAsync(Resultado(buscaId), resultado, ValidadeCache.Limitar(validoAte, options.Value.ResultadoBuscaTtl, relogio), cancellationToken);

  public Task InvalidarAsync(Guid buscaId, CancellationToken cancellationToken = default) =>
    Task.WhenAll(cache.RemoveAsync(Status(buscaId), cancellationToken), cache.RemoveAsync(Resultado(buscaId), cancellationToken));

  private static string Status(Guid id) => $"busca:status:{ChavesCache.Id(id)}";

  private static string Resultado(Guid id) => $"busca:resultado:{ChavesCache.Id(id)}";
}
