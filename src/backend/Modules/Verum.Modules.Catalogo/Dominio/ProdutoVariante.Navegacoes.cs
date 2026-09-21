namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoVariante
{
  public Produto Produto { get; private set; } = null!;

  private readonly List<ProdutoIdentificador> _identificadores = [];
  public IReadOnlyCollection<ProdutoIdentificador> Identificadores => _identificadores.AsReadOnly();

  private readonly List<ProdutoTermoBusca> _termosBusca = [];
  public IReadOnlyCollection<ProdutoTermoBusca> TermosBusca => _termosBusca.AsReadOnly();

  private readonly List<ProdutoImagem> _imagens = [];
  public IReadOnlyCollection<ProdutoImagem> Imagens => _imagens.AsReadOnly();
}

