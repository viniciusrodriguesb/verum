namespace Verum.Modules.Catalogo.Dominio;

internal sealed class ProdutoTermoBusca
{
  public Guid Id { get; private set; }

  public Guid ProdutoId { get; private set; }

  public Guid? ProdutoVarianteId { get; private set; }

  public string TermoOriginal { get; private set; } = null!;

  public string TermoNormalizado { get; private set; } = null!;

  public OrigemTermo Origem { get; private set; }

  public decimal Confianca { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }


  public Produto Produto { get; private set; } = null!;

  public ProdutoVariante? Variante { get; private set; }

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

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static Guid? ValidarProdutoVarianteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVarianteId));

  private static string ValidarTermoOriginal(string valor) =>
    Validacao.Texto(valor, nameof(TermoOriginal), 300);

  private static string ValidarTermoNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(TermoNormalizado), 300);

  private static OrigemTermo ValidarOrigem(OrigemTermo valor) =>
    Validacao.Enumeracao(valor, nameof(Origem));

  private static decimal ValidarConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(Confianca), 0m, 1m, 4);

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));
}
