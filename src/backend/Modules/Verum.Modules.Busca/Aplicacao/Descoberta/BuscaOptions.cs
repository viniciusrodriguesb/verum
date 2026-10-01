namespace Verum.Modules.Busca.Aplicacao.Descoberta;

public sealed class BuscaOptions
{
  public string FilaSolicitacoes { get; set; } = Contratos.BuscaSolicitada.FilaPadrao;

  public string[] Lojas { get; set; } = ["kabum"];

  public int MinimoOfertas { get; set; } = 3;

  public int MaximoResultados { get; set; } = 10;

  public int PrazoSegundos { get; set; } = 180;

  public int TimeoutFonteSegundos { get; set; } = 45;

  public int TimeoutIaSegundos { get; set; } = 20;

  public int LimiteVisitanteDiario { get; set; } = 1;
}
