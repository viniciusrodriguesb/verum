import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { autenticacaoInterceptor } from './autenticacao.interceptor';
import { AutenticacaoService } from '../services/autenticacao.service';

describe('autenticacaoInterceptor', () => {
  const obterToken = vi.fn();

  beforeEach(() => {
    obterToken.mockReset().mockResolvedValue('token-de-acesso');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([autenticacaoInterceptor])),
        provideHttpClientTesting(),
        { provide: AutenticacaoService, useValue: { obterToken } },
      ],
    });
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('envia bearer apenas à API da própria origem', async () => {
    TestBed.inject(HttpClient).post('/api/v1/contas/me', null).subscribe();

    await Promise.resolve();

    const pedido = TestBed.inject(HttpTestingController).expectOne('/api/v1/contas/me');

    expect(pedido.request.headers.get('Authorization')).toBe('Bearer token-de-acesso');

    pedido.flush({});
  });

  it('não vaza tokens para outras origens ou arquivos estáticos', () => {
    for (const url of ['https://outra-loja.test/api/produtos', '/configuracao.json']) {
      TestBed.inject(HttpClient).get(url).subscribe();

      const pedido = TestBed.inject(HttpTestingController).expectOne(url);

      expect(pedido.request.headers.has('Authorization')).toBe(false);

      pedido.flush({});
    }

    expect(obterToken).not.toHaveBeenCalled();
  });
});
