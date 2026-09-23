namespace Verum.CrossCutting.Cache;

internal static class ValidadeCache
{
  internal static DateTimeOffset Limitar(DateTimeOffset validoAte, TimeSpan ttl, TimeProvider relogio)
  {
    var limite = relogio.GetUtcNow().Add(ttl);

    return validoAte < limite ? validoAte : limite;
  }
}
