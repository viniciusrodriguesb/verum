namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoIdentificador
{
  public Guid Id { get; private set; }
  public Guid ProdutoVarianteId { get; private set; }
  public TipoIdentificador Tipo { get; private set; }
  public string Valor { get; private set; } = null!;
  public DateTimeOffset CriadoEm { get; private set; }

  // Materialização pelo EF Core.
  private ProdutoIdentificador() { }

  public ProdutoIdentificador(
    Guid produtoVarianteId,
    TipoIdentificador tipo,
    string valor)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ProdutoVarianteId = ValidarProdutoVarianteId(produtoVarianteId);
    Tipo = ValidarTipo(tipo);
    Valor = ValidarValor(valor);
    CriadoEm = ValidarCriadoEm(agora);

  }
}

