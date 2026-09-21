namespace Verum.Modules.Busca.Dominio;

internal sealed partial class ResultadoBusca
{
  public Busca Busca { get; private set; } = null!;

  private readonly List<ResultadoOferta> _ofertas = [];
  public IReadOnlyCollection<ResultadoOferta> Ofertas => _ofertas.AsReadOnly();
}

