using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class CachePipeline
{
  public static IServiceCollection AddVerumCache(this IServiceCollection services, IConfiguration configuration)
  {
    if (!configuration.GetValue<bool>("Redis:Enabled")) return services;

    var connection = configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings:Redis quando Redis:Enabled for true.");

    services.AddStackExchangeRedisCache(options =>
    {
      options.Configuration = connection;
      options.InstanceName = "verum:";
    });

    return services;
  }
}

