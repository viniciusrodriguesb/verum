namespace Verum.Modules.Notificacoes.Dominio;

internal sealed class Notificacao
{
  public Guid Id { get; private set; }

  public Guid ContaId { get; private set; }

  public TipoNotificacao Tipo { get; private set; }

  public string Titulo { get; private set; } = null!;

  public string Mensagem { get; private set; } = null!;

  public string? Dados { get; private set; }

  public StatusNotificacao Status { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset? LidaEm { get; private set; }

  private readonly List<EntregaNotificacao> _entregas = [];

  public IReadOnlyCollection<EntregaNotificacao> Entregas => _entregas.AsReadOnly();

  // Materialização pelo EF Core.
  private Notificacao() { }

  public Notificacao(
    Guid contaId,
    TipoNotificacao tipo,
    string titulo,
    string mensagem,
    string? dados = null)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    ContaId = ValidarContaId(contaId);

    Tipo = ValidarTipo(tipo);

    Titulo = ValidarTitulo(titulo);

    Mensagem = ValidarMensagem(mensagem);

    Dados = ValidarDados(dados);

    Status = ValidarStatus(StatusNotificacao.NaoLida);

    CriadaEm = ValidarCriadaEm(agora);

    LidaEm = ValidarLidaEm(null);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static TipoNotificacao ValidarTipo(TipoNotificacao valor) =>
    Validacao.Enumeracao(valor, nameof(Tipo));

  private static string ValidarTitulo(string valor) =>
    Validacao.Texto(valor, nameof(Titulo), 200);

  private static string ValidarMensagem(string valor) =>
    Validacao.Texto(valor, nameof(Mensagem), 1000);

  private static string? ValidarDados(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(Dados));

  private static StatusNotificacao ValidarStatus(StatusNotificacao valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset? ValidarLidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(LidaEm));
}
