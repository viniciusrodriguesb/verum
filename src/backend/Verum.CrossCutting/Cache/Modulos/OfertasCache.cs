using Microsoft.Extensions.Options;
using Verum.Modules.Ofertas.Contratos.Cache;

namespace Verum.CrossCutting.Cache.Modulos;

internal sealed class OfertasCache(CacheRedis cache, CoordenacaoRedis coordenacao, IOptions<RedisOptions> options, TimeProvider relogio) : IOfertasCache
{
  public Task<T?> ObterAtuaisAsync<T>(Guid varianteId, CancellationToken cancellationToken = default) where T : class =>
    cache.GetAsync<T>(Ofertas(varianteId), cancellationToken);

  public Task<bool> ArmazenarAtuaisAsync<T>(Guid varianteId, T ofertas, DateTimeOffset validasAte, CancellationToken cancellationToken = default) where T : class =>
    cache.SetAteAsync(Ofertas(varianteId), ofertas, ValidadeCache.Limitar(validasAte, options.Value.OfertasTtl, relogio), cancellationToken);

  public Task<bool> InvalidarAsync(Guid varianteId, CancellationToken cancellationToken = default) =>
    cache.RemoveAsync(Ofertas(varianteId), cancellationToken);

  public Task<string?> TentarReservarAtualizacaoAsync(Guid varianteId, Guid fonteId, TimeSpan duracao, CancellationToken cancellationToken = default) =>
    coordenacao.TentarAdquirirAsync(Atualizacao(varianteId, fonteId), duracao, cancellationToken);

  public Task<bool> RenovarReservaAsync(Guid varianteId, Guid fonteId, string token, TimeSpan duracao, CancellationToken cancellationToken = default) =>
    coordenacao.RenovarAsync(Atualizacao(varianteId, fonteId), token, duracao, cancellationToken);

  public Task<bool> LiberarReservaAsync(Guid varianteId, Guid fonteId, string token, CancellationToken cancellationToken = default) =>
    coordenacao.LiberarAsync(Atualizacao(varianteId, fonteId), token, cancellationToken);

  private static string Ofertas(Guid id) => $"ofertas:atuais:{ChavesCache.Id(id)}";

  private static string Atualizacao(Guid variante, Guid fonte) => $"ofertas:atualizacao:{ChavesCache.Id(variante)}:{ChavesCache.Id(fonte)}";
}
