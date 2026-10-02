namespace Verum.Modules.Busca.Contratos;

public interface IFalhaBuscaService
{
  Task RegistrarAsync(Guid buscaId, CancellationToken cancellationToken);
}
