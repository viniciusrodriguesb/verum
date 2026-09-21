namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoTermoBusca
{
  public Guid Id { get; private set; }
  public Guid ProdutoId { get; private set; }
  public Guid? ProdutoVarianteId { get; private set; }
  public string TermoOriginal { get; private set; } = null!;
  public string TermoNormalizado { get; private set; } = null!;
  public OrigemTermo Origem { get; private set; }
  public decimal Confianca { get; private set; }
  public DateTimeOffset CriadoEm { get; private set; }

  // Materialização pelo EF Core.
  private ProdutoTermoBusca() { }

  public ProdutoTermoBusca(
    Guid produtoId,
    string termoOriginal,
    OrigemTermo origem,
    decimal confianca,
    Guid? produtoVarianteId = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ProdutoId = ValidarProdutoId(produtoId);
    ProdutoVarianteId = ValidarProdutoVarianteId(produtoVarianteId);
    TermoOriginal = ValidarTermoOriginal(termoOriginal);
    TermoNormalizado = ValidarTermoNormalizado(TermoOriginal.ToLowerInvariant());
    Origem = ValidarOrigem(origem);
    Confianca = ValidarConfianca(confianca);
    CriadoEm = ValidarCriadoEm(agora);

  }
}

