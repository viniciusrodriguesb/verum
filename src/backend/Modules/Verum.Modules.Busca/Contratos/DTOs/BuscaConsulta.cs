namespace Verum.Modules.Busca.Contratos.DTOs;

public sealed record BuscaConsulta(
  Guid Id, string Status, string Etapa, bool Parcial, string? Motivo, string? CodigoErro,
  int FontesConsultadas, int FontesComSucesso, int FontesComFalha,
  IReadOnlyList<OfertaResultado> Ofertas);
