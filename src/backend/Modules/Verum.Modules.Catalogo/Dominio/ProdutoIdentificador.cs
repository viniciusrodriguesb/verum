namespace Verum.Modules.Catalogo.Dominio;

internal sealed class ProdutoIdentificador
{
  public Guid Id { get; private set; }

  public Guid ProdutoVarianteId { get; private set; }

  public TipoIdentificador Tipo { get; private set; }

  public string Valor { get; private set; } = null!;

  public DateTimeOffset CriadoEm { get; private set; }


  public ProdutoVariante Variante { get; private set; } = null!;

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

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoVarianteId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoVarianteId));

  private static TipoIdentificador ValidarTipo(TipoIdentificador valor) =>
    Validacao.Enumeracao(valor, nameof(Tipo));

  private static string ValidarValor(string valor) =>
    Validacao.Texto(valor, nameof(Valor), 100);

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));
}
