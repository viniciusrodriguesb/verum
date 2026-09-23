using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Verum.CrossCutting.Pipelines;

public static class PersistenciaPipeline
{
  public static IServiceCollection AddVerumPostgreSql<TContext>(this IServiceCollection services, IConfiguration configuration, string schema) where TContext : DbContext
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(schema);

    var connection = configuration.GetConnectionString("PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings:PostgreSQL.");

    return services.AddDbContext<TContext>(options => options.UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", schema)));
  }

  public static void AddVerumOutbox<TContext>(this IBusRegistrationConfigurator bus) where TContext : DbContext
  {
    bus.AddEntityFrameworkOutbox<TContext>(outbox =>
    {
      outbox.UsePostgres();

      outbox.UseBusOutbox();

      outbox.QueryDelay = TimeSpan.FromSeconds(1);
    });
  }

  public static void UseVerumOutbox<TContext>(this IReceiveEndpointConfigurator endpoint, IRegistrationContext context) where TContext : DbContext =>
    endpoint.UseEntityFrameworkOutbox<TContext>(context);

}
