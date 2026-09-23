namespace Verum.CrossCutting.Cache;

public sealed class RedisOptions
{
  public bool Enabled { get; set; }

  public string Prefixo { get; set; } = "verum:";

  public int TimeoutMilissegundos { get; set; } = 1000;

  public TimeSpan StatusBuscaTtl { get; set; } = TimeSpan.FromSeconds(5);

  public TimeSpan ResultadoBuscaTtl { get; set; } = TimeSpan.FromMinutes(15);

  public TimeSpan CatalogoTtl { get; set; } = TimeSpan.FromHours(6);

  public TimeSpan ResolucaoConsultaTtl { get; set; } = TimeSpan.FromHours(1);

  public TimeSpan OfertasTtl { get; set; } = TimeSpan.FromMinutes(2);

  public TimeSpan AcessoTtl { get; set; } = TimeSpan.FromSeconds(30);

  internal bool Valido() => !string.IsNullOrWhiteSpace(Prefixo) && Prefixo.Length <= 100
    && !Prefixo.Contains('{') && !Prefixo.Contains('}')
    && TimeoutMilissegundos is >= 100 and <= 30000
    && new[] { StatusBuscaTtl, ResultadoBuscaTtl, CatalogoTtl, ResolucaoConsultaTtl, OfertasTtl, AcessoTtl }
      .All(ttl => ttl >= TimeSpan.FromMilliseconds(1) && ttl <= TimeSpan.FromDays(30));
}
