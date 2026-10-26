using Verum.Modules.Busca.Contratos.Configuracoes;

namespace Verum.Modules.Busca.Contratos.DTOs;

public sealed record FundamentosRanking(string Versao, string ConfiancaCalculo, bool PrecoProvisorio,
  int LojasMercado, int Observacoes, int DiasHistorico, int LojasHistorico, decimal? MedianaMercado,
  decimal? MedianaHistorica, decimal? PercentilHistorico, double IdadeMinutos,
  decimal PesoPreco = CriteriosRanking.PesoPreco, decimal PesoConfianca = CriteriosRanking.PesoConfianca,
  decimal PesoAtualizacao = CriteriosRanking.PesoAtualizacao, int JanelaHistoricoDias = CriteriosRanking.JanelaHistoricoDias,
  int MeiaVidaMinutos = CriteriosRanking.MeiaVidaMinutos, decimal ToleranciaPreco = CriteriosRanking.ToleranciaPreco);
