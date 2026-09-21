namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class ProdutoVariante
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
}

