import { OfertaResultado as OfertaDto } from '../../dtos/oferta-resultado.dto';
import { OfertaResultado } from '../models/oferta-resultado.model';

export function apresentarOferta(dto: OfertaDto, agora = Date.now()): OfertaResultado {
  const host = new URL(dto.url).hostname;

  return {
    id: dto.ofertaId,
    produtoId: dto.varianteId,
    produto: dto.nome,
    fabricante: '',
    preco: dto.preco,
    condicaoPreco: dto.condicaoPreco,
    vencida: !Number.isFinite(Date.parse(dto.validaAte)) || Date.parse(dto.validaAte) <= agora,
    quantidadeParcelas: null,
    valorParcela: null,
    freteGratis: null,
    produtoNovo: null,
    verificadaHaMinutos: Math.max(0, Math.floor((agora - Date.parse(dto.observadaEm)) / 60000)),
    pontuacaoRanking: dto.pontuacao / 10,
    destaque: 'comum',
    loja: {
      codigo: host === 'www.kabum.com.br' ? 'kabum' : host,
      nome: host === 'www.kabum.com.br' ? 'KaBuM!' : host,
      confiavel: null,
    },
    avaliacao: {
      preco: dto.pontuacaoPreco / 10,
      confianca: dto.pontuacaoConfianca / 10,
      atualizacao: dto.pontuacaoAtualizacao / 10,
      resumo: dto.explicacao,
      fundamentos: dto.fundamentos,
    },
    imagemUrl: dto.imagemUrl,
    urlOferta: dto.url,
  };
}
