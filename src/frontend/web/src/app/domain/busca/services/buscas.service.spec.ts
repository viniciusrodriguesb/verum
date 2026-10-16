import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { BuscasService } from './buscas.service';

describe('BuscasService', () => {
  let service: BuscasService;

  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(BuscasService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    vi.useRealTimers();
  });

  it('envia o contrato e preserva o cookie sem repetir o POST', () => {
    const pedido = { consulta: 'ssd kingston', chaveIdempotencia: 'chave-1' };

    const sucesso = vi.fn();

    service.solicitar(pedido).subscribe(sucesso);

    const request = http.expectOne('/api/v1/buscas');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(pedido);
    expect(request.request.withCredentials).toBe(true);
    request.flush({ id: 'id-1', status: 'Recebida' });
    expect(sucesso).toHaveBeenCalledWith({ id: 'id-1', status: 'Recebida' });
  });

  it('interrompe o acompanhamento após o estado final', async () => {
    vi.useFakeTimers();

    const estados: string[] = [];

    const concluida = vi.fn();

    service
      .acompanhar('id-1')
      .subscribe({ next: (busca) => estados.push(busca.status), complete: concluida });
    await vi.advanceTimersByTimeAsync(0);

    const primeira = http.expectOne('/api/v1/buscas/id-1');

    expect(primeira.request.withCredentials).toBe(true);
    await vi.advanceTimersByTimeAsync(2000);
    http.expectNone('/api/v1/buscas/id-1');
    primeira.flush({ status: 'Processando' });
    await vi.advanceTimersByTimeAsync(2000);
    http.expectOne('/api/v1/buscas/id-1').flush({ status: 'Concluida', ofertas: [] });
    await vi.advanceTimersByTimeAsync(10000);
    http.expectNone('/api/v1/buscas/id-1');
    expect(estados).toEqual(['Processando', 'Concluida']);
    expect(concluida).toHaveBeenCalledOnce();
  });

  it('propaga o limite diário sem nova solicitação automática', () => {
    const erro = vi.fn();

    service
      .solicitar({ consulta: 'ssd kingston', chaveIdempotencia: 'chave-1' })
      .subscribe({ error: erro });
    http
      .expectOne('/api/v1/buscas')
      .flush({ code: 'LIMITE_BUSCAS' }, { status: 429, statusText: 'Too Many Requests' });
    expect(erro).toHaveBeenCalledOnce();
    http.expectNone('/api/v1/buscas');
  });
});
