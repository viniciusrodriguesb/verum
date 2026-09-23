namespace Verum.Modules.Catalogo.Contratos.Cache;

public interface ICatalogoCache
{
  Task<T?> ObterVarianteAsync<T>(Guid varianteId, CancellationToken cancellationToken = default) where T : class;

  Task<bool> ArmazenarVarianteAsync<T>(Guid varianteId, T variante, CancellationToken cancellationToken = default) where T : class;

  Task<bool> InvalidarVarianteAsync(Guid varianteId, CancellationToken cancellationToken = default);

  Task<T?> ObterResolucaoAsync<T>(string consultaNormalizada, string versaoInterpretador, CancellationToken cancellationToken = default) where T : class;

  Task<bool> ArmazenarResolucaoAsync<T>(string consultaNormalizada, string versaoInterpretador, T resolucao, CancellationToken cancellationToken = default) where T : class;

  Task<bool> InvalidarResolucaoAsync(string consultaNormalizada, string versaoInterpretador, CancellationToken cancellationToken = default);
}
