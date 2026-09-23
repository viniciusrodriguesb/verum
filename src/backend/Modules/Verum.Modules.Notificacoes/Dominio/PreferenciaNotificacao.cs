namespace Verum.Modules.Notificacoes.Dominio;

internal sealed class PreferenciaNotificacao
{
  public Guid Id { get; private set; }

  public Guid ContaId { get; private set; }

  public CanalNotificacao Canal { get; private set; }

  public bool Habilitada { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }

  // Materialização pelo EF Core.
  private PreferenciaNotificacao() { }

  public PreferenciaNotificacao(
    Guid contaId,
    CanalNotificacao canal,
    bool habilitada)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    ContaId = ValidarContaId(contaId);

    Canal = ValidarCanal(canal);

    Habilitada = ValidarHabilitada(habilitada);

    CriadaEm = ValidarCriadaEm(agora);

    AtualizadaEm = ValidarAtualizadaEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static CanalNotificacao ValidarCanal(CanalNotificacao valor) =>
    Validacao.Enumeracao(valor, nameof(Canal));

  private static bool ValidarHabilitada(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}
