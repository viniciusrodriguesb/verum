namespace Verum.Modules.Contas.Dominio;

internal sealed class Conta
{
  public Guid Id { get; private set; }

  public string SujeitoIdentidade { get; private set; } = null!;

  public string Email { get; private set; } = null!;

  public string NomeExibicao { get; private set; } = null!;

  public StatusConta Status { get; private set; }

  public DateTimeOffset CriadaEm { get; private set; }

  public DateTimeOffset AtualizadaEm { get; private set; }

  public DateTimeOffset? ExcluidaEm { get; private set; }

  // Materialização pelo EF Core.
  private Conta() { }

  public Conta(
    string sujeitoIdentidade,
    string email,
    string nomeExibicao)
  {
    var agora = DateTimeOffset.UtcNow;

    Id = ValidarId(Guid.CreateVersion7());

    SujeitoIdentidade = ValidarSujeitoIdentidade(sujeitoIdentidade);

    Email = ValidarEmail(email);

    NomeExibicao = ValidarNomeExibicao(nomeExibicao);

    Status = ValidarStatus(StatusConta.Ativa);

    CriadaEm = ValidarCriadaEm(agora);

    AtualizadaEm = ValidarAtualizadaEm(agora);

    ExcluidaEm = ValidarExcluidaEm(null);
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static string ValidarSujeitoIdentidade(string valor) =>
    Validacao.Texto(valor, nameof(SujeitoIdentidade), 100);

  private static string ValidarEmail(string valor) =>
    Validacao.Email(valor, nameof(Email));

  private static string ValidarNomeExibicao(string valor) =>
    Validacao.Texto(valor, nameof(NomeExibicao), 150);

  private static StatusConta ValidarStatus(StatusConta valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarCriadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(CriadaEm));

  private static DateTimeOffset ValidarAtualizadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(AtualizadaEm));

  private static DateTimeOffset? ValidarExcluidaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ExcluidaEm));
}
