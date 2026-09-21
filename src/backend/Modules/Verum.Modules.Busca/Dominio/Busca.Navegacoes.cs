namespace Verum.Modules.Busca.Dominio;

internal sealed partial class Busca
{
  private readonly List<BuscaEtapa> _etapas = [];
  public IReadOnlyCollection<BuscaEtapa> Etapas => _etapas.AsReadOnly();

  public ResultadoBusca? Resultado { get; private set; }
}

