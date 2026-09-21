namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Categoria
{
  public Guid Id { get; private set; }
  public Guid? CategoriaPaiId { get; private set; }
  public string Nome { get; private set; } = null!;
  public string Slug { get; private set; } = null!;
  public bool Ativa { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }
  public DateTimeOffset AtualizadaEm { get; private set; }

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
}

