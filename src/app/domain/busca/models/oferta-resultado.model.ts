export type DestaqueOferta = 'melhor-escolha' | 'mais-confiavel' | 'menor-preco';

export type CodigoLoja = 'magazine-luiza' | 'amazon' | 'kabum';

export interface LojaOferta {
  readonly codigo: CodigoLoja;
  readonly nome: string;
  readonly confiavel: boolean;
}

export interface AvaliacaoOferta {
  readonly preco: number;
  readonly confianca: number;
  readonly atualizacao: number;
  readonly resumo: string;
}

export interface OfertaResultado {
  readonly id: string;
  readonly produtoId: string;
  readonly produto: string;
  readonly fabricante: string;
  readonly preco: number;
  readonly quantidadeParcelas: number;
  readonly valorParcela: number;
  readonly freteGratis: boolean;
  readonly produtoNovo: boolean;
  readonly verificadaHaMinutos: number;
  readonly pontuacaoRanking: number;
  readonly destaque: DestaqueOferta;
  readonly loja: LojaOferta;
  readonly avaliacao: AvaliacaoOferta;
  readonly imagemUrl: string | null;
  readonly urlOferta: string;
}
