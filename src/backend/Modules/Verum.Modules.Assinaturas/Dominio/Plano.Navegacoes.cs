namespace Verum.Modules.Assinaturas.Dominio;

internal sealed partial class Plano
{
  private readonly List<Assinatura> _assinaturas = [];
  public IReadOnlyCollection<Assinatura> Assinaturas => _assinaturas.AsReadOnly();
}

