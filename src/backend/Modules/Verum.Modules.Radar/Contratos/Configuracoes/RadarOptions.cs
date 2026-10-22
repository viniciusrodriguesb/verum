namespace Verum.Modules.Radar.Contratos.Configuracoes;

public sealed class RadarOptions
{
  public bool Habilitado { get; set; }

  public int LimiteMonitoramentos { get; set; } = 10;

  public int IntervaloVerificacaoMinutos { get; set; } = 360;

  public int CooldownMinutos { get; set; } = 1440;

  public decimal HisteresePercentual { get; set; } = 2m;

  public int Lote { get; set; } = 20;
}
