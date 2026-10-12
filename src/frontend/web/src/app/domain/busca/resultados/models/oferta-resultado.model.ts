export type DestaqueOferta = 'melhor-escolha' | 'mais-confiavel' | 'menor-preco';

export type CodigoLoja = string;

export interface LojaOferta {
  readonly codigo: CodigoLoja;
  readonly nome: string;
  readonly confiavel: boolean | null;
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
  readonly condicaoPreco: string;
  readonly vencida: boolean;
  readonly quantidadeParcelas: number | null;
  readonly valorParcela: number | null;
  readonly freteGratis: boolean | null;
  readonly produtoNovo: boolean | null;
  readonly verificadaHaMinutos: number;
  readonly pontuacaoRanking: number;
  readonly destaque: DestaqueOferta;
  readonly loja: LojaOferta;
  readonly avaliacao: AvaliacaoOferta;
  readonly imagemUrl: string | null;
  readonly urlOferta: string;
}
