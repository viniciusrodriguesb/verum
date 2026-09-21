namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class ClienteGateway
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarContaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(ContaId));

  private static GatewayPagamento ValidarGateway(GatewayPagamento valor) =>
    Validacao.Enumeracao(valor, nameof(Gateway));

  private static string ValidarIdentificadorExterno(string valor) =>
    Validacao.Texto(valor, nameof(IdentificadorExterno), 200);

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}

