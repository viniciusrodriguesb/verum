namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class PacoteRecurso
{
  public PacoteAcesso Pacote { get; private set; } = null!;

  public Recurso Recurso { get; private set; } = null!;
}

