namespace Verum.Modules.Assinaturas.Dominio;

internal sealed class Pagamento
{
  public Guid Id { get; private set; }

  public Guid AssinaturaId { get; private set; }

  public string IdentificadorExterno { get; private set; } = null!;

  public StatusPagamento Status { get; private set; }

  public decimal Valor { get; private set; }

  public string Moeda { get; private set; } = null!;

  public DateTimeOffset? PagoEm { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }

  public DateTimeOffset AtualizadoEm { get; private set; }

  public DateTimeOffset? PeriodoInicio { get; private set; }

  public DateTimeOffset? PeriodoFim { get; private set; }

  public DateTimeOffset? AtualizadoGatewayEm { get; private set; }


  public Assinatura Assinatura { get; private set; } = null!;

  private Pagamento() { }

  public Pagamento(
    Guid assinaturaId,
    string identificadorExterno,
    decimal valor,
    string moeda)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    AssinaturaId = ValidarAssinaturaId(assinaturaId);

    IdentificadorExterno = ValidarIdentificadorExterno(identificadorExterno);

    Status = ValidarStatus(StatusPagamento.Pendente);

    Valor = ValidarValor(valor);

    Moeda = ValidarMoeda(moeda);

    PagoEm = ValidarPagoEm(null);

    CriadoEm = ValidarCriadoEm(agora);

    AtualizadoEm = ValidarAtualizadoEm(agora);
  }

  public bool Sincronizar(string status, DateTimeOffset? pagoEm, DateTimeOffset inicio, DateTimeOffset atualizadoEm)
  {
    if (AtualizadoGatewayEm > atualizadoEm) return false;

    // Um reenvio antigo nunca reativa uma concessão estornada.
    if (Status == StatusPagamento.Estornado) return false;

    var periodoConfirmado = Status == StatusPagamento.Aprovado;

    Status = status switch
    {
      "approved" => StatusPagamento.Aprovado,
      "refunded" or "charged_back" or "in_mediation" => StatusPagamento.Estornado,
      "rejected" => StatusPagamento.Recusado,
      "cancelled" => StatusPagamento.Cancelado,
      _ => StatusPagamento.Pendente
    };

    PagoEm = ValidarPagoEm(pagoEm) ?? PagoEm;

    if (!periodoConfirmado)
    {
      PeriodoInicio = Validacao.Data(inicio, nameof(PeriodoInicio));

      PeriodoFim = inicio.AddMonths(1);
    }

    AtualizadoGatewayEm = Validacao.Data(atualizadoEm, nameof(AtualizadoGatewayEm));

    AtualizadoEm = DateTimeOffset.UtcNow;

    return true;
  }

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

  private static DateTimeOffset? ValidarPagoEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(PagoEm));

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}
