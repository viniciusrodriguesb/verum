import { OfertaResultado } from './oferta-resultado.dto';
export interface BuscaConsulta {
  readonly id: string;
  readonly status:
    'Recebida' | 'Processando' | 'Concluida' | 'ConcluidaSemResultado' | 'Falhou' | 'Expirada';
  readonly etapa: string;
  readonly parcial: boolean;
  readonly motivo: string | null;
  readonly codigoErro: string | null;
  readonly fontesConsultadas: number;
  readonly fontesComSucesso: number;
  readonly fontesComFalha: number;
  readonly ofertas: readonly OfertaResultado[];
}
