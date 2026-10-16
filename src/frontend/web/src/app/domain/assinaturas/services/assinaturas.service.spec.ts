import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AssinaturasService } from './assinaturas.service';

describe('AssinaturasService', () => {
  beforeEach(() =>
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }),
  );

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('preserva a chave e não repete automaticamente uma contratação com resposta incerta', () => {
    const pedido = { plano: 'BUSCA_PLUS', chaveIdempotencia: crypto.randomUUID() };

    let falhou = false;

    TestBed.inject(AssinaturasService)
      .criar(pedido)
      .subscribe({ error: () => (falhou = true) });

    const request = TestBed.inject(HttpTestingController).expectOne('/api/v1/assinaturas');

    expect(request.request.body).toEqual(pedido);

    request.flush({}, { status: 503, statusText: 'Indisponível' });

    expect(falhou).toBe(true);

    TestBed.inject(HttpTestingController).expectNone('/api/v1/assinaturas');
  });
});
