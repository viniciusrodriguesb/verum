namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Produto
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
}

