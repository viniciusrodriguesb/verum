namespace Verum.Modules.Radar.Dominio;

internal sealed partial class Oportunidade
{
  public Guid Id { get; private set; }
  public Guid MonitoramentoId { get; private set; }
  public Guid OfertaId { get; private set; }
  public long OfertaObservacaoId { get; private set; }
  public decimal PrecoObservado { get; private set; }
  public decimal PrecoAlvo { get; private set; }
  public decimal? EconomiaDesdeCriacao { get; private set; }
  public decimal? PercentualReducao { get; private set; }
  public string OfertaSnapshot { get; private set; } = null!;
  public StatusOportunidade Status { get; private set; }
  public DateTimeOffset DetectadaEm { get; private set; }
  public DateTimeOffset? VisualizadaEm { get; private set; }
  public DateTimeOffset? ExpiraEm { get; private set; }

  // Materialização pelo EF Core.
  private Oportunidade() { }

  public Oportunidade(
    Guid monitoramentoId,
    Guid ofertaId,
    long ofertaObservacaoId,
    decimal precoObservado,
    decimal precoAlvo,
    string ofertaSnapshot,
    decimal? economiaDesdeCriacao = null,
    decimal? percentualReducao = null,
    DateTimeOffset? expiraEm = null)
  {
    var agora = DateTimeOffset.UtcNow;
    Id = ValidarId(Guid.CreateVersion7());
    MonitoramentoId = ValidarMonitoramentoId(monitoramentoId);
    OfertaId = ValidarOfertaId(ofertaId);
    OfertaObservacaoId = ValidarOfertaObservacaoId(ofertaObservacaoId);
    PrecoObservado = ValidarPrecoObservado(precoObservado);
    PrecoAlvo = ValidarPrecoAlvo(precoAlvo);
    EconomiaDesdeCriacao = ValidarEconomiaDesdeCriacao(economiaDesdeCriacao);
    PercentualReducao = ValidarPercentualReducao(percentualReducao);
    OfertaSnapshot = ValidarOfertaSnapshot(ofertaSnapshot);
    Status = ValidarStatus(StatusOportunidade.Detectada);
    DetectadaEm = ValidarDetectadaEm(agora);
    VisualizadaEm = ValidarVisualizadaEm(null);
    ExpiraEm = ValidarExpiraEm(expiraEm);
    if (!(PrecoObservado <= PrecoAlvo)) throw new ArgumentException("O preço observado deve atingir o preço-alvo.");
    if (!(ExpiraEm is null || ExpiraEm > DetectadaEm)) throw new ArgumentException("A expiração deve ser posterior à detecção.");
  }
}

