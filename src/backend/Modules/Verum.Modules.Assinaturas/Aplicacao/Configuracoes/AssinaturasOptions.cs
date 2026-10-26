namespace Verum.Modules.Assinaturas.Aplicacao.Configuracoes;

public sealed class AssinaturasOptions
{
  public bool Habilitado { get; set; }

  public string Gateway { get; set; } = "MercadoPago";

  public Dictionary<string, decimal> PrecosMensais { get; set; } = new();

  public int IntervaloProcessamentoSegundos { get; set; } = 5;

  public int IntervaloReconciliacaoSegundos { get; set; } = 300;

  public int TimeoutProcessamentoSegundos { get; set; } = 60;

  public int EventosPorCiclo { get; set; } = 20;

  public int LoteReconciliacao { get; set; } = 50;

  public int JanelaReconciliacaoDias { get; set; } = 90;
}
