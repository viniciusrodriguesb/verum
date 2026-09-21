using Microsoft.EntityFrameworkCore;
using Verum.Modules.Radar.Dominio;

namespace Verum.Modules.Radar.Infraestrutura.Persistencia;

internal sealed class RadarDbContext(DbContextOptions<RadarDbContext> options) : DbContext(options)
{
  public DbSet<Monitoramento> Monitoramentos => Set<Monitoramento>();
  public DbSet<Oportunidade> Oportunidades => Set<Oportunidade>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("radar");
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(RadarDbContext).Assembly);
  }
}
