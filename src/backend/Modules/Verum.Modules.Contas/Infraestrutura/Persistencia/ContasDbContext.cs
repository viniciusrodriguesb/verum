using Microsoft.EntityFrameworkCore;
using Verum.Modules.Contas.Dominio;

namespace Verum.Modules.Contas.Infraestrutura.Persistencia;

internal sealed class ContasDbContext(DbContextOptions<ContasDbContext> options) : DbContext(options)
{
  public DbSet<Conta> Contas => Set<Conta>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("contas");

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContasDbContext).Assembly);
  }
}
