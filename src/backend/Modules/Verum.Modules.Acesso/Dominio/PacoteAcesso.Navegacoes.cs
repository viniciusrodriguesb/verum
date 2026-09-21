namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class PacoteAcesso
{
  private readonly List<PacoteRecurso> _recursos = [];
  public IReadOnlyCollection<PacoteRecurso> Recursos => _recursos.AsReadOnly();

  private readonly List<ConcessaoPacote> _concessoes = [];
  public IReadOnlyCollection<ConcessaoPacote> Concessoes => _concessoes.AsReadOnly();
}

