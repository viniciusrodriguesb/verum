namespace Verum.Modules.Assinaturas.Dominio;

internal sealed class Assinatura
{
  public Guid Id { get; private set; }

  public Guid ContaId { get; private set; }

  public Guid PlanoId { get; private set; }

  public Guid ClienteGatewayId { get; private set; }

  public GatewayPagamento Gateway { get; private set; }

  public string IdentificadorExterno { get; private set; } = null!;

  public StatusAssinatura Status { get; private set; }

  public decimal ValorContratado { get; private set; }

  public string Moeda { get; private set; } = null!;

  public DateTimeOffset? PeriodoIniciadoEm { get; private set; }

  public DateTimeOffset? PeriodoTerminaEm { get; private set; }

  public DateTimeOffset? CancelamentoSolicitadoEm { get; private set; }

  public DateTimeOffset? CanceladaEm { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }


  public Plano Plano { get; private set; } = null!;

  public ClienteGateway ClienteGateway { get; private set; } = null!;

  private readonly List<Pagamento> _pagamentos = [];

  public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos.AsReadOnly();

  // Materialização pelo EF Core.
  private Assinatura() { }

  public Assinatura(
    Guid contaId,
    Guid planoId,
    Guid clienteGatewayId,
    GatewayPagamento gateway,
    string identificadorExterno,
    decimal valorContratado,
    string moeda)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    ContaId = ValidarContaId(contaId);

    PlanoId = ValidarPlanoId(planoId);

    ClienteGatewayId = ValidarClienteGatewayId(clienteGatewayId);

    Gateway = ValidarGateway(gateway);

    IdentificadorExterno = ValidarIdentificadorExterno(identificadorExterno);

    Status = ValidarStatus(StatusAssinatura.Pendente);

    ValorContratado = ValidarValorContratado(valorContratado);

    Moeda = ValidarMoeda(moeda);

    PeriodoIniciadoEm = ValidarPeriodoIniciadoEm(null);

    PeriodoTerminaEm = ValidarPeriodoTerminaEm(null);

    CancelamentoSolicitadoEm = ValidarCancelamentoSolicitadoEm(null);

    CanceladaEm = ValidarCanceladaEm(null);

    CriadaEm = ValidarCriadaEm(agora);

    AtualizadaEm = ValidarAtualizadaEm(agora);
  }

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
