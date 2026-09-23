namespace Verum.Modules.Ofertas.Contratos.Cache;

// Compartilhado pelos fluxos de Busca e Radar por contrato, sem acesso ao DbContext de Ofertas.
public interface IOfertasCache
{
  Task<T?> ObterAtuaisAsync<T>(Guid varianteId, CancellationToken cancellationToken = default) where T : class;

  Task<bool> ArmazenarAtuaisAsync<T>(Guid varianteId, T ofertas, DateTimeOffset validasAte, CancellationToken cancellationToken = default) where T : class;

  Task<bool> InvalidarAsync(Guid varianteId, CancellationToken cancellationToken = default);

  Task<string?> TentarReservarAtualizacaoAsync(Guid varianteId, Guid fonteId, TimeSpan duracao, CancellationToken cancellationToken = default);

  Task<bool> RenovarReservaAsync(Guid varianteId, Guid fonteId, string token, TimeSpan duracao, CancellationToken cancellationToken = default);

  Task<bool> LiberarReservaAsync(Guid varianteId, Guid fonteId, string token, CancellationToken cancellationToken = default);
}
