export interface CapacidadeRadar {
  readonly disponivel: boolean;
  readonly vendaDisponivel?: boolean;
  readonly permitido: boolean;
  readonly limite: number;
  readonly utilizado: number;
  readonly intervaloMinutos: number;
  readonly emailDisponivel: boolean;
  readonly planoNecessario: string;
}

export interface MonitoramentoConsulta {
  readonly id: string;
  readonly nome: string;
  readonly ofertaId: string;
  readonly varianteId: string;
  readonly precoInicial: number;
  readonly precoAlvo: number;
  readonly precoObservado: number | null;
  readonly estado: string;
  readonly criadoEm: string;
  readonly verificadaEm: string | null;
  readonly proximaVerificacaoEm: string | null;
  readonly urlOferta: string | null;
  readonly fonte: string | null;
  readonly condicao: string;
  readonly imagemUrl: string | null;
}

export interface OfertaRadar {
  readonly id: string;
  readonly varianteId: string;
  readonly nome: string;
  readonly preco: number;
  readonly condicao: string;
  readonly fonte: string;
  readonly url: string;
  readonly validaAte: string;
}

export interface PreferenciasAviso {
  readonly emailHabilitado: boolean;
  readonly silencioInicio: number;
  readonly silencioFim: number;
  readonly fuso: string;
}

export interface NotificacaoRadar {
  readonly id: string;
  readonly titulo: string;
  readonly mensagem: string;
  readonly criadaEm: string;
  readonly url: string | null;
  readonly entregas: readonly { readonly estado: string; readonly codigoErro: string | null }[];
}
