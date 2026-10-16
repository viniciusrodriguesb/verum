export interface BuscaHistorico {
  readonly id: string;
  readonly consulta: string;
  readonly status: string;
  readonly iniciadaEm: string;
  readonly concluidaEm: string | null;
}
