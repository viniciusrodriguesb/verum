namespace Verum.Modules.Acesso.Contratos;

public interface IAcessoRadarService
{
  Task<bool> PermitidoAsync(Guid contaId, CancellationToken cancellationToken);
}
