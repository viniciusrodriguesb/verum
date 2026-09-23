namespace Verum.Modules.Notificacoes.Dominio;

internal sealed class EntregaNotificacao
{
  public Guid Id { get; private set; }

  public Guid NotificacaoId { get; private set; }

  public CanalNotificacao Canal { get; private set; }

  public StatusEntrega Status { get; private set; }

  public short QuantidadeTentativas { get; private set; }

  public DateTimeOffset? ProximaTentativaEm { get; private set; }

  public DateTimeOffset? UltimaTentativaEm { get; private set; }

  public DateTimeOffset? EntregueEm { get; private set; }

  public string? CodigoErro { get; private set; }

  public string? DetalhesErro { get; private set; }

  public string? ReferenciaProvedor { get; private set; }


  public Notificacao Notificacao { get; private set; } = null!;

  // Materialização pelo EF Core.
  private EntregaNotificacao() { }

  public EntregaNotificacao(
    Guid notificacaoId,
    CanalNotificacao canal)
  {
    Id = ValidarId(Guid.CreateVersion7());

    NotificacaoId = ValidarNotificacaoId(notificacaoId);

    Canal = ValidarCanal(canal);

    Status = ValidarStatus(StatusEntrega.Pendente);

    QuantidadeTentativas = ValidarQuantidadeTentativas(0);

    ProximaTentativaEm = ValidarProximaTentativaEm(null);

    UltimaTentativaEm = ValidarUltimaTentativaEm(null);

    EntregueEm = ValidarEntregueEm(null);

    CodigoErro = ValidarCodigoErro(null);

    DetalhesErro = ValidarDetalhesErro(null);

    ReferenciaProvedor = ValidarReferenciaProvedor(null);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarNotificacaoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(NotificacaoId));

  private static CanalNotificacao ValidarCanal(CanalNotificacao valor) =>
    Validacao.Enumeracao(valor, nameof(Canal));

  private static StatusEntrega ValidarStatus(StatusEntrega valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static short ValidarQuantidadeTentativas(short valor) =>
    (short)Validacao.Numero(valor, nameof(QuantidadeTentativas), 0m, 32767m, 0);

  private static DateTimeOffset? ValidarProximaTentativaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ProximaTentativaEm));

  private static DateTimeOffset? ValidarUltimaTentativaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(UltimaTentativaEm));

  private static DateTimeOffset? ValidarEntregueEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(EntregueEm));

  private static string? ValidarCodigoErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(CodigoErro), 100);

  private static string? ValidarDetalhesErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(DetalhesErro), 1000);

  private static string? ValidarReferenciaProvedor(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(ReferenciaProvedor), 200);
}
