namespace Verum.Modules.Assinaturas.Dominio;

internal sealed class Pagamento
{
  public Guid Id { get; private set; }

  public Guid AssinaturaId { get; private set; }

  public string IdentificadorExterno { get; private set; } = null!;

  public StatusPagamento Status { get; private set; }

  public decimal Valor { get; private set; }

  public string Moeda { get; private set; } = null!;

  public MetodoPagamento? Metodo { get; private set; }

  public DateTimeOffset? VencimentoEm { get; private set; }

  public DateTimeOffset? PagoEm { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }

  public DateTimeOffset AtualizadoEm { get; private set; }


  public Assinatura Assinatura { get; private set; } = null!;

  // Materialização pelo EF Core.
  private Pagamento() { }

  public Pagamento(
    Guid assinaturaId,
    string identificadorExterno,
    decimal valor,
    string moeda,
    MetodoPagamento? metodo = null,
    DateTimeOffset? vencimentoEm = null)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    AssinaturaId = ValidarAssinaturaId(assinaturaId);

    IdentificadorExterno = ValidarIdentificadorExterno(identificadorExterno);

    Status = ValidarStatus(StatusPagamento.Pendente);

    Valor = ValidarValor(valor);

    Moeda = ValidarMoeda(moeda);

    Metodo = ValidarMetodo(metodo);

    VencimentoEm = ValidarVencimentoEm(vencimentoEm);

    PagoEm = ValidarPagoEm(null);

    CriadoEm = ValidarCriadoEm(agora);

    AtualizadoEm = ValidarAtualizadoEm(agora);
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
