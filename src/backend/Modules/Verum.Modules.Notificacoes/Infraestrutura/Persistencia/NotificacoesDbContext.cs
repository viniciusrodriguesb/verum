using Microsoft.EntityFrameworkCore;
using Verum.Modules.Notificacoes.Dominio;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia;

internal sealed class NotificacoesDbContext(DbContextOptions<NotificacoesDbContext> options) : DbContext(options)
{
  public DbSet<PreferenciaNotificacao> Preferencias => Set<PreferenciaNotificacao>();

  public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

  public DbSet<EntregaNotificacao> Entregas => Set<EntregaNotificacao>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("notificacoes");

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificacoesDbContext).Assembly);
  }
}
