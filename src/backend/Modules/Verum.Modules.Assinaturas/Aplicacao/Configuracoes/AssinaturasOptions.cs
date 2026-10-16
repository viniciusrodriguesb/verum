namespace Verum.Modules.Assinaturas.Aplicacao.Configuracoes;

public sealed class AssinaturasOptions
{
  public bool Habilitado { get; set; }

  public string Gateway { get; set; } = "MercadoPago";

  public Dictionary<string, decimal> PrecosMensais { get; set; } = new();

  public int IntervaloProcessamentoSegundos { get; set; } = 5;

  public int IntervaloReconciliacaoSegundos { get; set; } = 300;
}
