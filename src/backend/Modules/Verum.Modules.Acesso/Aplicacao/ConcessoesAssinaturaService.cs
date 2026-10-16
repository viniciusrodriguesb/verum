using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Verum.Modules.Acesso.Contratos;
using Verum.Modules.Acesso.Dominio;
using Verum.Modules.Acesso.Infraestrutura.Persistencia;

namespace Verum.Modules.Acesso.Aplicacao;

internal sealed class ConcessoesAssinaturaService(AcessoDbContext db) : IConcessoesAssinaturaService
{
  public async Task AplicarAsync(Guid contaId, Guid pagamentoId, string pacote, DateTimeOffset inicio, DateTimeOffset fim,
    bool aprovado, DbConnection conexao, DbTransaction transacao, CancellationToken cancellationToken)
  {
    db.Database.SetDbConnection(conexao, contextOwnsConnection: false);

    await db.Database.UseTransactionAsync(transacao, cancellationToken);

    try
    {
      await db.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT pg_advisory_xact_lock(hashtextextended({pagamentoId.ToString()}, 710009))", cancellationToken);

      var concessao = await db.Concessoes.SingleOrDefaultAsync(x => x.Origem == OrigemConcessao.Assinatura
        && x.ReferenciaOrigemId == pagamentoId, cancellationToken);

      if (!aprovado)
        concessao?.Revogar();
      else if (concessao is null)
      {
        var pacoteId = await db.Pacotes.Where(x => x.Codigo == pacote && x.Ativo).Select(x => x.Id).SingleAsync(cancellationToken);

        db.Concessoes.Add(new ConcessaoPacote(contaId, pacoteId, OrigemConcessao.Assinatura, inicio, pagamentoId, fim));
      }

      await db.SaveChangesAsync(cancellationToken);
    }
    finally
    {
      await db.Database.UseTransactionAsync((DbTransaction?)null, CancellationToken.None);
    }
  }
}
