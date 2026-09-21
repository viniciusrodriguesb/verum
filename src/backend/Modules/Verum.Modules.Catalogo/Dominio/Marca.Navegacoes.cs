namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Marca
{
  private readonly List<Produto> _produtos = [];
  public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();
}

