namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Pagamento
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
}

