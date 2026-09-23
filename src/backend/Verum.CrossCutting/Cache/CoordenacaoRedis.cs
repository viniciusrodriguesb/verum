using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Verum.CrossCutting.Cache;

public sealed class CoordenacaoRedis(RedisConexao conexao, IOptions<RedisOptions> options, TimeProvider relogio)
{
  private const string LiberarScript = """
    if redis.call('GET', KEYS[1]) == ARGV[1] then
      return redis.call('DEL', KEYS[1])
    end
    return 0
    """;

  private const string RenovarScript = """
    if redis.call('GET', KEYS[1]) == ARGV[1] then
      return redis.call('PEXPIRE', KEYS[1], ARGV[2])
    end
    return 0
    """;

  private const string LimiteScript = """
    local agora = redis.call('TIME')
    local agoraMs = tonumber(agora[1]) * 1000 + math.floor(tonumber(agora[2]) / 1000)
    if tonumber(ARGV[2]) <= agoraMs then return {0, 0, 0} end
    local usado = tonumber(redis.call('GET', KEYS[1]) or '0')
    if usado >= tonumber(ARGV[1]) then
      return {0, usado, math.max(0, redis.call('PTTL', KEYS[1]))}
    end
    usado = redis.call('INCR', KEYS[1])
    redis.call('PEXPIREAT', KEYS[1], ARGV[2])
    return {1, usado, math.max(0, redis.call('PTTL', KEYS[1]))}
    """;

  public async Task<bool> TryAddAsync(string chave, string valor, TimeSpan ttl, CancellationToken cancellationToken = default)
  {
    var key = Chave("reserva", chave);

    ArgumentException.ThrowIfNullOrWhiteSpace(valor);

    CacheRedis.ValidarTtl(ttl);

    var db = await BancoAsync(cancellationToken);

    return await db.StringSetAsync(key, valor, ttl, When.NotExists).WaitAsync(cancellationToken);
  }

  public async Task<string?> TentarAdquirirAsync(string chave, TimeSpan duracao, CancellationToken cancellationToken = default)
  {
    var token = Guid.NewGuid().ToString("N");

    return await TryAddAsync(chave, token, duracao, cancellationToken) ? token : null;
  }

  public async Task<bool> RenovarAsync(string chave, string token, TimeSpan duracao, CancellationToken cancellationToken = default)
  {
    var key = Chave("reserva", chave);

    ArgumentException.ThrowIfNullOrWhiteSpace(token);

    CacheRedis.ValidarTtl(duracao);

    var db = await BancoAsync(cancellationToken);

    var result = await db.ScriptEvaluateAsync(RenovarScript, [key], [token, (long)duracao.TotalMilliseconds]).WaitAsync(cancellationToken);

    return (long)result == 1;
  }

  public async Task<bool> LiberarAsync(string chave, string token, CancellationToken cancellationToken = default)
  {
    var key = Chave("reserva", chave);

    ArgumentException.ThrowIfNullOrWhiteSpace(token);

    var db = await BancoAsync(cancellationToken);

    var result = await db.ScriptEvaluateAsync(LiberarScript, [key], [token]).WaitAsync(cancellationToken);

    return (long)result == 1;
  }

  public async Task<(bool Permitido, long Utilizado, TimeSpan TentarNovamenteEm)> TentarConsumirAsync(
    string chave, int limite, DateTimeOffset fimJanela, CancellationToken cancellationToken = default)
  {
    ChavesCache.Validar(chave);

    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limite);

    var restante = fimJanela - relogio.GetUtcNow();

    if (restante <= TimeSpan.Zero || restante > TimeSpan.FromDays(366))
      throw new ArgumentOutOfRangeException(nameof(fimJanela), "Informe o fim da janela, no futuro e em até 366 dias.");

    var key = Chave("limite", $"{chave}:{fimJanela.ToUnixTimeMilliseconds()}");

    var db = await BancoAsync(cancellationToken);

    var result = (RedisResult[])(await db.ScriptEvaluateAsync(LimiteScript, [key], [limite, fimJanela.ToUnixTimeMilliseconds()]).WaitAsync(cancellationToken))!;

    return ((long)result[0] == 1, (long)result[1], TimeSpan.FromMilliseconds((long)result[2]));
  }

  private RedisKey Chave(string tipo, string chave) => $"{options.Value.Prefixo}v1:coord:{tipo}:{ChavesCache.Validar(chave)}";

  private async Task<IDatabase> BancoAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    return (await conexao.ObterAsync().WaitAsync(cancellationToken)).GetDatabase();
  }
}
