namespace Verum.Modules.Catalogo.Dominio;

internal sealed class Categoria
{
  public Guid Id { get; private set; }

  public Guid? CategoriaPaiId { get; private set; }

  public string Nome { get; private set; } = null!;

  public string Slug { get; private set; } = null!;

  public bool Ativa { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }


  public Categoria? CategoriaPai { get; private set; }

  private readonly List<Categoria> _subcategorias = [];

  public IReadOnlyCollection<Categoria> Subcategorias => _subcategorias.AsReadOnly();

  private readonly List<Produto> _produtos = [];

  public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();

  // Materialização pelo EF Core.
  private Categoria() { }

  public Categoria(
    string nome,
    string slug,
    Guid? categoriaPaiId = null)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    CategoriaPaiId = ValidarCategoriaPaiId(categoriaPaiId);

    Nome = ValidarNome(nome);

    Slug = ValidarSlug(slug);

    Ativa = ValidarAtiva(true);

    CriadaEm = ValidarCriadaEm(agora);

    AtualizadaEm = ValidarAtualizadaEm(agora);

    if (!(CategoriaPaiId != Id)) throw new ArgumentException("Uma categoria não pode ser sua própria categoria pai.");
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid? ValidarCategoriaPaiId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(CategoriaPaiId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 140);

  private static bool ValidarAtiva(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}
