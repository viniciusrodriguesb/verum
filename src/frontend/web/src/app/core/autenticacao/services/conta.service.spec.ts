import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AutenticacaoService } from './autenticacao.service';
import { ContaService } from './conta.service';

describe('ContaService', () => {
  it('sincroniza sem enviar claims no corpo e esconde a conta quando a identidade muda', async () => {
    const usuario = signal<{ sujeito: string } | null>({ sujeito: 'maria' });

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AutenticacaoService, useValue: { usuario } },
      ],
    });

    const service = TestBed.inject(ContaService);

    const sincronizacao = service.sincronizar();

    const pedido = TestBed.inject(HttpTestingController).expectOne('/api/v1/contas/me');

    expect(pedido.request.body).toBeNull();

    pedido.flush({
      id: 'conta-maria',
      sujeitoIdentidade: 'maria',
      email: 'maria@example.test',
      nomeExibicao: 'Maria',
    });

    await sincronizacao;

    expect(service.conta()?.id).toBe('conta-maria');

    usuario.set(null);

    expect(service.conta()).toBeNull();

    usuario.set({ sujeito: 'outro-usuario' });

    expect(service.conta()).toBeNull();

    TestBed.inject(HttpTestingController).verify();
  });
});
