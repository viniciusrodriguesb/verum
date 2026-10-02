namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

public sealed class PlaywrightOptions
{
  public bool Enabled { get; set; }

  public int ConsultasSimultaneas { get; set; } = 2;

  public int TimeoutTotalSegundos { get; set; } = 60;

  public int TimeoutAcaoSegundos { get; set; } = 10;

  public Dictionary<string, LojaPlaywrightOptions> Lojas { get; set; } = new();
}
