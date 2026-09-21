namespace Verum.Modules.Radar.Dominio;

internal sealed partial class Oportunidade
{
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

