export interface OfertaResultado {
  readonly ofertaId: string;
  readonly varianteId: string;
  readonly nome: string;
  readonly url: string;
  readonly preco: number;
  readonly condicaoPreco: string;
  readonly imagemUrl: string | null;
  readonly fonte: string;
  readonly observadaEm: string;
  readonly validaAte: string;
  readonly posicao: number;
  readonly pontuacao: number;
  readonly explicacao: string;
  readonly pontuacaoPreco: number;
  readonly pontuacaoConfianca: number;
  readonly pontuacaoAtualizacao: number;
}
