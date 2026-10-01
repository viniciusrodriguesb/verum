namespace Verum.Modules.Ofertas.Contratos.Descoberta;

public sealed record OfertaDescoberta(
  string Nome, string Url, decimal Preco, string CondicaoPreco, string? ImagemUrl,
  string Fonte, bool OrigemIa, string Evidencia, DateTimeOffset ObservadaEm);

public sealed record OfertaConsultada(
  Guid Id, Guid VarianteId, string Nome, string Url, decimal Preco, string CondicaoPreco,
  string? ImagemUrl, string Fonte, decimal Confianca, DateTimeOffset ObservadaEm, DateTimeOffset ValidaAte);

public sealed record DiagnosticoFonte(
  string Fonte, bool OrigemIa, string Status, int Encontrados, int Aceitos, string? Codigo = null,
  DateTimeOffset? IniciadaEm = null, DateTimeOffset? FinalizadaEm = null);

public interface IOfertasDescobertaService
{
  Task<IReadOnlyList<OfertaConsultada>> ObterAtuaisAsync(Guid[] variantes, CancellationToken cancellationToken);

  Task<OfertaConsultada> RegistrarAsync(Guid varianteId, OfertaDescoberta oferta, CancellationToken cancellationToken);

  Task RegistrarExecucaoAsync(Guid buscaId, DiagnosticoFonte execucao, CancellationToken cancellationToken);
}
