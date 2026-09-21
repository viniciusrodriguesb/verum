using Microsoft.EntityFrameworkCore;
using Verum.Modules.Acesso.Dominio;

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia;

internal sealed class AcessoDbContext(DbContextOptions<AcessoDbContext> options) : DbContext(options)
{
  public DbSet<RegistroUso> RegistrosUso => Set<RegistroUso>();
  public DbSet<Recurso> Recursos => Set<Recurso>();
  public DbSet<PacoteAcesso> Pacotes => Set<PacoteAcesso>();
  public DbSet<PacoteRecurso> RecursosPacotes => Set<PacoteRecurso>();
  public DbSet<ConcessaoPacote> Concessoes => Set<ConcessaoPacote>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("acesso");
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AcessoDbContext).Assembly);
  }
}
