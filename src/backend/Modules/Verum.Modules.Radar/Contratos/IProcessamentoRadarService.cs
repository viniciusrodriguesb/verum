namespace Verum.Modules.Radar.Contratos;

public interface IProcessamentoRadarService
{
  Task<IReadOnlyList<Guid>> PendentesAsync(CancellationToken cancellationToken);

  Task VerificarAsync(Guid monitoramentoId, CancellationToken cancellationToken);

  Task EncaminharAvisosAsync(CancellationToken cancellationToken);
}
