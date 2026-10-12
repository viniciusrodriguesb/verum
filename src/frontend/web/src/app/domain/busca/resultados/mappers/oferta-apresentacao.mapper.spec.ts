import { OfertaResultado } from '../../dtos/oferta-resultado.dto';
import { apresentarOferta } from './oferta-apresentacao.mapper';

describe('apresentarOferta', () => {
  const oferta: OfertaResultado = {
    ofertaId: 'oferta-1',
    varianteId: 'variante-1',
    nome: 'iPhone 17',
    url: 'https://www.kabum.com.br/produto/1',
    preco: 5000,
    condicaoPreco: 'No PIX',
    imagemUrl: null,
    fonte: 'kabum',
    observadaEm: '2026-10-12T12:00:00Z',
    validaAte: '2026-10-12T12:15:00Z',
    posicao: 1,
    pontuacao: 80,
    explicacao: 'Oferta observada',
    pontuacaoPreco: 100,
    pontuacaoConfianca: 50,
    pontuacaoAtualizacao: 100,
  };

  it('preserva preço e snapshot, distinguindo a oferta válida da vencida', () => {
    const valida = apresentarOferta(oferta, Date.parse('2026-10-12T12:14:59Z'));

    const vencida = apresentarOferta(oferta, Date.parse('2026-10-12T12:15:00Z'));

    expect(valida.vencida).toBe(false);

    expect(vencida.vencida).toBe(true);

    expect(vencida.preco).toBe(oferta.preco);

    expect(vencida.urlOferta).toBe(oferta.url);
  });

  it('não apresenta validade como confirmada quando a data é inválida', () => {
    expect(apresentarOferta({ ...oferta, validaAte: 'inválida' }).vencida).toBe(true);
  });
});
