using Verum.Modules.Ofertas.Contratos.Radar.DTOs;

namespace Verum.Modules.Ofertas.Contratos.Radar;

public interface IOfertaRadarService
{
  Task<OfertaRadar?> ObterAsync(Guid ofertaId, CancellationToken cancellationToken);

  Task<OfertaRadar?> AtualizarAsync(Guid ofertaId, CancellationToken cancellationToken);
}
