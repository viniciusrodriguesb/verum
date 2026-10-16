export interface AcessoBusca {
  readonly contexto: 'Visitante' | 'Free' | 'Pago';
  readonly pacote: string;
  readonly limite: number;
  readonly utilizado: number;
  readonly restante: number;
  readonly reiniciaEm: string;
  readonly concessaoValidaAte: string | null;
}
