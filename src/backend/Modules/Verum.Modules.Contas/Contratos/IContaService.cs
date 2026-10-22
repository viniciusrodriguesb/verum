using Verum.Modules.Contas.Contratos.DTOs;

namespace Verum.Modules.Contas.Contratos;

public interface IContaService
{
  Task<ContaAutenticada> ObterAsync(string sujeito, CancellationToken cancellationToken);

  Task<ContaAutenticada> SincronizarAsync(string sujeito, string email, string nome, CancellationToken cancellationToken);
}
