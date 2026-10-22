namespace Verum.Modules.Assinaturas.Contratos.DTOs;

public sealed record PlanoConsulta(string Codigo, decimal ValorMensal, string Moeda,
  int? LimiteRadares = null, int? IntervaloRadarMinutos = null);
