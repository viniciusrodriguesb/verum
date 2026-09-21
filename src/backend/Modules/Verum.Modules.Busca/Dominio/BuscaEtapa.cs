namespace Verum.Modules.Busca.Dominio;

internal sealed partial class BuscaEtapa
{
  public long Id { get; private set; }
  public Guid BuscaId { get; private set; }
  public EtapaBusca Etapa { get; private set; }
  public StatusEtapa Status { get; private set; }
  public DateTimeOffset IniciadaEm { get; private set; }
  public DateTimeOffset? ConcluidaEm { get; private set; }
  public string? Detalhes { get; private set; }

  // Materialização pelo EF Core.
  private BuscaEtapa() { }

  public BuscaEtapa(
    Guid buscaId,
    EtapaBusca etapa,
    string? detalhes = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(0);
    BuscaId = ValidarBuscaId(buscaId);
    Etapa = ValidarEtapa(etapa);
    Status = ValidarStatus(StatusEtapa.EmAndamento);
    IniciadaEm = ValidarIniciadaEm(agora);
    ConcluidaEm = ValidarConcluidaEm(null);
    Detalhes = ValidarDetalhes(detalhes);

  }
}

