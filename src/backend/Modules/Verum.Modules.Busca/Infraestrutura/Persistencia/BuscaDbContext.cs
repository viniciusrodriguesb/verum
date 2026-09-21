using Microsoft.EntityFrameworkCore;
using Verum.Modules.Busca.Dominio;
using BuscaEntidade = Verum.Modules.Busca.Dominio.Busca;

namespace Verum.Modules.Busca.Infraestrutura.Persistencia;

internal sealed class BuscaDbContext(DbContextOptions<BuscaDbContext> options) : DbContext(options)
{
  public DbSet<BuscaEntidade> Buscas => Set<BuscaEntidade>();
  public DbSet<BuscaEtapa> Etapas => Set<BuscaEtapa>();
  public DbSet<ResultadoBusca> Resultados => Set<ResultadoBusca>();
  public DbSet<ResultadoOferta> OfertasResultado => Set<ResultadoOferta>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("busca");
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuscaDbContext).Assembly);
  }
}
