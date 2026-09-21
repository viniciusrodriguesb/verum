namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class FonteOferta
{
  private readonly List<Oferta> _ofertas = [];
  public IReadOnlyCollection<Oferta> Ofertas => _ofertas.AsReadOnly();

  private readonly List<OfertaObservacao> _observacoes = [];
  public IReadOnlyCollection<OfertaObservacao> Observacoes => _observacoes.AsReadOnly();

  private readonly List<ExecucaoConsultaFonte> _execucoes = [];
  public IReadOnlyCollection<ExecucaoConsultaFonte> Execucoes => _execucoes.AsReadOnly();
}

