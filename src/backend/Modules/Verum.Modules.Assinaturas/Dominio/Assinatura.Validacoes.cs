namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Assinatura
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static Guid ValidarPlanoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(PlanoId));

  private static Guid ValidarClienteGatewayId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ClienteGatewayId));

  private static GatewayPagamento ValidarGateway(GatewayPagamento valor) =>
    Validacao.Enumeracao(valor, nameof(Gateway));

  private static string ValidarIdentificadorExterno(string valor) =>
    Validacao.Texto(valor, nameof(IdentificadorExterno), 200);

  private static StatusAssinatura ValidarStatus(StatusAssinatura valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static decimal ValidarValorContratado(decimal valor) =>
    Validacao.Numero(valor, nameof(ValorContratado), 0.01m, 999999999999.99m, 2);

  private static string ValidarMoeda(string valor) =>
    Validacao.Moeda(valor, nameof(Moeda));

  private static DateTimeOffset? ValidarPeriodoIniciadoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(PeriodoIniciadoEm));

  private static DateTimeOffset? ValidarPeriodoTerminaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(PeriodoTerminaEm));

  private static DateTimeOffset? ValidarCancelamentoSolicitadoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(CancelamentoSolicitadoEm));

  private static DateTimeOffset? ValidarCanceladaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(CanceladaEm));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}

