using Microsoft.Extensions.Options;
using Verum.Modules.Catalogo.Contratos.Cache;

namespace Verum.CrossCutting.Cache.Modulos;

internal sealed class CatalogoCache(CacheRedis cache, IOptions<RedisOptions> options) : ICatalogoCache
{
  public Task<T?> ObterVarianteAsync<T>(Guid varianteId, CancellationToken cancellationToken = default) where T : class =>
    cache.GetAsync<T>(Variante(varianteId), cancellationToken);

  public Task<bool> ArmazenarVarianteAsync<T>(Guid varianteId, T variante, CancellationToken cancellationToken = default) where T : class =>
    cache.SetAsync(Variante(varianteId), variante, options.Value.CatalogoTtl, cancellationToken);

  public Task<bool> InvalidarVarianteAsync(Guid varianteId, CancellationToken cancellationToken = default) =>
    cache.RemoveAsync(Variante(varianteId), cancellationToken);

  public Task<T?> ObterResolucaoAsync<T>(string consultaNormalizada, string versaoInterpretador, CancellationToken cancellationToken = default) where T : class =>
    cache.GetAsync<T>(Resolucao(consultaNormalizada, versaoInterpretador), cancellationToken);

  public Task<bool> ArmazenarResolucaoAsync<T>(string consultaNormalizada, string versaoInterpretador, T resolucao, CancellationToken cancellationToken = default) where T : class =>
    cache.SetAsync(Resolucao(consultaNormalizada, versaoInterpretador), resolucao, options.Value.ResolucaoConsultaTtl, cancellationToken);

  public Task<bool> InvalidarResolucaoAsync(string consultaNormalizada, string versaoInterpretador, CancellationToken cancellationToken = default) =>
    cache.RemoveAsync(Resolucao(consultaNormalizada, versaoInterpretador), cancellationToken);

  private static string Variante(Guid id) => $"catalogo:variante:{ChavesCache.Id(id)}";

  // A normalização semântica pertence ao Catálogo; URLs não devem ser convertidas arbitrariamente para minúsculas.
  private static string Resolucao(string consulta, string versao) =>
    $"catalogo:resolucao:{ChavesCache.Hash(versao)}:{ChavesCache.Hash(consulta)}";
}
