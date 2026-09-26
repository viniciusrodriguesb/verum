using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Verum.BuildingBlocks.Erros;
using Verum.Modules.Ofertas.Contratos.Descoberta;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright;

internal sealed class ConsultaLojaPlaywright(NavegadorPlaywright navegador, IOptions<PlaywrightOptions> options, ILogger<ConsultaLojaPlaywright> logger) : IConsultaLoja
{
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

    for (var numero = 1; numero <= config.MaxPaginas; numero++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!Permitida(url.AbsoluteUri, config))
        throw new InvalidOperationException("A navegação saiu dos hosts configurados para a loja.");

      if (!paginas.Add(url.AbsoluteUri)) break;

      var response = await page.GotoAsync(url.AbsoluteUri, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

      if (response is null || !response.Ok)
        throw new HttpRequestException("A loja retornou uma resposta HTTP inválida.", null, response is null ? null : (System.Net.HttpStatusCode)response.Status);

      if (!Permitida(page.Url, config))
        throw new InvalidOperationException("O redirecionamento saiu dos hosts configurados.");

      if (numero == 1)
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

      await page.Locator(config.ResultadosProntos).WaitForAsync(new() { State = WaitForSelectorState.Visible });

      var cards = page.Locator(config.Itens);

      var quantidade = await cards.CountAsync();

      limiteAtingido |= quantidade > config.MaxItens;

      if (quantidade == 0)
      {
        if (config.SemResultados is null || !await page.Locator(config.SemResultados).IsVisibleAsync())
          throw new InvalidOperationException("Layout da loja não corresponde aos seletores configurados.");

        break;
      }

      for (var indice = 0; indice < Math.Min(quantidade, config.MaxItens); indice++)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var card = cards.Nth(indice);

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

        var destino = ResolverLink(page.Url, href, config);

        if (!valido || destino is null)
        {
          descartados.Add(new(numero, indice, "CAMPOS_INVALIDOS"));

          logger.LogWarning("Item descartado na loja {Loja}, página {Pagina}, índice {Indice}.", loja, numero, indice);

          continue;
        }

        if (urls.Add(destino)) itens.Add(new(destino, campos));

        if (itens.Count >= config.MaxItens)
          return new(loja, DateTimeOffset.UtcNow, itens, descartados, true);
      }

      if (config.ProximaPagina is null) break;

      var proxima = page.Locator(config.ProximaPagina);

      if (await proxima.CountAsync() == 0) break;

      var proximaUrl = ResolverLink(page.Url, await proxima.GetAttributeAsync("href"), config)
        ?? throw new InvalidOperationException("Link de paginação inválido.");

      if (numero == config.MaxPaginas)
        return new(loja, DateTimeOffset.UtcNow, itens, descartados, true);

      url = new Uri(proximaUrl);
    }

    return new(loja, DateTimeOffset.UtcNow, itens, descartados, limiteAtingido);
  }

  private static string? ResolverLink(string pagina, string? href, LojaPlaywrightOptions config) =>
    !string.IsNullOrWhiteSpace(href) && Uri.TryCreate(new Uri(pagina), href, out var uri) && Permitida(uri.AbsoluteUri, config)
      ? uri.AbsoluteUri : null;

  internal static bool Permitida(string url, LojaPlaywrightOptions config) =>
    Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http"
    && string.IsNullOrEmpty(uri.UserInfo)
    && config.HostsPermitidos.Contains(uri.IdnHost, StringComparer.OrdinalIgnoreCase);
}
