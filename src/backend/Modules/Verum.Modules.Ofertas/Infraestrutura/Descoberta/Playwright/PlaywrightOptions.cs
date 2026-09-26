namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright;

public sealed class PlaywrightOptions
{
  public bool Enabled { get; set; }

  public int ConsultasSimultaneas { get; set; } = 2;

  public int TimeoutTotalSegundos { get; set; } = 60;

  public int TimeoutAcaoSegundos { get; set; } = 10;

  public Dictionary<string, LojaPlaywrightOptions> Lojas { get; set; } = new();
}

public sealed class LojaPlaywrightOptions
{
  public string UrlBusca { get; set; } = string.Empty;

  public string[] HostsPermitidos { get; set; } = [];

  public string? CampoBusca { get; set; }

  public string? BotaoBusca { get; set; }

  public string? BotaoPreparacao { get; set; }

  public string ResultadosProntos { get; set; } = string.Empty;

  public string Itens { get; set; } = string.Empty;

  public string? SemResultados { get; set; }

  public string LinkProduto { get; set; } = string.Empty;

  public string? ProximaPagina { get; set; }

  public int MaxPaginas { get; set; } = 3;

  public int MaxItens { get; set; } = 50;

  public Dictionary<string, CampoLojaOptions> Campos { get; set; } = new();
}

public sealed class CampoLojaOptions
{
  public string Seletor { get; set; } = string.Empty;

  public string? Atributo { get; set; }

  public bool Obrigatorio { get; set; }
}

