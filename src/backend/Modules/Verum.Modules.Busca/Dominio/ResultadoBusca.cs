namespace Verum.Modules.Busca.Dominio;

internal sealed class ResultadoBusca
{
  public Guid Id { get; private set; }

  public Guid BuscaId { get; private set; }

  public string VersaoRanking { get; private set; } = null!;

  public int QuantidadeAnalisada { get; private set; }

  public int QuantidadeExibida { get; private set; }

  public bool Parcial { get; private set; }

  public DateTimeOffset GeradoEm { get; private set; }


  public Busca Busca { get; private set; } = null!;

  private readonly List<ResultadoOferta> _ofertas = [];

  public IReadOnlyCollection<ResultadoOferta> Ofertas => _ofertas.AsReadOnly();

  // Materialização pelo EF Core.
  private ResultadoBusca() { }

  public ResultadoBusca(
    Guid buscaId,
    string versaoRanking,
    int quantidadeAnalisada,
    int quantidadeExibida,
    bool parcial)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    BuscaId = ValidarBuscaId(buscaId);

    VersaoRanking = ValidarVersaoRanking(versaoRanking);

    QuantidadeAnalisada = ValidarQuantidadeAnalisada(quantidadeAnalisada);

    QuantidadeExibida = ValidarQuantidadeExibida(quantidadeExibida);

    Parcial = ValidarParcial(parcial);

    GeradoEm = ValidarGeradoEm(agora);

    if (!(QuantidadeExibida <= QuantidadeAnalisada)) throw new ArgumentException("QuantidadeExibida não pode exceder QuantidadeAnalisada.");
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarBuscaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(BuscaId));

  private static string ValidarVersaoRanking(string valor) =>
    Validacao.Texto(valor, nameof(VersaoRanking), 50);

  private static int ValidarQuantidadeAnalisada(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeAnalisada), 0m, 2147483647m, 0);

  private static int ValidarQuantidadeExibida(int valor) =>
    (int)Validacao.Numero(valor, nameof(QuantidadeExibida), 0m, 2147483647m, 0);

  private static bool ValidarParcial(bool valor) =>
    valor;

  private static DateTimeOffset ValidarGeradoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(GeradoEm));
}
