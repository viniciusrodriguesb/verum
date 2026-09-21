namespace Verum.Modules.Notificacoes.Dominio;

internal sealed partial class Notificacao
{
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

