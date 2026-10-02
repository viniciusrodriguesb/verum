using Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

namespace Verum.Modules.Ofertas.Contratos.Descoberta;

public interface IOfertasDescobertaService
{
  Task<IReadOnlyList<OfertaConsultada>> ObterAtuaisAsync(Guid[] variantes, CancellationToken cancellationToken);

  Task<OfertaConsultada> RegistrarAsync(Guid varianteId, OfertaDescoberta oferta, CancellationToken cancellationToken);

  Task RegistrarExecucaoAsync(Guid buscaId, DiagnosticoFonte execucao, CancellationToken cancellationToken);
}
