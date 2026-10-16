using Verum.BuildingBlocks.Identidade;
using Verum.Modules.Busca.Contratos.DTOs;

namespace Verum.Modules.Busca.Contratos;

public interface IBuscaService
{
  Task<BuscaAceita> SolicitarAsync(AtorBusca ator, SolicitarBusca pedido, CancellationToken cancellationToken);

  Task<BuscaConsulta> ObterAsync(AtorBusca ator, Guid buscaId, CancellationToken cancellationToken);

  Task<IReadOnlyList<BuscaHistorico>> ObterHistoricoAsync(AtorBusca ator, CancellationToken cancellationToken);

  Task<BuscaAceita> SolicitarAsync(Guid visitanteId, SolicitarBusca pedido, CancellationToken cancellationToken) =>
    SolicitarAsync(AtorBusca.Visitante(visitanteId), pedido, cancellationToken);

  Task<BuscaConsulta> ObterAsync(Guid visitanteId, Guid buscaId, CancellationToken cancellationToken) =>
    ObterAsync(AtorBusca.Visitante(visitanteId), buscaId, cancellationToken);
}
