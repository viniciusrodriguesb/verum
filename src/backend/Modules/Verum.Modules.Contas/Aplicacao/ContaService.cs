using Microsoft.EntityFrameworkCore;
using Npgsql;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Contas.Contratos;
using Verum.Modules.Contas.Contratos.DTOs;
using Verum.Modules.Contas.Dominio;
using Verum.Modules.Contas.Infraestrutura.Persistencia;

namespace Verum.Modules.Contas.Aplicacao;

internal sealed class ContaService(ContasDbContext db) : IContaService
{
  public async Task<ContaAutenticada> SincronizarAsync(string sujeito, string email, string nome, CancellationToken cancellationToken)
  {
    var conta = await db.Contas.SingleOrDefaultAsync(x => x.SujeitoIdentidade == sujeito, cancellationToken);

    if (conta is null)
    {
      conta = new Conta(sujeito, email, nome);

      db.Contas.Add(conta);

      try
      {
        await db.SaveChangesAsync(cancellationToken);
      }
      catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
      {
        // Duas abas podem realizar o primeiro acesso simultaneamente. O índice único protege o vínculo.
        db.Entry(conta).State = EntityState.Detached;

        conta = await db.Contas.SingleAsync(x => x.SujeitoIdentidade == sujeito, cancellationToken);
      }
    }

    if (conta.Status != StatusConta.Ativa)
      throw new ErroAplicacaoException("CONTA_INATIVA", "Esta conta não está ativa.", TipoErro.AcessoNegado);

    conta.AtualizarPerfil(email, nome);

    await db.SaveChangesAsync(cancellationToken);

    return new ContaAutenticada(conta.Id, conta.SujeitoIdentidade, conta.Email, conta.NomeExibicao);
  }
}
