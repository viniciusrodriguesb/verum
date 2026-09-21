namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Produto
{
  public Categoria Categoria { get; private set; } = null!;

  public Marca Marca { get; private set; } = null!;

  private readonly List<ProdutoVariante> _variantes = [];
  public IReadOnlyCollection<ProdutoVariante> Variantes => _variantes.AsReadOnly();

  private readonly List<ProdutoTermoBusca> _termosBusca = [];
  public IReadOnlyCollection<ProdutoTermoBusca> TermosBusca => _termosBusca.AsReadOnly();

  private readonly List<ProdutoImagem> _imagens = [];
  public IReadOnlyCollection<ProdutoImagem> Imagens => _imagens.AsReadOnly();
}

