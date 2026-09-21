namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class ClienteGateway
{
  private readonly List<Assinatura> _assinaturas = [];
  public IReadOnlyCollection<Assinatura> Assinaturas => _assinaturas.AsReadOnly();
}

