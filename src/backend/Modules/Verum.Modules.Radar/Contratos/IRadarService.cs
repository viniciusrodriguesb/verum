using Verum.Modules.Radar.Contratos.DTOs;

namespace Verum.Modules.Radar.Contratos;

public interface IRadarService
{
  Task<CapacidadeRadar> CapacidadeAsync(Guid? contaId, CancellationToken cancellationToken);

  Task<IReadOnlyList<MonitoramentoConsulta>> ListarAsync(Guid contaId, CancellationToken cancellationToken);

  Task<MonitoramentoConsulta> CriarAsync(Guid contaId, CriarMonitoramento pedido, CancellationToken cancellationToken);

  Task AlterarAsync(Guid contaId, Guid id, decimal precoAlvo, string acao, CancellationToken cancellationToken);
}
