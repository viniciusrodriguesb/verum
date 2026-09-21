namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class Recurso
{
  private readonly List<PacoteRecurso> _pacotes = [];
  public IReadOnlyCollection<PacoteRecurso> Pacotes => _pacotes.AsReadOnly();
}

