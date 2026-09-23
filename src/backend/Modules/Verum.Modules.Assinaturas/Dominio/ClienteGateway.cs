namespace Verum.Modules.Assinaturas.Dominio;

internal sealed class ClienteGateway
{
  public Guid Id { get; private set; }

  public Guid ContaId { get; private set; }

  public GatewayPagamento Gateway { get; private set; }

  public string IdentificadorExterno { get; private set; } = null!;

  public DateTimeOffset CriadoEm { get; private set; }

  public DateTimeOffset AtualizadoEm { get; private set; }

  private readonly List<Assinatura> _assinaturas = [];

  public IReadOnlyCollection<Assinatura> Assinaturas => _assinaturas.AsReadOnly();

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
