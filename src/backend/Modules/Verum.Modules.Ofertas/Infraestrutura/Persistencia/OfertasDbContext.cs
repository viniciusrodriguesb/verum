using Microsoft.EntityFrameworkCore;
using Verum.Modules.Ofertas.Dominio;

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia;

internal sealed class OfertasDbContext(DbContextOptions<OfertasDbContext> options) : DbContext(options)
{
  public DbSet<Loja> Lojas => Set<Loja>();
  public DbSet<FonteOferta> Fontes => Set<FonteOferta>();
  public DbSet<Oferta> Ofertas => Set<Oferta>();
  public DbSet<OfertaObservacao> Observacoes => Set<OfertaObservacao>();
  public DbSet<ExecucaoConsultaFonte> ExecucoesConsultaFonte => Set<ExecucaoConsultaFonte>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("ofertas");
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(OfertasDbContext).Assembly);
  }
}
