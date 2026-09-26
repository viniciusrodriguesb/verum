using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace Verum.Modules.Ofertas.Infraestrutura.Descoberta.Playwright;

internal sealed class NavegadorPlaywright : IAsyncDisposable
{

  #region Construtor
  private readonly PlaywrightOptions _options;
  private readonly SemaphoreSlim _vagas;
  private readonly SemaphoreSlim _inicializacao = new(1, 1);
  private readonly CancellationTokenSource _encerramento = new();
  private IPlaywright? _playwright;
  private IBrowser? _browser;
  public NavegadorPlaywright(IOptions<PlaywrightOptions> options)
  {
    _options = options.Value;

    _vagas = new SemaphoreSlim(_options.ConsultasSimultaneas, _options.ConsultasSimultaneas);
  } 
  #endregion

  public async Task<T> ExecutarAsync<T>(Func<IBrowserContext, CancellationToken, Task<T>> executar, CancellationToken cancellationToken)
  {
    using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _encerramento.Token);

    prazo.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutTotalSegundos));

    var entrou = false;

    IBrowserContext? context = null;

    Task<T>? operacao = null;

    try
    {
      await _vagas.WaitAsync(prazo.Token);

      entrou = true;

      var browser = await ObterBrowserAsync(prazo.Token);

      prazo.Token.ThrowIfCancellationRequested();

      context = await browser.NewContextAsync(new()
      {
        Locale = "pt-BR",
        TimezoneId = "America/Sao_Paulo",
        AcceptDownloads = false,
        ServiceWorkers = ServiceWorkerPolicy.Block
      });

      context.SetDefaultTimeout(_options.TimeoutAcaoSegundos * 1000);

      context.SetDefaultNavigationTimeout(_options.TimeoutAcaoSegundos * 1000);

      prazo.Token.ThrowIfCancellationRequested();

      operacao = executar(context, prazo.Token);

      return await operacao.WaitAsync(prazo.Token);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && !_encerramento.IsCancellationRequested)
    {
      throw new TimeoutException("A consulta da loja excedeu o prazo total.");
    }
    finally
    {
      if (context is not null)
      {
        try { await context.CloseAsync(); }
        catch (PlaywrightException) { }
      }

      if (operacao is not null)
      {
        try { await operacao; }
        catch (Exception) { }
      }

      if (entrou) _vagas.Release();
    }
  }

  private async Task<IBrowser> ObterBrowserAsync(CancellationToken cancellationToken)
  {
    await _inicializacao.WaitAsync(cancellationToken);

    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (_browser?.IsConnected == true) return _browser;

      if (_browser is not null) await _browser.DisposeAsync();

      _playwright ??= await Microsoft.Playwright.Playwright.CreateAsync();

      _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true, Timeout = 10000 });

      return _browser;
    }
    finally
    {
      _inicializacao.Release();
    }
  }

  public async ValueTask DisposeAsync()
  {
    await _encerramento.CancelAsync();

    for (var i = 0; i < _options.ConsultasSimultaneas; i++) await _vagas.WaitAsync();

    if (_browser is not null) await _browser.DisposeAsync();

    _playwright?.Dispose();

    _encerramento.Dispose();

    _vagas.Dispose();

    _inicializacao.Dispose();
  }
}

