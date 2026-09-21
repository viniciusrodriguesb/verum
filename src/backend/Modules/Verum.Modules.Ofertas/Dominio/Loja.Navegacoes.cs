namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class Loja
{
  private readonly List<Oferta> _ofertas = [];
  public IReadOnlyCollection<Oferta> Ofertas => _ofertas.AsReadOnly();
}

