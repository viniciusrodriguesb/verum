using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Verum.CrossCutting.Cache;

public sealed class CacheRedis(RedisConexao conexao, IOptions<RedisOptions> options, ILogger<CacheRedis> logger, TimeProvider relogio) : IDisposable
{
  private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

  private readonly SemaphoreSlim[] _preenchimentos = Enumerable.Range(0, 256).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

  public async Task<T?> GetAsync<T>(string chave, CancellationToken cancellationToken = default) where T : class
  {
    var key = Chave(chave);

    cancellationToken.ThrowIfCancellationRequested();

    if (!options.Value.Enabled)
      return null;

    try
    {
      var db = (await conexao.ObterAsync().WaitAsync(cancellationToken)).GetDatabase();

      var value = await db.StringGetAsync(key).WaitAsync(cancellationToken);

      return value.IsNull ? null : JsonSerializer.Deserialize<T>((byte[])value!, Json);
    }
    catch (RedisException ex)
    {
      logger.LogWarning(ex, "Redis indisponível na leitura do cache.");

      return null;
    }
    catch (JsonException ex)
    {
      logger.LogWarning(ex, "Entrada de cache incompatível com o contrato solicitado.");

      return null;
    }
  }

  public Task<bool> SetAsync<T>(string chave, T valor, TimeSpan ttl, CancellationToken cancellationToken = default) where T : class
  {
    ValidarTtl(ttl);

    return SetAteAsync(chave, valor, relogio.GetUtcNow().Add(ttl), cancellationToken);
  }

  public async Task<bool> SetAteAsync<T>(string chave, T valor, DateTimeOffset validoAte, CancellationToken cancellationToken = default) where T : class
  {
    var key = Chave(chave);

    ArgumentNullException.ThrowIfNull(valor);

    cancellationToken.ThrowIfCancellationRequested();

    if (!options.Value.Enabled)
      return false;

    if (validoAte <= relogio.GetUtcNow())
    {
      await RemoveAsync(chave, cancellationToken);

      return false;
    }

    var payload = JsonSerializer.SerializeToUtf8Bytes(valor, Json);

    try
    {
      var db = (await conexao.ObterAsync().WaitAsync(cancellationToken)).GetDatabase();

      var restante = validoAte - relogio.GetUtcNow();

      if (restante < TimeSpan.FromMilliseconds(1)) {
        await RemoveAsync(chave, cancellationToken);

        return false;
      }

      var expiraEm = validoAte - TimeSpan.FromTicks((long)(restante.Ticks * Random.Shared.NextDouble() * 0.1));

      var result = await db.ExecuteAsync("SET", key, payload, "PXAT", expiraEm.ToUnixTimeMilliseconds()).WaitAsync(cancellationToken);

      return !result.IsNull;
    }
    catch (RedisException ex)
    {
      logger.LogWarning(ex, "Redis indisponível na gravação do cache.");

      return false;
    }
  }

  public async Task<bool> RemoveAsync(string chave, CancellationToken cancellationToken = default)
  {
    var key = Chave(chave);

    cancellationToken.ThrowIfCancellationRequested();

    if (!options.Value.Enabled)
      return false;

    try
    {
      var db = (await conexao.ObterAsync().WaitAsync(cancellationToken)).GetDatabase();

      return await db.KeyDeleteAsync(key).WaitAsync(cancellationToken);
    }
    catch (RedisException ex)
    {
      logger.LogWarning(ex, "Redis indisponível na remoção do cache.");

      return false;
    }
  }

  public async Task<bool> ExistsAsync(string chave, CancellationToken cancellationToken = default)
  {
    var key = Chave(chave);

    cancellationToken.ThrowIfCancellationRequested();

    if (!options.Value.Enabled)
      return false;

    try
    {
      var db = (await conexao.ObterAsync().WaitAsync(cancellationToken)).GetDatabase();

      return await db.KeyExistsAsync(key).WaitAsync(cancellationToken);
    }
    catch (RedisException ex)
    {
      logger.LogWarning(ex, "Redis indisponível na consulta de existência.");

      return false;
    }
  }

  public async Task<T?> GetOrCreateAsync<T>(string chave, Func<CancellationToken, Task<T?>> carregar,
    TimeSpan ttl, CancellationToken cancellationToken = default) where T : class
  {
    ChavesCache.Validar(chave);

    ArgumentNullException.ThrowIfNull(carregar);

    ValidarTtl(ttl);

    var valor = await GetAsync<T>(chave, cancellationToken);

    if (valor is not null)
      return valor;

    var gate = _preenchimentos[(uint)StringComparer.Ordinal.GetHashCode(chave) % (uint)_preenchimentos.Length];

    await gate.WaitAsync(cancellationToken);

    try
    {
      valor = await GetAsync<T>(chave, cancellationToken);

      if (valor is not null)
        return valor;

      var validoAte = relogio.GetUtcNow().Add(ttl);

      valor = await carregar(cancellationToken);

      if (valor is not null) await SetAteAsync(chave, valor, validoAte, cancellationToken);

      return valor;
    }
    finally {
      gate.Release();
    }
  }

  private RedisKey Chave(string chave) => $"{options.Value.Prefixo}v1:cache:{ChavesCache.Validar(chave)}";

  internal static void ValidarTtl(TimeSpan ttl)
  {
    if (ttl < TimeSpan.FromMilliseconds(1) || ttl > TimeSpan.FromDays(30))
      throw new ArgumentOutOfRangeException(nameof(ttl), "A validade deve estar entre 1 ms e 30 dias.");
  }

  public void Dispose()
  {
    foreach (var gate in _preenchimentos) gate.Dispose();
  }
}
