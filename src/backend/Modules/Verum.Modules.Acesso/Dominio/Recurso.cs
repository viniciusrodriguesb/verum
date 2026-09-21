namespace Verum.Modules.Acesso.Dominio;

internal sealed partial class Recurso
{
  public Guid Id { get; private set; }
  public string Codigo { get; private set; } = null!;
  public string Nome { get; private set; } = null!;
  public string Descricao { get; private set; } = null!;
  public TipoLimite TipoLimite { get; private set; }
  public bool Ativo { get; private set; }
  public DateTimeOffset CriadoEm { get; private set; }

  // Materialização pelo EF Core.
  private Recurso() { }

  public Recurso(
    string codigo,
    string nome,
    string descricao,
    TipoLimite tipoLimite)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    Codigo = ValidarCodigo(codigo);
    Nome = ValidarNome(nome);
    Descricao = ValidarDescricao(descricao);
    TipoLimite = ValidarTipoLimite(tipoLimite);
    Ativo = ValidarAtivo(true);
    CriadoEm = ValidarCriadoEm(agora);

  }
}

