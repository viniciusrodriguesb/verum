namespace Verum.Modules.Notificacoes.Dominio;

internal sealed partial class EntregaNotificacao
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
}

