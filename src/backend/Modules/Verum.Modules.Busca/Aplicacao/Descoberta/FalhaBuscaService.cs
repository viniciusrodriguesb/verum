using Microsoft.EntityFrameworkCore;
using Verum.Modules.Busca.Contratos;
using Verum.Modules.Busca.Infraestrutura.Persistencia;

namespace Verum.Modules.Busca.Aplicacao.Descoberta;

internal sealed class FalhaBuscaService(BuscaDbContext db) : IFalhaBuscaService
{
  public async Task RegistrarAsync(Guid buscaId, CancellationToken cancellationToken)
  {
    if (buscaId == Guid.Empty) return;

    await db.Database.OpenConnectionAsync(cancellationToken);

    try
    {
      await db.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT pg_advisory_lock(hashtextextended({buscaId.ToString()}, 710004))", cancellationToken);

      var busca = await db.Buscas.SingleOrDefaultAsync(x => x.Id == buscaId, cancellationToken);

      if (busca is null || busca.ConcluidaEm.HasValue) return;

      busca.RegistrarFalhaTecnica();

      await db.SaveChangesAsync(cancellationToken);
    }
    finally
    {
      try
      {
        await db.Database.ExecuteSqlInterpolatedAsync(
          $"SELECT pg_advisory_unlock(hashtextextended({buscaId.ToString()}, 710004))", CancellationToken.None);
      }
      finally
      {
        await db.Database.CloseConnectionAsync();
      }
    }
  }
}
