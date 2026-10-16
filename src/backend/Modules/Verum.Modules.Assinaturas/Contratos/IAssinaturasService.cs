using Verum.Modules.Assinaturas.Contratos.DTOs;

namespace Verum.Modules.Assinaturas.Contratos;

public interface IAssinaturasService
{
  Task<IReadOnlyList<PlanoConsulta>> ListarPlanosAsync(CancellationToken cancellationToken);

  Task<IReadOnlyList<AssinaturaConsulta>> ListarAsync(Guid contaId, CancellationToken cancellationToken);

  Task<AssinaturaConsulta> CriarAsync(Guid contaId, string email, SolicitarAssinatura pedido, CancellationToken cancellationToken);

  Task<AssinaturaConsulta> ObterAsync(Guid contaId, Guid id, CancellationToken cancellationToken);

  Task CancelarAsync(Guid contaId, Guid id, CancellationToken cancellationToken);
}
