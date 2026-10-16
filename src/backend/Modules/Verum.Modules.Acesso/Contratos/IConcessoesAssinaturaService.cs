using System.Data.Common;

namespace Verum.Modules.Acesso.Contratos;

public interface IConcessoesAssinaturaService
{
  Task AplicarAsync(Guid contaId, Guid pagamentoId, string pacote, DateTimeOffset inicio, DateTimeOffset fim,
    bool aprovado, DbConnection conexao, DbTransaction transacao, CancellationToken cancellationToken);
}
