namespace Verum.Modules.Radar.Dominio;

internal sealed partial class Monitoramento
{
  private readonly List<Oportunidade> _oportunidades = [];
  public IReadOnlyCollection<Oportunidade> Oportunidades => _oportunidades.AsReadOnly();
}

