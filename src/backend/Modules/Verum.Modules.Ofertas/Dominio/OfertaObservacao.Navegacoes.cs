namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class OfertaObservacao
{
  public Oferta Oferta { get; private set; } = null!;

  public FonteOferta Fonte { get; private set; } = null!;
}

