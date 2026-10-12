import { SolicitacoesPendentesService } from './solicitacoes-pendentes.service';

describe('SolicitacoesPendentesService', () => {
  beforeEach(() => sessionStorage.clear());

  afterEach(() => sessionStorage.clear());

  it('preserva pedidos distintos e recupera a chave após recarregar a aplicação', () => {
    const original = new SolicitacoesPendentesService();

    const iphone = original.obter('iphone 17');

    const samsung = original.obter('samsung s25');

    const recarregada = new SolicitacoesPendentesService();

    expect(recarregada.obter('iphone 17')).toEqual(iphone);

    expect(recarregada.obter('samsung s25')).toEqual(samsung);

    recarregada.concluir('iphone 17');

    expect(recarregada.obter('iphone 17').chaveIdempotencia).not.toBe(iphone.chaveIdempotencia);

    expect(recarregada.obter('samsung s25')).toEqual(samsung);
  });

  it('ignora conteúdo inválido no armazenamento', () => {
    sessionStorage.setItem('verum.busca.pendente:iphone 17', 'inválido');

    expect(new SolicitacoesPendentesService().obter('iphone 17').consulta).toBe('iphone 17');
  });
});
