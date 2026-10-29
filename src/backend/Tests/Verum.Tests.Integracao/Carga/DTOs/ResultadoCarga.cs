namespace Verum.Tests.Integracao.Carga.DTOs;

internal sealed record ResultadoCarga(string Consulta, string Status, double Segundos, bool Parcial,
  int FontesComSucesso, int FontesComFalha, int Alertas, string[] Ofertas, string? Erro);
