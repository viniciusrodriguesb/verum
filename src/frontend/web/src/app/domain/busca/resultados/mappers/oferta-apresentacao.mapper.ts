import { OfertaResultado as OfertaDto } from '../../dtos/oferta-resultado.dto';
import { OfertaResultado } from '../models/oferta-resultado.model';

export function apresentarOferta(dto: OfertaDto): OfertaResultado {
  const host = new URL(dto.url).hostname;
  return {
    id: dto.ofertaId,
    produtoId: dto.varianteId,
    produto: dto.nome,
    fabricante: '',
    preco: dto.preco,
    condicaoPreco: dto.condicaoPreco,
    quantidadeParcelas: null,
    valorParcela: null,
    freteGratis: null,
    produtoNovo: null,
    verificadaHaMinutos: Math.max(
      0,
      Math.floor((Date.now() - Date.parse(dto.observadaEm)) / 60000),
    ),
    pontuacaoRanking: dto.pontuacao / 10,
    destaque: 'melhor-escolha',
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
    },
    imagemUrl: dto.imagemUrl,
    urlOferta: dto.url,
  };
}
