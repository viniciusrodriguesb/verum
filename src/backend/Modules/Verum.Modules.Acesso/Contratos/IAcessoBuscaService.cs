using System.Data.Common;
using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Acesso.Contratos.DTOs;

namespace Verum.Modules.Acesso.Contratos;

public interface IAcessoBuscaService
{
  Task PrepararAsync(AtorBusca ator, CancellationToken cancellationToken);

  Task<AcessoBusca> ObterAsync(AtorBusca ator, CancellationToken cancellationToken);

  Task RegistrarAsync(AtorBusca ator, Guid buscaId, DbConnection conexao, DbTransaction transacao, CancellationToken cancellationToken);
}
