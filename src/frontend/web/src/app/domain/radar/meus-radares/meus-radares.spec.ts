import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { of, throwError } from 'rxjs';
import { RadarService } from '../services/radar.service';
import { MeusRadaresComponent } from './meus-radares';
import { MonitoramentoConsulta } from '../dtos/radar.dto';

describe('Operações do Radar', () => {
  const item: MonitoramentoConsulta = {
    id: 'radar',
    nome: 'Console',
    ofertaId: 'oferta',
    varianteId: 'variante',
    precoInicial: 1000,
    precoAlvo: 900,
    precoObservado: 1000,
    estado: 'Monitorando',
    criadoEm: '2026-10-21T12:00:00Z',
    verificadaEm: null,
    proximaVerificacaoEm: null,
    urlOferta: null,
    fonte: null,
    condicao: 'Novo',
    imagemUrl: null,
  };

  const api = {
    capacidade: vi.fn(() => of({ disponivel: true, permitido: true })),
    listar: vi.fn(() => of([item])),
    preferencias: vi.fn(() =>
      of({ emailHabilitado: false, silencioInicio: 0, silencioFim: 0, fuso: 'UTC' }),
    ),
    notificacoes: vi.fn(() => throwError(() => new Error('Indisponível'))),
    alterar: vi.fn(),
    salvarPreferencias: vi.fn(() => of(undefined)),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      imports: [MeusRadaresComponent],
      providers: [
        provideRouter([]),
        { provide: RadarService, useValue: api },
        { provide: AutenticacaoService, useValue: { autenticado: signal(true) } },
      ],
    }).overrideComponent(MeusRadaresComponent, { set: { template: '', imports: [] } });
  });

  it('preserva editor e preço após erro e fecha somente após sucesso', async () => {
    const componente = TestBed.createComponent(MeusRadaresComponent).componentInstance;

    componente['ajustar'](item);
    componente['edicao'].setValue(850.25);
    componente['aviso'].set('Sucesso antigo');
    api.alterar.mockReturnValueOnce(throwError(() => new Error('Falhou')));
    await componente['salvarMeta']();
    expect(componente['editando']()).toEqual(item);
    expect(componente['edicao'].value).toBe(850.25);
    expect(componente['aviso']()).toBeNull();
    expect(componente['erro']()).toBeTruthy();
    api.alterar.mockReturnValueOnce(of(undefined));
    await componente['salvarMeta']();
    expect(componente['editando']()).toBeNull();
  });

  it('falha de avisos não impede lista nem capacidade', async () => {
    const componente = TestBed.createComponent(MeusRadaresComponent).componentInstance;

    await componente['carregar']();
    expect(componente['radares']()).toEqual([item]);
    expect(componente['capacidade']()?.permitido).toBe(true);
    expect(componente['erroAvisos']()).toBeTruthy();
    expect(componente['erro']()).toBeNull();
  });

  it('pedir encerramento não altera o monitoramento e cancelar devolve foco', () => {
    const componente = TestBed.createComponent(MeusRadaresComponent).componentInstance;

    const botao = document.createElement('button');

    document.body.append(botao);
    componente['pedirEncerramento'](item.id, { currentTarget: botao } as unknown as Event);
    expect(api.alterar).not.toHaveBeenCalled();
    componente['cancelarEncerramento']();
    expect(document.activeElement).toBe(botao);
    botao.remove();
  });

  it('não salva valores padrão antes de carregar as preferências reais', async () => {
    const componente = TestBed.createComponent(MeusRadaresComponent).componentInstance;

    await componente['salvarPreferencias']();
    expect(api.salvarPreferencias).not.toHaveBeenCalled();
  });

  it('exige horários diferentes se o silêncio estiver ligado', async () => {
    const componente = TestBed.createComponent(MeusRadaresComponent).componentInstance;

    componente['preferenciasCarregadas'].set(true);
    componente['preferencias'].patchValue({ silencioInicio: 22, silencioFim: 22 });

    await componente['salvarPreferencias']();
    expect(api.salvarPreferencias).not.toHaveBeenCalled();
  });
});
