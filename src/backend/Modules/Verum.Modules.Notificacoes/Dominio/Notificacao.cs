namespace Verum.Modules.Notificacoes.Dominio;

internal sealed partial class Notificacao
{
  public Guid Id { get; private set; }
  public Guid ContaId { get; private set; }
  public TipoNotificacao Tipo { get; private set; }
  public string Titulo { get; private set; } = null!;
  public string Mensagem { get; private set; } = null!;
  public string? Dados { get; private set; }
  public StatusNotificacao Status { get; private set; }
  public DateTimeOffset CriadaEm { get; private set; }
  public DateTimeOffset? LidaEm { get; private set; }

  // Materialização pelo EF Core.
  private Notificacao() { }

  public Notificacao(
    Guid contaId,
    TipoNotificacao tipo,
    string titulo,
    string mensagem,
    string? dados = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ContaId = ValidarContaId(contaId);
    Tipo = ValidarTipo(tipo);
    Titulo = ValidarTitulo(titulo);
    Mensagem = ValidarMensagem(mensagem);
    Dados = ValidarDados(dados);
    Status = ValidarStatus(StatusNotificacao.NaoLida);
    CriadaEm = ValidarCriadaEm(agora);
    LidaEm = ValidarLidaEm(null);

  }
}

