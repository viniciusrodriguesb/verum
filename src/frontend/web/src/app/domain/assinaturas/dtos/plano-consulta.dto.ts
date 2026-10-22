export interface PlanoConsulta {
  codigo: string;
  valorMensal: number;
  moeda: string;
  limiteRadares?: number | null;
  intervaloRadarMinutos?: number | null;
}
