using Microsoft.Extensions.Options;
using Verum.Modules.Acesso.Contratos.Cache;

namespace Verum.CrossCutting.Cache.Modulos;

internal sealed class AcessoCache(CacheRedis cache, CoordenacaoRedis coordenacao, IOptions<RedisOptions> options, TimeProvider relogio) : IAcessoCache
{
  public Task<T?> ObterPermissoesAsync<T>(Guid contaId, CancellationToken cancellationToken = default) where T : class =>
    cache.GetAsync<T>(Permissoes(contaId), cancellationToken);

  public Task<bool> ArmazenarPermissoesAsync<T>(Guid contaId, T permissoes, DateTimeOffset validasAte, CancellationToken cancellationToken = default) where T : class =>
    cache.SetAteAsync(Permissoes(contaId), permissoes, ValidadeCache.Limitar(validasAte, options.Value.AcessoTtl, relogio), cancellationToken);

  public Task<bool> InvalidarPermissoesAsync(Guid contaId, CancellationToken cancellationToken = default) =>
    cache.RemoveAsync(Permissoes(contaId), cancellationToken);

  public Task<ResultadoLimite> TentarConsumirLimiteContaAsync(Guid contaId, string recurso, int limite, DateTimeOffset fimJanela, CancellationToken cancellationToken = default) =>
    ConsumirAsync($"conta:{ChavesCache.Id(contaId)}", recurso, limite, fimJanela, cancellationToken);

  public Task<ResultadoLimite> TentarConsumirLimiteVisitanteAsync(Guid visitanteId, string recurso, int limite, DateTimeOffset fimJanela, CancellationToken cancellationToken = default) =>
    ConsumirAsync($"visitante:{ChavesCache.Id(visitanteId)}", recurso, limite, fimJanela, cancellationToken);

  private async Task<ResultadoLimite> ConsumirAsync(string ator, string recurso, int limite, DateTimeOffset fimJanela, CancellationToken cancellationToken)
  {
    var resultado = await coordenacao.TentarConsumirAsync($"acesso:{ator}:{ChavesCache.Hash(recurso)}", limite, fimJanela, cancellationToken);

    return new ResultadoLimite(resultado.Permitido, resultado.Utilizado, Math.Max(0, limite - resultado.Utilizado), resultado.TentarNovamenteEm);
  }

  private static string Permissoes(Guid id) => $"acesso:permissoes:{ChavesCache.Id(id)}";
}
