namespace Verum.Modules.Acesso.Dominio;

internal sealed class PacoteAcesso
{
  public Guid Id { get; private set; }

  public string Codigo { get; private set; } = null!;

  public string Nome { get; private set; } = null!;

  public bool Ativo { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }

  public DateTimeOffset AtualizadoEm { get; private set; }

  private readonly List<PacoteRecurso> _recursos = [];

  public IReadOnlyCollection<PacoteRecurso> Recursos => _recursos.AsReadOnly();

  private readonly List<ConcessaoPacote> _concessoes = [];

  public IReadOnlyCollection<ConcessaoPacote> Concessoes => _concessoes.AsReadOnly();

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

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 80);

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static bool ValidarAtivo(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}
