export interface FundamentosRanking {
  readonly versao: string;
  readonly confiancaCalculo: 'Baixa' | 'Media' | 'Alta';
  readonly precoProvisorio: boolean;
  readonly lojasMercado: number;
  readonly observacoes: number;
  readonly diasHistorico: number;
  readonly lojasHistorico: number;
  readonly medianaMercado: number | null;
  readonly medianaHistorica: number | null;
  readonly percentilHistorico: number | null;
  readonly idadeMinutos: number;
  readonly pesoPreco: number;
  readonly pesoConfianca: number;
  readonly pesoAtualizacao: number;
  readonly janelaHistoricoDias: number;
  readonly meiaVidaMinutos: number;
  readonly toleranciaPreco: number;
}
