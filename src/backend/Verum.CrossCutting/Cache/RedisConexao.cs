using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Verum.CrossCutting.Cache;

public sealed class RedisConexao(IConfiguration configuration, IOptions<RedisOptions> options) : IDisposable
{
  private readonly object _sincronizacao = new();

  private Task<IConnectionMultiplexer>? _conexao;

  private bool _descartada;

  public Task<IConnectionMultiplexer> ObterAsync()
  {
    lock (_sincronizacao)
    {
      ObjectDisposedException.ThrowIf(_descartada, this);

      if (!options.Value.Enabled)
        throw new InvalidOperationException("Redis está desabilitado; a operação de coordenação não pode continuar.");

      if (_conexao is null || _conexao.IsFaulted || _conexao.IsCanceled)
        _conexao = ConectarAsync();

      return _conexao;
    }
  }

  private async Task<IConnectionMultiplexer> ConectarAsync()
  {
    var settings = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis")!);

    settings.AbortOnConnectFail = false;

    settings.ConnectRetry = 0;

    settings.ConnectTimeout = options.Value.TimeoutMilissegundos;

    settings.AsyncTimeout = options.Value.TimeoutMilissegundos;

    settings.SyncTimeout = options.Value.TimeoutMilissegundos;

    settings.BacklogPolicy = BacklogPolicy.FailFast;

    return await ConnectionMultiplexer.ConnectAsync(settings).ConfigureAwait(false);
  }

  public void Dispose()
  {
    lock (_sincronizacao)
    {
      if (_descartada)
        return;

      _descartada = true;

      if (_conexao is not null)
        _ = _conexao.ContinueWith(t => t.Result.Dispose(), CancellationToken.None,
          TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
  }
}
