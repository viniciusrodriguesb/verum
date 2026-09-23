namespace Verum.Modules.Ofertas.Dominio;

internal sealed class Loja
{
  public Guid Id { get; private set; }

  public string Nome { get; private set; } = null!;

  public string NomeNormalizado { get; private set; } = null!;

  public string? Dominio { get; private set; }

  public string? Url { get; private set; }

  public bool Verificada { get; private set; }

  public decimal PontuacaoConfianca { get; private set; }

  public StatusLoja Status { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }

  private readonly List<Oferta> _ofertas = [];

  public IReadOnlyCollection<Oferta> Ofertas => _ofertas.AsReadOnly();

  // Materialização pelo EF Core.
  private Loja() { }

  public Loja(
    string nome,
    decimal pontuacaoConfianca,
    string? dominio = null,
    string? url = null)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    Nome = ValidarNome(nome);

    NomeNormalizado = ValidarNomeNormalizado(Nome.ToLowerInvariant());

    Dominio = ValidarDominio(dominio);

    Url = ValidarUrl(url);

    Verificada = ValidarVerificada(false);

    PontuacaoConfianca = ValidarPontuacaoConfianca(pontuacaoConfianca);

    Status = ValidarStatus(StatusLoja.Ativa);

    CriadaEm = ValidarCriadaEm(agora);

    AtualizadaEm = ValidarAtualizadaEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 180);

  private static string ValidarNomeNormalizado(string valor) =>
    Validacao.Texto(valor, nameof(NomeNormalizado), 180);

  private static string? ValidarDominio(string? valor) =>
    valor is null ? null : Validacao.Dominio(valor, nameof(Dominio));

  private static string? ValidarUrl(string? valor) =>
    valor is null ? null : Validacao.Url(valor, nameof(Url));

  private static bool ValidarVerificada(bool valor) =>
    valor;

  private static decimal ValidarPontuacaoConfianca(decimal valor) =>
    Validacao.Numero(valor, nameof(PontuacaoConfianca), 0m, 100m, 2);

  private static StatusLoja ValidarStatus(StatusLoja valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));
}
