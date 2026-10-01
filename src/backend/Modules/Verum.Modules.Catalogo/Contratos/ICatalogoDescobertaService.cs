namespace Verum.Modules.Catalogo.Contratos;

public interface ICatalogoDescobertaService
{
  Task<Guid[]> ResolverAsync(string consulta, CancellationToken cancellationToken);

  Task<Guid> RegistrarAsync(string consulta, string nome, string url, CancellationToken cancellationToken);
}

