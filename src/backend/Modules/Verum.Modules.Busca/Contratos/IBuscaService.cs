using Verum.Modules.Busca.Contratos.DTOs;

namespace Verum.Modules.Busca.Contratos;

public interface IBuscaService
{
  Task<BuscaAceita> SolicitarAsync(Guid visitanteId, SolicitarBusca pedido, CancellationToken cancellationToken);

  Task<BuscaConsulta> ObterAsync(Guid visitanteId, Guid buscaId, CancellationToken cancellationToken);
}
