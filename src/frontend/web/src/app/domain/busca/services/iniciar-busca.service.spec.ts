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

  afterEach(() => http.verify());

  it('permanece no formulário e mostra o limite diário sem abrir o radar', () => {
    service.iniciar('iphone');

    service.iniciar('iphone');

    expect(navegar).not.toHaveBeenCalled();

    http.expectOne('/api/v1/buscas').flush({}, { status: 429, statusText: 'Too Many Requests' });

    expect(navegar).not.toHaveBeenCalled();

    expect(TestBed.inject(AvisosApiService).mensagem()).toContain('limite diário');

    expect(service.enviando()).toBe(false);
  });

  it('abre o radar somente após a aceitação da API, com o id recebido', () => {
    service.iniciar('iphone');

    expect(navegar).not.toHaveBeenCalled();

    http.expectOne('/api/v1/buscas').flush({ id: 'busca-1', status: 'Recebida' });

    expect(navegar).toHaveBeenCalledWith(['/busca/processamento'], {
      queryParams: { q: 'iphone', id: 'busca-1' },
    });
  });
});
