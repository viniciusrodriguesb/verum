using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia;

internal sealed class BuscaDbContextFactory : IDesignTimeDbContextFactory<BuscaDbContext>
{
  public BuscaDbContext CreateDbContext(string[] args)
  {
    var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings__PostgreSQL para executar migrations.");

    var options = new DbContextOptionsBuilder<BuscaDbContext>()
      .UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "busca"))
      .Options;

    return new BuscaDbContext(options);
  }
}
