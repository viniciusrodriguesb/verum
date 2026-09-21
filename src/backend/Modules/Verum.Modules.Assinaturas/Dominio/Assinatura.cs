namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Assinatura
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
}

