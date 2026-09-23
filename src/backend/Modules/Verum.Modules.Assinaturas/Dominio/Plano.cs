namespace Verum.Modules.Assinaturas.Dominio;

internal sealed class Plano
{
  public Guid Id { get; private set; }

  public string Codigo { get; private set; } = null!;

  public string Nome { get; private set; } = null!;

  public string Descricao { get; private set; } = null!;

  public string PacoteAcessoCodigo { get; private set; } = null!;

  public decimal ValorAtual { get; private set; }

  public string Moeda { get; private set; } = null!;

  public PeriodicidadePlano Periodicidade { get; private set; }

  public bool Ativo { get; private set; }

  public DateTimeOffset CriadoEm { get; private set; }

  public DateTimeOffset AtualizadoEm { get; private set; }

  private readonly List<Assinatura> _assinaturas = [];

  public IReadOnlyCollection<Assinatura> Assinaturas => _assinaturas.AsReadOnly();

  // Materialização pelo EF Core.
  private Plano() { }

  public Plano(
    string codigo,
    string nome,
    string descricao,
    string pacoteAcessoCodigo,
    decimal valorAtual,
    string moeda,
    PeriodicidadePlano periodicidade)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    Codigo = ValidarCodigo(codigo);

    Nome = ValidarNome(nome);

    Descricao = ValidarDescricao(descricao);

    PacoteAcessoCodigo = ValidarPacoteAcessoCodigo(pacoteAcessoCodigo);

    ValorAtual = ValidarValorAtual(valorAtual);

    Moeda = ValidarMoeda(moeda);

    Periodicidade = ValidarPeriodicidade(periodicidade);

    Ativo = ValidarAtivo(true);

    CriadoEm = ValidarCriadoEm(agora);

    AtualizadoEm = ValidarAtualizadoEm(agora);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarCodigo(string valor) =>
    Validacao.Texto(valor, nameof(Codigo), 80);

  private static string ValidarNome(string valor) =>
    Validacao.Texto(valor, nameof(Nome), 120);

  private static string ValidarDescricao(string valor) =>
    Validacao.Texto(valor, nameof(Descricao), 500);

  private static string ValidarPacoteAcessoCodigo(string valor) =>
    Validacao.Texto(valor, nameof(PacoteAcessoCodigo), 80);

  private static decimal ValidarValorAtual(decimal valor) =>
    Validacao.Numero(valor, nameof(ValorAtual), 0.01m, 999999999999.99m, 2);

  private static string ValidarMoeda(string valor) =>
    Validacao.Moeda(valor, nameof(Moeda));

  private static PeriodicidadePlano ValidarPeriodicidade(PeriodicidadePlano valor) =>
    Validacao.Enumeracao(valor, nameof(Periodicidade));

  private static bool ValidarAtivo(bool valor) =>
    valor;

  private static DateTimeOffset ValidarCriadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadoEm));

  private static DateTimeOffset ValidarAtualizadoEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadoEm));
}
