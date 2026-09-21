namespace Verum.Modules.Radar.Dominio;

internal sealed partial class Monitoramento
{
  public Guid Id { get; private set; }
  public Guid ContaId { get; private set; }
  public Guid ProdutoId { get; private set; }
  public Guid? ProdutoVarianteId { get; private set; }
  public string Nome { get; private set; } = null!;
  public decimal PrecoInicial { get; private set; }
  public decimal PrecoAlvo { get; private set; }
  public decimal? MenorPrecoAtual { get; private set; }
  public Guid? UltimaOfertaId { get; private set; }
  public long? UltimaObservacaoId { get; private set; }
  public StatusMonitoramento Status { get; private set; }
  public DateTimeOffset? UltimaVerificacaoEm { get; private set; }
  public DateTimeOffset? ProximaVerificacaoEm { get; private set; }
  public DateTimeOffset CriadoEm { get; private set; }
  public DateTimeOffset AtualizadoEm { get; private set; }
  public DateTimeOffset? PausadoEm { get; private set; }
  public DateTimeOffset? ExcluidoEm { get; private set; }
  public DateTimeOffset? UltimoAlertaEm { get; private set; }
  public decimal? UltimoPrecoAlertado { get; private set; }

  // Materialização pelo EF Core.
  private Monitoramento() { }

  public Monitoramento(
    Guid contaId,
    Guid produtoId,
    string nome,
    decimal precoInicial,
    decimal precoAlvo,
    Guid? produtoVarianteId = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    ContaId = ValidarContaId(contaId);
    ProdutoId = ValidarProdutoId(produtoId);
    ProdutoVarianteId = ValidarProdutoVarianteId(produtoVarianteId);
    Nome = ValidarNome(nome);
    PrecoInicial = ValidarPrecoInicial(precoInicial);
    PrecoAlvo = ValidarPrecoAlvo(precoAlvo);
    MenorPrecoAtual = ValidarMenorPrecoAtual(null);
    UltimaOfertaId = ValidarUltimaOfertaId(null);
    UltimaObservacaoId = ValidarUltimaObservacaoId(null);
    Status = ValidarStatus(StatusMonitoramento.Ativo);
    UltimaVerificacaoEm = ValidarUltimaVerificacaoEm(null);
    ProximaVerificacaoEm = ValidarProximaVerificacaoEm(null);
    CriadoEm = ValidarCriadoEm(agora);
    AtualizadoEm = ValidarAtualizadoEm(agora);
    PausadoEm = ValidarPausadoEm(null);
    ExcluidoEm = ValidarExcluidoEm(null);
    UltimoAlertaEm = ValidarUltimoAlertaEm(null);
    UltimoPrecoAlertado = ValidarUltimoPrecoAlertado(null);

  }
}

