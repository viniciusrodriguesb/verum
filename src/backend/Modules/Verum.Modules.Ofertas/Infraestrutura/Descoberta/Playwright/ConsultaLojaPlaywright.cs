using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Ofertas.Contratos.Descoberta;
using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;
using Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright.Configuracoes;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright;

internal sealed class ConsultaLojaPlaywright(NavegadorPlaywright navegador, IOptions<PlaywrightOptions> options, ILogger<ConsultaLojaPlaywright> logger) : IConsultaLoja
{
  public bool Disponivel => options.Value.Enabled;

  public Task<ResultadoConsultaLoja> ConsultarAsync(string loja, string consulta, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(consulta) || consulta.Length > 500)
      throw ErroAplicacaoException.Validacao("Informe uma consulta com até 500 caracteres.", nameof(consulta));

    if (!options.Value.Enabled)
      throw new InvalidOperationException("Habilite Playwright:Enabled para consultar lojas.");

    if (string.IsNullOrWhiteSpace(loja) || !options.Value.Lojas.TryGetValue(loja, out var config))
      throw new ArgumentException("Loja não configurada.", nameof(loja));

    var url = new Uri(config.UrlBusca.Replace("{consulta}", Uri.EscapeDataString(consulta), StringComparison.Ordinal));

    return navegador.ExecutarAsync((context, token) => ConsultarPaginaAsync(context, config, loja, consulta, url, token), cancellationToken);
  }

  private async Task<ResultadoConsultaLoja> ConsultarPaginaAsync(
    IBrowserContext context, LojaPlaywrightOptions config, string loja, string consulta, Uri url, CancellationToken cancellationToken)
  {
    await context.RouteAsync("**/*", async route =>
    {
      if (!Permitida(route.Request.Url, config)) await route.AbortAsync();
      else await route.ContinueAsync();
    });

    await context.RouteWebSocketAsync("**/*", socket => socket.CloseAsync());

    var page = await context.NewPageAsync();

    var itens = new List<ItemConsultaLoja>();

    var descartados = new List<ItemLojaDescartado>();

    var urls = new HashSet<string>(StringComparer.Ordinal);

    var paginas = new HashSet<string>(StringComparer.Ordinal);

    var limiteAtingido = false;

    var navegacaoPorClique = false;

    for (var numero = 1; numero <= config.MaxPaginas; numero++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!Permitida(url.AbsoluteUri, config))
        throw new InvalidOperationException("A navegação saiu dos hosts configurados para a loja.");

      if (!navegacaoPorClique && !paginas.Add(url.AbsoluteUri)) break;

      await PrepararPaginaAsync(page, config, consulta, url, numero == 1, !navegacaoPorClique);

      var quantidade = await AguardarItensAsync(page, config, cancellationToken);

      var cards = page.Locator(config.Itens);

      limiteAtingido |= quantidade > config.MaxItens;

      if (quantidade == 0) break;

      for (var indice = 0; indice < Math.Min(quantidade, config.MaxItens); indice++)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var item = await ExtrairItemAsync(cards.Nth(indice), page.Url, config);

        if (item is null)
        {
          descartados.Add(new(numero, indice, "CAMPOS_INVALIDOS"));

          logger.LogWarning("Item descartado na loja {Loja}, página {Pagina}, índice {Indice}.", loja, numero, indice);

          continue;
        }

        if (urls.Add(item.Url)) itens.Add(item);

        if (itens.Count >= config.MaxItens)
          return new(loja, DateTimeOffset.UtcNow, itens, descartados, true);
      }

      if (config.ProximaPagina is null) break;

      var proxima = page.Locator(config.ProximaPagina);

      if (await proxima.CountAsync() == 0) break;

      if (numero == config.MaxPaginas)
        return new(loja, DateTimeOffset.UtcNow, itens, descartados, true);

      if (config.PaginacaoPorClique)
      {
        var primeiroLink = string.IsNullOrWhiteSpace(config.LinkProduto) ? cards.First : cards.First.Locator(config.LinkProduto);

        var hrefAnterior = await primeiroLink.GetAttributeAsync("href")
          ?? throw new InvalidOperationException("Não foi possível identificar a página atual.");

        await proxima.ClickAsync();

        // Aguarda troca real dos resultados em páginas SPA, não uma pausa arbitrária.
        await Microsoft.Playwright.Assertions.Expect(primeiroLink).Not.ToHaveAttributeAsync("href", hrefAnterior,
          new() { Timeout = options.Value.TimeoutAcaoSegundos * 1000 });

        navegacaoPorClique = true;

        url = new Uri(page.Url);
      }
      else
      {
        var proximaUrl = ResolverLink(page.Url, await proxima.GetAttributeAsync("href"), config)
          ?? throw new InvalidOperationException("Link de paginação inválido.");

        url = new Uri(proximaUrl);
      }
    }

    return new(loja, DateTimeOffset.UtcNow, itens, descartados, limiteAtingido);
  }

  private static async Task PrepararPaginaAsync(
    IPage page, LojaPlaywrightOptions config, string consulta, Uri url, bool prepararBusca, bool navegar)
  {
    if (navegar)
    {
      var response = await page.GotoAsync(url.AbsoluteUri, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

      if (response is null || !response.Ok)
        throw new HttpRequestException("A loja retornou uma resposta HTTP inválida.", null, response is null ? null : (System.Net.HttpStatusCode)response.Status);
    }

    if (!Permitida(page.Url, config))
      throw new InvalidOperationException("O redirecionamento saiu dos hosts configurados.");

    if (prepararBusca)
    {
      if (config.BotaoPreparacao is not null && await page.Locator(config.BotaoPreparacao).IsVisibleAsync())
        await page.Locator(config.BotaoPreparacao).ClickAsync();

      if (config.CampoBusca is not null)
      {
        await page.Locator(config.CampoBusca).FillAsync(consulta);

        if (config.BotaoBusca is null) await page.Locator(config.CampoBusca).PressAsync("Enter");
        else await page.Locator(config.BotaoBusca).ClickAsync();
      }
    }
  }

  private async Task<int> AguardarItensAsync(IPage page, LojaPlaywrightOptions config, CancellationToken cancellationToken)
  {
    var marcadores = config.Bloqueio is null ? config.ResultadosProntos : config.ResultadosProntos + ", " + config.Bloqueio;

    await page.Locator(marcadores).First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

    if (config.Bloqueio is not null && await page.Locator(config.Bloqueio).First.IsVisibleAsync())
      throw new HttpRequestException("A loja bloqueou a consulta automatizada.", null, System.Net.HttpStatusCode.Forbidden);

    var cards = page.Locator(config.Itens);

    var espera = System.Diagnostics.Stopwatch.StartNew();

    var quantidade = await cards.CountAsync();

    // A hidratação de páginas SPA pode substituir os cards logo após o marcador ficar visível.
    // Aguarda novamente dentro do prazo da ação, sem interpretar a troca temporária como lista vazia.
    while (quantidade == 0 && (config.SemResultados is null || !await page.Locator(config.SemResultados).First.IsVisibleAsync()))
    {
      if (espera.Elapsed >= TimeSpan.FromSeconds(options.Value.TimeoutAcaoSegundos))
        throw new InvalidOperationException("Layout da loja não corresponde aos seletores configurados.");

      await Task.Delay(50, cancellationToken);

      quantidade = await cards.CountAsync();
    }

    return quantidade;
  }

  private static async Task<ItemConsultaLoja?> ExtrairItemAsync(ILocator card, string pagina, LojaPlaywrightOptions config)
  {
    var campos = new Dictionary<string, string?>();

    var valido = true;

    foreach (var (nome, campo) in config.Campos)
    {
      var locator = string.IsNullOrWhiteSpace(campo.Seletor) ? card : card.Locator(campo.Seletor);

      var count = await locator.CountAsync();

      var texto = count == 1 ? (campo.Atributo is null
        ? await locator.InnerTextAsync() : await locator.GetAttributeAsync(campo.Atributo))?.Trim() : null;

      campos[nome] = texto;

      if (campo.Obrigatorio && string.IsNullOrWhiteSpace(texto)) valido = false;
    }

    var link = string.IsNullOrWhiteSpace(config.LinkProduto) ? card : card.Locator(config.LinkProduto);

    var href = await link.CountAsync() == 1 ? await link.GetAttributeAsync("href") : null;

    var destino = ResolverLink(pagina, href, config);

    if (destino is not null && config.CaminhoProdutoPadrao is not null)
    {
      var caminho = Regex.Match(new Uri(destino).PathAndQuery, config.CaminhoProdutoPadrao,
        RegexOptions.None, TimeSpan.FromMilliseconds(100));

      destino = caminho.Success ? new Uri(new Uri(destino).GetLeftPart(UriPartial.Authority) + caminho.Value).AbsoluteUri : null;
    }

    return valido && destino is not null ? new(destino, campos) : null;
  }

  private static string? ResolverLink(string pagina, string? href, LojaPlaywrightOptions config) =>
    !string.IsNullOrWhiteSpace(href) && Uri.TryCreate(new Uri(pagina), href, out var uri) && Permitida(uri.AbsoluteUri, config)
      ? uri.AbsoluteUri : null;

  internal static bool Permitida(string url, LojaPlaywrightOptions config) =>
    Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http"
    && string.IsNullOrEmpty(uri.UserInfo)
    && config.HostsPermitidos.Contains(uri.IdnHost, StringComparer.OrdinalIgnoreCase);
}
