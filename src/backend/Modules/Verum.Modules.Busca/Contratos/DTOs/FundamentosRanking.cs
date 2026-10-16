namespace Verum.Modules.Busca.Contratos.DTOs;

public sealed record FundamentosRanking(string Versao, string ConfiancaCalculo, bool PrecoProvisorio,
  int LojasMercado, int Observacoes, int DiasHistorico, int LojasHistorico, decimal? MedianaMercado,
  decimal? MedianaHistorica, decimal? PercentilHistorico, double IdadeMinutos,
  decimal PesoPreco = 0.45m, decimal PesoConfianca = 0.35m, decimal PesoAtualizacao = 0.20m,
  int JanelaHistoricoDias = 30, int MeiaVidaMinutos = 60, decimal ToleranciaPreco = 0.005m);
