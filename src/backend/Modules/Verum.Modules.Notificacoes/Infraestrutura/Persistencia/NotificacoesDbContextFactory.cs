using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia;

// Utilizado apenas pelo dotnet ef, sem inicializar API, filas ou serviços externos.
internal sealed class NotificacoesDbContextFactory : IDesignTimeDbContextFactory<NotificacoesDbContext>
{
  public NotificacoesDbContext CreateDbContext(string[] args)
  {
    var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");

    if (string.IsNullOrWhiteSpace(connection))
      throw new InvalidOperationException("Configure ConnectionStrings__PostgreSQL para executar migrations.");

    var options = new DbContextOptionsBuilder<NotificacoesDbContext>()
      .UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "notificacoes"))
      .Options;

    return new NotificacoesDbContext(options);
  }
}
