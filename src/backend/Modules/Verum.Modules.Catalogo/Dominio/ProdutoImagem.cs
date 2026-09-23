namespace Verum.Modules.Catalogo.Dominio;

internal sealed class ProdutoImagem
{
  public Guid Id { get; private set; }

  public Guid ProdutoId { get; private set; }

  public Guid? ProdutoVarianteId { get; private set; }

  public string Url { get; private set; } = null!;

  public string Origem { get; private set; } = null!;

  public bool Principal { get; private set; }

  public short Ordem { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }


  public Produto Produto { get; private set; } = null!;

  public ProdutoVariante? Variante { get; private set; }

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

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static Guid? ValidarProdutoVarianteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVarianteId));

  private static string ValidarUrl(string valor) =>
    Validacao.Url(valor, nameof(Url));

  private static string ValidarOrigem(string valor) =>
    Validacao.Texto(valor, nameof(Origem), 100);

  private static bool ValidarPrincipal(bool valor) =>
    valor;

  private static short ValidarOrdem(short valor) =>
    (short)Validacao.Numero(valor, nameof(Ordem), 0m, 32767m, 0);

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}
