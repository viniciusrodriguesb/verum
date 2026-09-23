using Microsoft.EntityFrameworkCore;
using Verum.Modules.Assinaturas.Dominio;

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia;

internal sealed class AssinaturasDbContext(DbContextOptions<AssinaturasDbContext> options) : DbContext(options)
{
  public DbSet<Plano> Planos => Set<Plano>();

  public DbSet<ClienteGateway> ClientesGateway => Set<ClienteGateway>();

  public DbSet<Assinatura> Assinaturas => Set<Assinatura>();

  public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

  public DbSet<EventoGateway> EventosGateway => Set<EventoGateway>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("assinaturas");

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssinaturasDbContext).Assembly);
  }
}
