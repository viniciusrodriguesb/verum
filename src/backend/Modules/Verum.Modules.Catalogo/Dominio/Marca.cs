namespace Verum.Modules.Catalogo.Dominio;

internal sealed class Marca
{
  public Guid Id { get; private set; }

  public string Nome { get; private set; } = null!;

  public string NomeNormalizado { get; private set; } = null!;

  public string Slug { get; private set; } = null!;

  public bool Ativa { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  private readonly List<Produto> _produtos = [];

  public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();

  // Materialização pelo EF Core.
  private Marca() { }

  public Marca(
    string nome,
    string slug)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    Nome = ValidarNome(nome);

    NomeNormalizado = ValidarNomeNormalizado(Nome.ToLowerInvariant());

    Slug = ValidarSlug(slug);

    Ativa = ValidarAtiva(true);

    CriadaEm = ValidarCriadaEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 120);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 140);

  private static bool ValidarAtiva(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}
