import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { errosApiInterceptor } from '@shared/interceptors/erros-api.interceptor';
import { AvisosApiService } from '@shared/services/avisos-api.service';

import { IniciarBuscaService } from './iniciar-busca.service';

describe('IniciarBuscaService', () => {
  const navegar = vi.fn().mockResolvedValue(true);

  let service: IniciarBuscaService;

  let http: HttpTestingController;

  beforeEach(() => {
    navegar.mockClear();

    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        IniciarBuscaService,
        provideHttpClient(withInterceptors([errosApiInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate: navegar } },
      ],
    });

    service = TestBed.inject(IniciarBuscaService);

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();

    vi.useRealTimers();

    sessionStorage.clear();
  });

  it('permanece no formulário e mostra o limite diário sem abrir o radar', () => {
    service.iniciar('iphone');

    service.iniciar('iphone');

    http.expectOne('/api/v1/buscas/visitante').flush(null);

    expect(navegar).not.toHaveBeenCalled();

    http.expectOne('/api/v1/buscas').flush({}, { status: 429, statusText: 'Too Many Requests' });

    expect(navegar).not.toHaveBeenCalled();

    expect(TestBed.inject(AvisosApiService).mensagem()).toContain('limite diário');

    expect(service.enviando()).toBe(false);
  });

  it('abre o radar somente após a aceitação da API, com o id recebido', () => {
    service.iniciar('iphone');

    http.expectOne('/api/v1/buscas/visitante').flush(null);

    expect(navegar).not.toHaveBeenCalled();

    http.expectOne('/api/v1/buscas').flush({ id: 'busca-1', status: 'Recebida' });

    expect(navegar).toHaveBeenCalledWith(['/busca/processamento'], {
      queryParams: { q: 'iphone', id: 'busca-1' },
    });
  });

  it('após timeout retoma o POST com a mesma chave sem repetição automática', async () => {
    vi.useFakeTimers();

    service.iniciar('iphone 17');

    http.expectOne('/api/v1/buscas/visitante').flush(null);

    const original = http.expectOne('/api/v1/buscas');

    const pedido = original.request.body;

    await vi.advanceTimersByTimeAsync(15001);

    expect(original.cancelled).toBe(true);

    expect(navegar).not.toHaveBeenCalled();

    http.expectNone('/api/v1/buscas');

    service.iniciar('iphone 17');

    http.expectOne('/api/v1/buscas/visitante').flush(null);

    const retomada = http.expectOne('/api/v1/buscas');

    expect(retomada.request.body).toEqual(pedido);

    retomada.flush({ id: 'busca-original', status: 'Recebida' });

    expect(navegar).toHaveBeenCalledWith(['/busca/processamento'], {
      queryParams: { q: 'iphone 17', id: 'busca-original' },
    });
  });

  it('não envia o POST enquanto a sessão de visitante não foi estabelecida', () => {
    service.iniciar('iphone 17');

    http.expectNone('/api/v1/buscas');

    http
      .expectOne('/api/v1/buscas/visitante')
      .flush({}, { status: 503, statusText: 'Unavailable' });

    http.expectNone('/api/v1/buscas');

    expect(navegar).not.toHaveBeenCalled();
  });
});
