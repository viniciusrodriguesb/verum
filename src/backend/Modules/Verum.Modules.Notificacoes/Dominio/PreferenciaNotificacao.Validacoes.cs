namespace Verum.Modules.Notificacoes.Dominio;

internal sealed partial class PreferenciaNotificacao
{
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

