using Verum.BuildingBlocks.Erros;

namespace Verum.Modules.Radar.Dominio;

internal sealed class Oportunidade
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


  public Monitoramento Monitoramento { get; private set; } = null!;

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

    if (!(PrecoObservado <= PrecoAlvo)) throw ErroAplicacaoException.Validacao("O preço observado deve atingir o preço-alvo.");

    if (!(ExpiraEm is null || ExpiraEm > DetectadaEm)) throw ErroAplicacaoException.Validacao("A expiração deve ser posterior à detecção.");
  }

  private static Guid ValidarId(Guid valor) =>
    Validacao.Identificador(valor, nameof(Id));

  private static Guid ValidarMonitoramentoId(Guid valor) =>
    Validacao.Identificador(valor, nameof(MonitoramentoId));

  private static Guid ValidarOfertaId(Guid valor) =>
    Validacao.Identificador(valor, nameof(OfertaId));

  private static long ValidarOfertaObservacaoId(long valor) =>
    (long)Validacao.Numero(valor, nameof(OfertaObservacaoId), 1m, long.MaxValue, 0);

  private static decimal ValidarPrecoObservado(decimal valor) =>
    Validacao.Numero(valor, nameof(PrecoObservado), 0.01m, 999999999999.99m, 2);

  private static decimal ValidarPrecoAlvo(decimal valor) =>
    Validacao.Numero(valor, nameof(PrecoAlvo), 0.01m, 999999999999.99m, 2);

  private static decimal? ValidarEconomiaDesdeCriacao(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(EconomiaDesdeCriacao), -999999999999.99m, 999999999999.99m, 2);

  private static decimal? ValidarPercentualReducao(decimal? valor) =>
    valor is null ? null : Validacao.Numero(valor.Value, nameof(PercentualReducao), -999m, 100m, 4);

  private static string ValidarOfertaSnapshot(string valor) =>
    Validacao.Json(valor, nameof(OfertaSnapshot));

  private static StatusOportunidade ValidarStatus(StatusOportunidade valor) =>
    Validacao.Enumeracao(valor, nameof(Status));

  private static DateTimeOffset ValidarDetectadaEm(DateTimeOffset valor) =>
    Validacao.Data(valor, nameof(DetectadaEm));

  private static DateTimeOffset? ValidarVisualizadaEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(VisualizadaEm));

  private static DateTimeOffset? ValidarExpiraEm(DateTimeOffset? valor) =>
    valor is null ? null : Validacao.Data(valor.Value, nameof(ExpiraEm));
}
