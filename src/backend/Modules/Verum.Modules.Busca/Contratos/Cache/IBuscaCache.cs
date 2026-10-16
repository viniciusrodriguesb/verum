namespace Verum.Modules.Busca.Contratos.Cache;

public interface IBuscaCache
{
  Task<T?> ObterStatusAsync<T>(Guid buscaId, CancellationToken cancellationToken = default) where T : class;

  Task<bool> ArmazenarStatusAsync<T>(Guid buscaId, T status, CancellationToken cancellationToken = default) where T : class;

  Task<T?> ObterResultadoAsync<T>(Guid buscaId, CancellationToken cancellationToken = default) where T : class;

  Task<bool> ArmazenarResultadoAsync<T>(Guid buscaId, T resultado, DateTimeOffset validoAte, CancellationToken cancellationToken = default) where T : class;

  Task InvalidarAsync(Guid buscaId, CancellationToken cancellationToken = default);
}
