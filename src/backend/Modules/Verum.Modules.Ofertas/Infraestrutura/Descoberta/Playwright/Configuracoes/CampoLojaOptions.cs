namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

public sealed class CampoLojaOptions
{
  public string Seletor { get; set; } = string.Empty;

  public string? Atributo { get; set; }

  public bool Obrigatorio { get; set; }
}
