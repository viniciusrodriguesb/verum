using Verum.Modules.Notificacoes.Contratos.DTOs;

namespace Verum.Modules.Notificacoes.Contratos;

public interface INotificacoesRadarService
{
  bool EmailDisponivel { get; }

  Task<PreferenciasAviso> PreferenciasAsync(Guid contaId, CancellationToken cancellationToken);

  Task SalvarPreferenciasAsync(Guid contaId, PreferenciasAviso pedido, CancellationToken cancellationToken);

  Task RegistrarAsync(AvisoOportunidade aviso, CancellationToken cancellationToken);

  Task EnviarPendentesAsync(CancellationToken cancellationToken);

  Task<IReadOnlyList<NotificacaoConsulta>> ListarAsync(Guid contaId, CancellationToken cancellationToken);
}
