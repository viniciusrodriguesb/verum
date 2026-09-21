namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class PacoteAcesso
{
  public Guid Id { get; private set; }
  public string Codigo { get; private set; } = null!;
  public string Nome { get; private set; } = null!;
  public bool Ativo { get; private set; }
  public DateTimeOffset CriadoEm { get; private set; }
  public DateTimeOffset AtualizadoEm { get; private set; }

  // Materialização pelo EF Core.
  private PacoteAcesso() { }

  public PacoteAcesso(
    string codigo,
    string nome)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    Codigo = ValidarCodigo(codigo);
    Nome = ValidarNome(nome);
    Ativo = ValidarAtivo(true);
    CriadoEm = ValidarCriadoEm(agora);
    AtualizadoEm = ValidarAtualizadoEm(agora);

  }
}

