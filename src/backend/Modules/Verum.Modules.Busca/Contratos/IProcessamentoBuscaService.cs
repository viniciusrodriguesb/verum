namespace Verum.Modules.Busca.Contratos;

public interface IProcessamentoBuscaService
{
  Task ProcessarAsync(Guid buscaId, CancellationToken cancellationToken);
}
