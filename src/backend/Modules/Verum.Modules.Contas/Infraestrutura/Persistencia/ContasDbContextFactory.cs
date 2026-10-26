using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verum.Modules.Contas.Infraestrutura.Persistencia;

internal sealed class ContasDbContextFactory : IDesignTimeDbContextFactory<ContasDbContext>
{
  public ContasDbContext CreateDbContext(string[] args)
  {
    var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings__PostgreSQL para executar migrations.");

    var options = new DbContextOptionsBuilder<ContasDbContext>()
      .UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "contas"))
      .Options;

    return new ContasDbContext(options);
  }
}
