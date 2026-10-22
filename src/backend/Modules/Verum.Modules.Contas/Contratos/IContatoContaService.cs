namespace Verum.Modules.Contas.Contratos;

public interface IContatoContaService
{
  Task<string?> EmailAtivoAsync(Guid contaId, CancellationToken cancellationToken);
}
