namespace Verum.Modules.Acesso.Contratos.Cache;

public interface IAcessoCache
{
  Task<T?> ObterPermissoesAsync<T>(Guid contaId, CancellationToken cancellationToken = default) where T : class;

  Task<bool> ArmazenarPermissoesAsync<T>(Guid contaId, T permissoes, DateTimeOffset validasAte, CancellationToken cancellationToken = default) where T : class;

  Task<bool> InvalidarPermissoesAsync(Guid contaId, CancellationToken cancellationToken = default);

  Task<ResultadoLimite> TentarConsumirLimiteContaAsync(Guid contaId, string recurso, int limite, DateTimeOffset fimJanela, CancellationToken cancellationToken = default);

  Task<ResultadoLimite> TentarConsumirLimiteVisitanteAsync(Guid visitanteId, string recurso, int limite, DateTimeOffset fimJanela, CancellationToken cancellationToken = default);
}
