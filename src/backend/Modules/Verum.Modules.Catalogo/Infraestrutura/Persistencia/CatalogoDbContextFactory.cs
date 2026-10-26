using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia;

internal sealed class CatalogoDbContextFactory : IDesignTimeDbContextFactory<CatalogoDbContext>
{
  public CatalogoDbContext CreateDbContext(string[] args)
  {
    var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings__PostgreSQL para executar migrations.");

    var options = new DbContextOptionsBuilder<CatalogoDbContext>()
      .UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "catalogo"))
      .Options;

    return new CatalogoDbContext(options);
  }
}
