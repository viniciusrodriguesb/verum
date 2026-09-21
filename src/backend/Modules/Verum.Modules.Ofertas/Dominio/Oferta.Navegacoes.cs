namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class Oferta
{
  public Loja Loja { get; private set; } = null!;

  public FonteOferta Fonte { get; private set; } = null!;

  private readonly List<OfertaObservacao> _observacoes = [];
  public IReadOnlyCollection<OfertaObservacao> Observacoes => _observacoes.AsReadOnly();
}

