namespace Verum.Modules.Busca.Dominio;

internal sealed class BuscaEtapa
{
  public long Id { get; private set; }

  public Guid BuscaId { get; private set; }

  public EtapaBusca Etapa { get; private set; }

  public StatusEtapa Status { get; private set; }

  public DateTimeOffset IniciadaEm { get; private set; }

  public DateTimeOffset? ConcluidaEm { get; private set; }

  public string? Detalhes { get; private set; }


  public Busca Busca { get; private set; } = null!;

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

  private static long ValidarId(long valor) =>
    (long)Validacao.Numero(valor, nameof(Id), 0m, long.MaxValue, 0);

  private static Guid ValidarBuscaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(BuscaId));

  private static EtapaBusca ValidarEtapa(EtapaBusca valor) =>
    Validacao.Enumeracao(valor, nameof(Etapa));

  private static StatusEtapa ValidarStatus(StatusEtapa valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarIniciadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(IniciadaEm));

  private static DateTimeOffset? ValidarConcluidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ConcluidaEm));

  private static string? ValidarDetalhes(string? valor) =>
    valor is null ? null : Validacao.Json(valor, nameof(Detalhes));
}
