namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

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

  public string? Bloqueio { get; set; }

  public string? CaminhoProdutoPadrao { get; set; }

  public string LinkProduto { get; set; } = string.Empty;

  public string? ProximaPagina { get; set; }

  public bool PaginacaoPorClique { get; set; }

  public int MaxPaginas { get; set; } = 3;

  public int MaxItens { get; set; } = 50;

  public Dictionary<string, CampoLojaOptions> Campos { get; set; } = new();
}
