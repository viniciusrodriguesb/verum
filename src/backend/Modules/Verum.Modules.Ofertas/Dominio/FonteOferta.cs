namespace Verum.Modules.Ofertas.Dominio;

internal sealed class FonteOferta
{
  public Guid Id { get; private set; }

  public string Nome { get; private set; } = null!;

  public TipoFonte Tipo { get; private set; }

  public string Codigo { get; private set; } = null!;

  public bool Ativa { get; private set; }

  public decimal NivelConfianca { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  private readonly List<Oferta> _ofertas = [];

  public IReadOnlyCollection<Oferta> Ofertas => _ofertas.AsReadOnly();

  private readonly List<OfertaObservacao> _observacoes = [];

  public IReadOnlyCollection<OfertaObservacao> Observacoes => _observacoes.AsReadOnly();

  private readonly List<ExecucaoConsultaFonte> _execucoes = [];

  public IReadOnlyCollection<ExecucaoConsultaFonte> Execucoes => _execucoes.AsReadOnly();

  // Materialização pelo EF Core.
  private FonteOferta() { }

  public FonteOferta(
    string nome,
    TipoFonte tipo,
    string codigo,
    decimal nivelConfianca)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    Nome = ValidarNome(nome);

    Tipo = ValidarTipo(tipo);

    Codigo = ValidarCodigo(codigo);

    Ativa = ValidarAtiva(true);

    NivelConfianca = ValidarNivelConfianca(nivelConfianca);

    CriadaEm = ValidarCriadaEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static TipoFonte ValidarTipo(TipoFonte valor) =>
    Validacao.Enumeracao(valor, nameof(Tipo));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 80);

  private static bool ValidarAtiva(bool valor) =>
    valor;

  private static decimal ValidarNivelConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(NivelConfianca), 0m, 100m, 2);

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));
}
