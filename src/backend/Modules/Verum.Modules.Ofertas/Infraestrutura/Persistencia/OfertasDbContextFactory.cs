using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia;

// Utilizado apenas pelo dotnet ef, sem inicializar API, filas ou serviços externos.
internal sealed class OfertasDbContextFactory : IDesignTimeDbContextFactory<OfertasDbContext>
{
  public OfertasDbContext CreateDbContext(string[] args)
  {
    var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings__PostgreSQL para executar migrations.");

    var options = new DbContextOptionsBuilder<OfertasDbContext>()
      .UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "ofertas"))
      .Options;

    return new OfertasDbContext(options);
  }
}
