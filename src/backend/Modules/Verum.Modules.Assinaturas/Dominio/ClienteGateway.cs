namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class ClienteGateway
{
  public Guid Id { get; private set; }
  public Guid ContaId { get; private set; }
  public GatewayPagamento Gateway { get; private set; }
  public string IdentificadorExterno { get; private set; } = null!;
  public DateTimeOffset CriadoEm { get; private set; }
  public DateTimeOffset AtualizadoEm { get; private set; }

  // Materialização pelo EF Core.
  private ClienteGateway() { }

  public ClienteGateway(
    Guid contaId,
    GatewayPagamento gateway,
    string identificadorExterno)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ContaId = ValidarContaId(contaId);
    Gateway = ValidarGateway(gateway);
    IdentificadorExterno = ValidarIdentificadorExterno(identificadorExterno);
    CriadoEm = ValidarCriadoEm(agora);
    AtualizadoEm = ValidarAtualizadoEm(agora);

  }
}

