namespace Verum.Modules.Ofertas.Contratos.Descoberta.DTOs;

public sealed record DiagnosticoFonte(
  string Fonte, bool OrigemIa, string Status, int Encontrados, int Aceitos, string? Codigo = null,
  DateTimeOffset? IniciadaEm = null, DateTimeOffset? FinalizadaEm = null);
