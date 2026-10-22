import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { ContaService } from '@core/autenticacao/services/conta.service';
import { AssinaturaConsulta } from '@domain/assinaturas/dtos/assinatura-consulta.dto';
import { AssinaturasService } from '@domain/assinaturas/services/assinaturas.service';
import { BuscasService } from '@domain/busca/services/buscas.service';
import { of, Subject, throwError } from 'rxjs';
import { MinhaContaComponent } from './minha-conta';

describe('Minha conta', () => {
  let fixture: ComponentFixture<MinhaContaComponent>;

  const assinatura: AssinaturaConsulta = {
    id: 'assinatura',
    plano: 'BUSCA_PLUS',
    status: 'Ativa',
    valor: 29,
    moeda: 'BRL',
    periodoTerminaEm: '2099-11-17T12:00:00Z',
    urlCheckout: null,
    cancelamentoSolicitado: false,
  };

  const perfil = {
    id: 'conta',
    sujeitoIdentidade: 'maria',
    nomeExibicao: 'Maria Silva',
    email: 'maria@example.test',
  };

  let contas: { obter: ReturnType<typeof vi.fn> };

  let buscas: { obterAcesso: ReturnType<typeof vi.fn> };

  let assinaturas: {
    listar: ReturnType<typeof vi.fn>;
    listarPlanos: ReturnType<typeof vi.fn>;
    criar: ReturnType<typeof vi.fn>;
    cancelar: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    contas = { obter: vi.fn(() => of(perfil)) };
    buscas = {
      obterAcesso: vi.fn(() =>
        of({
          contexto: 'Free',
          pacote: 'FREE',
          limite: 3,
          utilizado: 1,
          restante: 2,
          reiniciaEm: '2099-10-18T00:00:00Z',
          concessaoValidaAte: null,
        }),
      ),
    };
    assinaturas = {
      listar: vi.fn(() => of([])),
      listarPlanos: vi.fn(() => of([])),
      criar: vi.fn(),
      cancelar: vi.fn(() => of(undefined)),
    };
    TestBed.configureTestingModule({
      imports: [MinhaContaComponent],
      providers: [
        provideRouter([]),
        { provide: ContaService, useValue: contas },
        { provide: BuscasService, useValue: buscas },
        { provide: AssinaturasService, useValue: assinaturas },
        {
          provide: AutenticacaoService,
          useValue: {
            usuario: signal({ sujeito: 'maria', username: 'maria', emailConfirmado: true }),
            nome: signal('Maria'),
            autenticado: signal(true),
            gerenciarConta: vi.fn(),
            sair: vi.fn(),
          },
        },
      ],
    });
  });

  afterEach(() => {
    fixture?.destroy();
    sessionStorage.clear();
  });

  async function abrir() {
    fixture = TestBed.createComponent(MinhaContaComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function clicar(texto: string) {
    const botao = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ).find((item) => item.textContent?.trim() === texto);

    expect(botao).toBeTruthy();
    botao!.click();
    fixture.detectChanges();
  }

  it('exibe perfil e consumo real e informa quando não há planos à venda', async () => {
    await abrir();
    expect(fixture.nativeElement.textContent).toContain('Maria Silva');
    expect(fixture.nativeElement.textContent).toContain('Verum Free');
    expect(
      fixture.nativeElement.querySelector('[role="progressbar"]').getAttribute('aria-valuenow'),
    ).toBe('2');
    clicar('Plano e assinatura');
    expect(fixture.nativeElement.textContent).toContain(
      'Novas contratações estão temporariamente indisponíveis',
    );
    expect(fixture.nativeElement.textContent).not.toContain('Confirmar cancelamento');

    expect(fixture.nativeElement.textContent).not.toContain('Encontre o plano para você');

    expect(fixture.nativeElement.textContent).not.toContain('Valores mensais. Contratação segura');
  });

  it('representa cota esgotada por barra vazia e expõe horário da última atualização', async () => {
    buscas.obterAcesso.mockReturnValue(
      of({
        contexto: 'Free',
        pacote: 'FREE',
        limite: 3,
        utilizado: 3,
        restante: 0,
        reiniciaEm: '2099-10-18T00:00:00Z',
        concessaoValidaAte: null,
      }),
    );

    await abrir();

    const barra = fixture.nativeElement.querySelector('[role="progressbar"]');

    expect(barra.getAttribute('aria-valuenow')).toBe('0');

    expect(barra.getAttribute('aria-label')).toBe('Buscas disponíveis hoje');

    expect(barra.querySelector('span').style.width).toBe('0%');

    expect(fixture.nativeElement.textContent).toContain('3 de 3 buscas usadas');

    expect(fixture.nativeElement.textContent).toContain('Limite atingido');

    expect(fixture.nativeElement.textContent).toContain('Última atualização:');
  });

  it('mantém erro recuperável sem fabricar um plano gratuito quando a API falha', async () => {
    contas.obter.mockReturnValue(throwError(() => new Error('offline')));
    await abrir();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'Não foi possível',
    );
    expect(fixture.nativeElement.textContent).not.toContain('Verum Free');
    contas.obter.mockReturnValue(of(perfil));
    clicar('Tentar novamente');
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Maria Silva');
    });
  });

  it('mantém o acesso já pago ao mostrar uma assinatura cancelada', async () => {
    buscas.obterAcesso.mockReturnValue(
      of({
        contexto: 'Pago',
        pacote: 'BUSCA_PLUS',
        limite: 30,
        utilizado: 7,
        restante: 23,
        reiniciaEm: '2099-10-18T00:00:00Z',
        concessaoValidaAte: assinatura.periodoTerminaEm,
      }),
    );

    assinaturas.listar.mockReturnValue(
      of([{ ...assinatura, status: 'Cancelada', cancelamentoSolicitado: true }]),
    );

    await abrir();

    clicar('Plano e assinatura');

    expect(fixture.nativeElement.textContent).toContain('Acesso pago');

    expect(fixture.nativeElement.textContent).toContain('Desativada');

    expect(fixture.nativeElement.textContent).not.toContain('Cancelar renovação');
  });

  it('exige confirmação e distingue solicitação de cancelamento de confirmação do gateway', async () => {
    assinaturas.listar.mockReturnValue(of([assinatura]));
    await abrir();
    clicar('Plano e assinatura');
    clicar('Cancelar renovação');
    expect(assinaturas.cancelar).not.toHaveBeenCalled();
    clicar('Confirmar cancelamento');
    await fixture.whenStable();
    expect(assinaturas.cancelar).toHaveBeenCalledExactlyOnceWith('assinatura');
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Cancelamento em processamento');
    });
    expect(fixture.nativeElement.textContent).not.toContain('Confirmar cancelamento');
  });

  it('preserva idempotência após resposta incerta e bloqueia cliques durante a contratação', async () => {
    assinaturas.listarPlanos.mockReturnValue(
      of([{ codigo: 'BUSCA_PLUS', valorMensal: 29, moeda: 'BRL' }]),
    );
    assinaturas.criar.mockReturnValueOnce(throwError(() => new Error('timeout')));
    await abrir();
    clicar('Plano e assinatura');
    clicar('Escolher plano');
    clicar('Ir para o pagamento');
    await fixture.whenStable();
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Ir para o pagamento');
    });

    const primeiroPedido = assinaturas.criar.mock.calls[0][0];

    const resposta = new Subject<AssinaturaConsulta>();

    assinaturas.criar.mockReturnValue(resposta);
    clicar('Ir para o pagamento');
    expect(assinaturas.criar.mock.calls[1][0]).toEqual(primeiroPedido);
    clicar('Preparando…');
    expect(assinaturas.criar).toHaveBeenCalledTimes(2);
    resposta.next({ ...assinatura, status: 'Pendente' });
    resposta.complete();
    await fixture.whenStable();
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Aguardando confirmação');
    });
    expect(sessionStorage.length).toBe(0);
  });
});
