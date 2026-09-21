namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoTermoBusca
{
  public Produto Produto { get; private set; } = null!;

  public ProdutoVariante? Variante { get; private set; }
}

