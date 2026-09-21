namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Categoria
{
  public Categoria? CategoriaPai { get; private set; }

  private readonly List<Categoria> _subcategorias = [];
  public IReadOnlyCollection<Categoria> Subcategorias => _subcategorias.AsReadOnly();

  private readonly List<Produto> _produtos = [];
  public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();
}

