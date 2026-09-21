namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoImagem
{
  public Guid Id { get; private set; }
  public Guid ProdutoId { get; private set; }
  public Guid? ProdutoVarianteId { get; private set; }
  public string Url { get; private set; } = null!;
  public string Origem { get; private set; } = null!;
  public bool Principal { get; private set; }
  public short Ordem { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }

  // Materialização pelo EF Core.
  private ProdutoImagem() { }

  public ProdutoImagem(
    Guid produtoId,
    string url,
    string origem,
    bool principal,
    short ordem,
    Guid? produtoVarianteId = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ProdutoId = ValidarProdutoId(produtoId);
    ProdutoVarianteId = ValidarProdutoVarianteId(produtoVarianteId);
    Url = ValidarUrl(url);
    Origem = ValidarOrigem(origem);
    Principal = ValidarPrincipal(principal);
    Ordem = ValidarOrdem(ordem);
    CriadaEm = ValidarCriadaEm(agora);

  }
}

