using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verum.Modules.Radar.Infraestrutura.Persistencia;

// Utilizado apenas pelo dotnet ef, sem inicializar API, filas ou serviços externos.
internal sealed class RadarDbContextFactory : IDesignTimeDbContextFactory<RadarDbContext>
{
  public RadarDbContext CreateDbContext(string[] args)
  {
    var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings__PostgreSQL para executar migrations.");

    var options = new DbContextOptionsBuilder<RadarDbContext>()
      .UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "radar"))
      .Options;

    return new RadarDbContext(options);
  }
}
