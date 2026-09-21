namespace Verum.Modules.Busca.Dominio;

internal sealed partial class Busca
{
  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid? ValidarContaId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ContaId));

  private static Guid? ValidarVisitanteId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(VisitanteId));

  private static string ValidarChaveIdempotencia(string valor) =>
    Validacao.Texto(valor, nameof(ChaveIdempotencia), 100);

  private static string ValidarConsultaOriginal(string valor) =>
    Validacao.Texto(valor, nameof(ConsultaOriginal), 500);

  private static string ValidarConsultaNormalizada(string valor) =>
    Validacao.Texto(valor, nameof(ConsultaNormalizada), 500);

  private static string? ValidarConsultaEstruturada(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(ConsultaEstruturada));

  private static Guid? ValidarProdutoPrincipalId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoPrincipalId));

  private static Guid? ValidarProdutoVariantePrincipalId(Guid? valor) =>
    valor is null ? null : Validacao.Identificador(valor.Value, nameof(ProdutoVariantePrincipalId));

  private static StatusBusca ValidarStatus(StatusBusca valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static EtapaBusca ValidarEtapaAtual(EtapaBusca valor) =>
    Validacao.Enumeracao(valor, nameof(EtapaAtual));

  private static MotivoBuscaSemResultado? ValidarMotivoSemResultado(MotivoBuscaSemResultado? valor) =>
    valor is null ? null : Validacao.Enumeracao(valor.Value, nameof(MotivoSemResultado));

  private static bool ValidarResultadoParcial(bool valor) =>
    valor;

  private static string? ValidarCodigoErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(CodigoErro), 100);

  private static string? ValidarMensagemErro(string? valor) =>
    valor is null ? null : Validacao.Texto(valor, nameof(MensagemErro), 500);

  private static DateTimeOffset ValidarIniciadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(IniciadaEm));

  private static DateTimeOffset? ValidarConcluidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ConcluidaEm));

  private static DateTimeOffset ValidarExpiraEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(ExpiraEm));

  private static int ValidarQuantidadeFontesConsultadas(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeFontesConsultadas), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeFontesComSucesso(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeFontesComSucesso), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeFontesComFalha(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeFontesComFalha), 0m, 2147483647m, 0);
}

