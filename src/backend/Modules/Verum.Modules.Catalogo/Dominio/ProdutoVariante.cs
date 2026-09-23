namespace Verum.Modules.Catalogo.Dominio;

internal sealed class ProdutoVariante
{
  public Guid Id { get; private set; }

  public Guid ProdutoId { get; private set; }

  public string Nome { get; private set; } = null!;

  public string NomeNormalizado { get; private set; } = null!;

  public string Slug { get; private set; } = null!;

  public string Atributos { get; private set; } = null!;

  public StatusProduto Status { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }


  public Produto Produto { get; private set; } = null!;

  private readonly List<ProdutoIdentificador> _identificadores = [];

  public IReadOnlyCollection<ProdutoIdentificador> Identificadores => _identificadores.AsReadOnly();

  private readonly List<ProdutoTermoBusca> _termosBusca = [];

  public IReadOnlyCollection<ProdutoTermoBusca> TermosBusca => _termosBusca.AsReadOnly();

  private readonly List<ProdutoImagem> _imagens = [];

  public IReadOnlyCollection<ProdutoImagem> Imagens => _imagens.AsReadOnly();

  // Materialização pelo EF Core.
  private ProdutoVariante() { }

  public ProdutoVariante(
    Guid produtoId,
    string nome,
    string slug,
    string atributos)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    ProdutoId = ValidarProdutoId(produtoId);

    Nome = ValidarNome(nome);

    NomeNormalizado = ValidarNomeNormalizado(Nome.ToLowerInvariant());

    Slug = ValidarSlug(slug);

    Atributos = ValidarAtributos(atributos);

    Status = ValidarStatus(StatusProduto.Ativo);

    CriadaEm = ValidarCriadaEm(agora);

    AtualizadaEm = ValidarAtualizadaEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarProdutoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ProdutoId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 300);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 300);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 320);

  private static string ValidarAtributos(string valor) =>
    Validacao.Json(valor, nameof(Atributos));

  private static StatusProduto ValidarStatus(StatusProduto valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}
