namespace Verum.Modules.Acesso.Dominio;

internal sealed class Recurso
{
  public Guid Id { get; private set; }

  public string Codigo { get; private set; } = null!;

  public string Nome { get; private set; } = null!;

  public string Descricao { get; private set; } = null!;

  public TipoLimite TipoLimite { get; private set; }

  public bool Ativo { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }

  private readonly List<PacoteRecurso> _pacotes = [];

  public IReadOnlyCollection<PacoteRecurso> Pacotes => _pacotes.AsReadOnly();

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

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 100);

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 150);

  private static string ValidarDescricao(string valor) =>
    Validacao.Texto(valor, nameof(Descricao), 500);

  private static TipoLimite ValidarTipoLimite(TipoLimite valor) =>
    Validacao.Enumeracao(valor, nameof(TipoLimite));

  private static bool ValidarAtivo(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));
}
