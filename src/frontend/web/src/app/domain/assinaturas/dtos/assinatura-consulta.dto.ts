export interface AssinaturaConsulta {
  id: string;
  plano: string;
  status: 'Pendente' | 'Ativa' | 'Inadimplente' | 'Cancelada' | 'Expirada';
  valor: number;
  moeda: string;
  urlCheckout: string | null;
  periodoTerminaEm: string | null;
  cancelamentoSolicitado: boolean;
}
