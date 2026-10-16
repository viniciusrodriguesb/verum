import { TestBed } from '@angular/core/testing';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { SolicitacoesPendentesService } from './solicitacoes-pendentes.service';

let sujeito: string | null = null;

const autenticacao = { usuario: () => (sujeito ? { sujeito } : null) } as AutenticacaoService;

describe('SolicitacoesPendentesService', () => {
  beforeEach(() => {
    sessionStorage.clear();
    sujeito = null;
    TestBed.configureTestingModule({
      providers: [{ provide: AutenticacaoService, useValue: autenticacao }],
    });
  });

  afterEach(() => sessionStorage.clear());

  it('preserva pedidos distintos e recupera a chave após recarregar a aplicação', () => {
    const original = TestBed.runInInjectionContext(() => new SolicitacoesPendentesService());

    const iphone = original.obter('iphone 17');

    const samsung = original.obter('samsung s25');

    const recarregada = TestBed.runInInjectionContext(() => new SolicitacoesPendentesService());

    expect(recarregada.obter('iphone 17')).toEqual(iphone);

    expect(recarregada.obter('samsung s25')).toEqual(samsung);

    recarregada.concluir('iphone 17');

    expect(recarregada.obter('iphone 17').chaveIdempotencia).not.toBe(iphone.chaveIdempotencia);

    expect(recarregada.obter('samsung s25')).toEqual(samsung);
  });

  it('ignora conteúdo inválido no armazenamento', () => {
    sessionStorage.setItem('verum.busca.pendente:iphone 17', 'inválido');

    const service = TestBed.runInInjectionContext(() => new SolicitacoesPendentesService());

    expect(service.obter('iphone 17').consulta).toBe('iphone 17');
  });

  it('não migra solicitações pendentes entre visitante e contas diferentes', () => {
    const service = TestBed.runInInjectionContext(() => new SolicitacoesPendentesService());

    const visitante = service.obter('iphone 17');

    sujeito = 'conta-a';

    const contaA = service.obter('iphone 17');

    sujeito = 'conta-b';

    const contaB = service.obter('iphone 17');

    expect(
      new Set([visitante.chaveIdempotencia, contaA.chaveIdempotencia, contaB.chaveIdempotencia])
        .size,
    ).toBe(3);

    sujeito = null;

    expect(service.obter('iphone 17')).toEqual(visitante);
  });
});
