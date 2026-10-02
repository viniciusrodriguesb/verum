namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record ResultadoConsultaLoja(
  string Loja,
  DateTimeOffset ConsultadaEm,
  IReadOnlyList<ItemConsultaLoja> Itens,
  IReadOnlyList<ItemLojaDescartado> Descartados,
  bool LimiteAtingido);
