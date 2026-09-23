using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Verum.CrossCutting.Cache;
using Verum.CrossCutting.Cache.Modulos;
using Verum.Modules.Acesso.Contratos.Cache;
using Verum.Modules.Busca.Contratos.Cache;
using Verum.Modules.Catalogo.Contratos.Cache;
using Verum.Modules.Ofertas.Contratos.Cache;

namespace Verum.CrossCutting.Pipelines;

public static class CachePipeline
{
  public static IServiceCollection AddVerumCache(this IServiceCollection services, IConfiguration configuration)
  {
    if (configuration.GetValue<bool>("Redis:Enabled") && string.IsNullOrWhiteSpace(configuration.GetConnectionString("Redis")))
      throw new InvalidOperationException("Configure ConnectionStrings:Redis quando Redis:Enabled for true.");

    services.AddOptions<Cache.RedisOptions>().Bind(configuration.GetSection("Redis"))
      .Validate(options => options.Valido(), "Configuração Redis inválida: verifique prefixo, timeout e TTLs.")
      .ValidateOnStart();

    services.AddLogging();

    services.TryAddSingleton<TimeProvider>(TimeProvider.System);

    services.TryAddSingleton(provider => new RedisConexao(configuration, provider.GetRequiredService<IOptions<Cache.RedisOptions>>()));

    services.TryAddSingleton<CacheRedis>();

    services.TryAddSingleton<CoordenacaoRedis>();

    services.TryAddSingleton<IBuscaCache, BuscaCache>();

    services.TryAddSingleton<ICatalogoCache, CatalogoCache>();

    services.TryAddSingleton<IOfertasCache, OfertasCache>();

    services.TryAddSingleton<IAcessoCache, AcessoCache>();

    if (configuration.GetValue<bool>("Redis:Enabled"))
    {
      services.AddStackExchangeRedisCache(_ => { });

      services.AddOptions<RedisCacheOptions>().Configure<RedisConexao, IOptions<Cache.RedisOptions>>((options, conexao, settings) =>
      {
        options.ConnectionMultiplexerFactory = conexao.ObterAsync;

        options.InstanceName = $"{settings.Value.Prefixo}v1:distributed:";
      });
    }

    return services;
  }
}
