namespace Verum.Modules.Catalogo.Dominio;

internal sealed class Produto
{
  public Guid Id { get; private set; }

  public Guid CategoriaId { get; private set; }

  public Guid MarcaId { get; private set; }

  public string Nome { get; private set; } = null!;

  public string NomeNormalizado { get; private set; } = null!;

  public string? Modelo { get; private set; }

  public string Slug { get; private set; } = null!;

  public string? Descricao { get; private set; }

  public string Atributos { get; private set; } = null!;

  public StatusProduto Status { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }

  public DateTimeOffset AtualizadoEm { get; private set; }


  public Categoria Categoria { get; private set; } = null!;

  public Marca Marca { get; private set; } = null!;

  private readonly List<ProdutoVariante> _variantes = [];

  public IReadOnlyCollection<ProdutoVariante> Variantes => _variantes.AsReadOnly();

  private readonly List<ProdutoTermoBusca> _termosBusca = [];

  public IReadOnlyCollection<ProdutoTermoBusca> TermosBusca => _termosBusca.AsReadOnly();

  private readonly List<ProdutoImagem> _imagens = [];

  public IReadOnlyCollection<ProdutoImagem> Imagens => _imagens.AsReadOnly();

  // Materialização pelo EF Core.
  private Produto() { }

  public Produto(
    Guid categoriaId,
    Guid marcaId,
    string nome,
    string slug,
    string atributos,
    string? modelo = null,
    string? descricao = null)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    CategoriaId = ValidarCategoriaId(categoriaId);

    MarcaId = ValidarMarcaId(marcaId);

    Nome = ValidarNome(nome);

    NomeNormalizado = ValidarNomeNormalizado(Nome.ToLowerInvariant());

    Modelo = ValidarModelo(modelo);

    Slug = ValidarSlug(slug);

    Descricao = ValidarDescricao(descricao);

    Atributos = ValidarAtributos(atributos);

    Status = ValidarStatus(StatusProduto.Ativo);

    CriadoEm = ValidarCriadoEm(agora);

    AtualizadoEm = ValidarAtualizadoEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarCategoriaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(CategoriaId));

  private static Guid ValidarMarcaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(MarcaId));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 250);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 250);

  private static string? ValidarModelo(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(Modelo), 120);

  private static string ValidarSlug(string valor) =>
    Validacao.Texto(valor, nameof(Slug), 280);

  private static string? ValidarDescricao(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(Descricao), int.MaxValue);

  private static string ValidarAtributos(string valor) =>
    Validacao.Json(valor, nameof(Atributos));

  private static StatusProduto ValidarStatus(StatusProduto valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}
