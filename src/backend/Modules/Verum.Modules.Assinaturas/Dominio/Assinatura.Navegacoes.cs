namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Assinatura
{
  public Plano Plano { get; private set; } = null!;

  public ClienteGateway ClienteGateway { get; private set; } = null!;

  private readonly List<Pagamento> _pagamentos = [];
  public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos.AsReadOnly();
}

