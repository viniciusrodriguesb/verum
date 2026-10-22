namespace Verum.Modules.Notificacoes.Dominio;

internal sealed class PreferenciaNotificacao
{
  public Guid Id { get; private set; }

  public Guid ContaId { get; private set; }

  public CanalNotificacao Canal { get; private set; }

  public bool Habilitada { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }

  public int SilencioInicio { get; private set; } = 22;

  public int SilencioFim { get; private set; } = 8;

  public string Fuso { get; private set; } = "America/Sao_Paulo";

  public void Atualizar(bool habilitada, int inicio, int fim, string fuso)
  {
    if (inicio is < 0 or > 23 || fim is < 0 or > 23)
      throw Verum.BuildingBlocks.Erros.ErroAplicacaoException.Validacao("Horários devem estar entre 0 e 23.");

    fuso = Validacao.Texto(fuso, nameof(Fuso), 100);

    try { _ = TimeZoneInfo.FindSystemTimeZoneById(fuso); }
    catch (TimeZoneNotFoundException) { throw Verum.BuildingBlocks.Erros.ErroAplicacaoException.Validacao("Fuso inválido."); }

    Habilitada = habilitada;

    SilencioInicio = inicio;

    SilencioFim = fim;

    Fuso = Validacao.Texto(fuso, nameof(Fuso), 100);

    AtualizadaEm = DateTimeOffset.UtcNow;
  }

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
