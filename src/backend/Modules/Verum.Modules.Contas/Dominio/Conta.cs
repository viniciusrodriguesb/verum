namespace Verum.Modules.Contas.Dominio;

internal sealed partial class Conta
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
}

