import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { InicioComponent } from './inicio';

describe('Home e cota da busca', () => {
  let fixture: ComponentFixture<InicioComponent>;

  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [InicioComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: AutenticacaoService,
          useValue: {
            autenticado: signal(true),
            nome: signal('Maria'),
            usuario: signal({ sujeito: 'maria' }),
          },
        },
      ],
    });

    http = TestBed.inject(HttpTestingController);

    fixture = TestBed.createComponent(InicioComponent);

    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();

    http.verify();
  });

  function saldo(restante: number) {
    http.expectOne('/api/v1/buscas/acesso').flush({
      contexto: 'Free',
      pacote: 'FREE',
      limite: 3,
      utilizado: 3 - restante,
      restante,
      reiniciaEm: '2099-10-18T00:00:00Z',
      concessaoValidaAte: null,
    });

    fixture.detectChanges();
  }

  it('antecipa a cota com dados reais e remove promessas de ofertas vazias', () => {
    saldo(2);

    const texto = fixture.nativeElement.textContent;

    expect(texto).toContain('2 buscas restantes de 3');

    expect(texto).not.toContain('Preços atualizados agora');

    expect(texto).not.toContain('Ver mais resultados');

    expect(fixture.nativeElement.querySelector('verum-navegacao-mobile')).toBeTruthy();
  });

  it('bloqueia o envio antes do POST e mantém a orientação de reposição junto ao campo', () => {
    saldo(0);

    const input = fixture.nativeElement.querySelector('input');

    input.value = 'iphone 15';

    input.dispatchEvent(new Event('input'));

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(true);

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

    http.expectNone('/api/v1/buscas');

    expect(fixture.nativeElement.textContent).toContain('Novas buscas em');

    expect(fixture.nativeElement.textContent).toContain('seu horário local');

    expect(fixture.nativeElement.querySelector('#estado-busca a').getAttribute('href')).toBe(
      '/conta',
    );
  });

  it('não inventa saldo se a consulta falhar e conserva a validação do termo', () => {
    http.expectOne('/api/v1/buscas/acesso').flush({}, { status: 503, statusText: 'Offline' });

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Informe o produto');

    expect(fixture.nativeElement.textContent).not.toContain('buscas restantes');

    http.expectNone('/api/v1/buscas');
  });
});
