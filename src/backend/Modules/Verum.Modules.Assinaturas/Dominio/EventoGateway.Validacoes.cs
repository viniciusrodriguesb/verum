namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class EventoGateway
{
  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static GatewayPagamento ValidarGateway(GatewayPagamento valor) =>
    Validacao.Enumeracao(valor, nameof(Gateway));

  private static string ValidarIdentificadorExterno(string valor) =>
    Validacao.Texto(valor, nameof(IdentificadorExterno), 200);

  private static string ValidarTipo(string valor) =>
    Validacao.Texto(valor, nameof(Tipo), 150);

  private static string ValidarConteudo(string valor) =>
    Validacao.Json(valor, nameof(Conteudo));

  private static StatusEventoGateway ValidarStatus(StatusEventoGateway valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarRecebidoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(RecebidoEm));

  private static DateTimeOffset? ValidarProcessadoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ProcessadoEm));

  private static short ValidarTentativas(short valor) =>
    (short)Validacao.Numero(valor, nameof(Tentativas), 0m, 32767m, 0);

  private static string? ValidarUltimoErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(UltimoErro), 1000);
}

