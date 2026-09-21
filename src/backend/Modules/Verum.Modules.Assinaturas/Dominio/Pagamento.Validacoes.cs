namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Pagamento
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarAssinaturaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(AssinaturaId));

  private static string ValidarIdentificadorExterno(string valor) =>
    Validacao.Texto(valor, nameof(IdentificadorExterno), 200);

  private static StatusPagamento ValidarStatus(StatusPagamento valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static decimal ValidarValor(decimal valor) =>
    Validacao.Numero(valor, nameof(Valor), 0.01m, 999999999999.99m, 2);

  private static string ValidarMoeda(string valor) =>
    Validacao.Moeda(valor, nameof(Moeda));

  private static MetodoPagamento? ValidarMetodo(MetodoPagamento? valor) =>
    valor is null ? null : Validacao.Enumeracao(valor.Value, nameof(Metodo));

  private static DateTimeOffset? ValidarVencimentoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(VencimentoEm));

  private static DateTimeOffset? ValidarPagoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(PagoEm));

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}

