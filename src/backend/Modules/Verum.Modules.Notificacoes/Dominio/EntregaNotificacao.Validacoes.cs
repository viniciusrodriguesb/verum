namespace Verum.Modules.Notificacoes.Dominio;

internal sealed partial class EntregaNotificacao
{
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

