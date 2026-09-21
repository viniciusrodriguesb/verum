namespace Verum.Modules.Ofertas.Dominio;

internal sealed partial class Loja
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
}

