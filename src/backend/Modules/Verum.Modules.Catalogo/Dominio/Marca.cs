namespace Verum.Modules.Catalogo.Dominio;

internal sealed partial class Marca
{
  public Guid Id { get; private set; }
  public string Nome { get; private set; } = null!;
  public string NomeNormalizado { get; private set; } = null!;
  public string Slug { get; private set; } = null!;
  public bool Ativa { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }

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
}

