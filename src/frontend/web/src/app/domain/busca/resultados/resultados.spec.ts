import { AUTENTICACAO_CONFIG } from '@core/autenticacao/configuracoes/autenticacao.config';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { BuscaConsulta } from '../dtos/busca-consulta.dto';
import { BuscasService } from '../services/buscas.service';
import { ResultadosComponent } from './resultados';

describe('validade das ofertas na listagem', () => {
  afterEach(() => vi.useRealTimers());

  it('atualiza o aviso sem mudar o status nem refazer a busca', async () => {
    vi.useFakeTimers();

    vi.setSystemTime(new Date('2026-10-12T12:00:00Z'));

    const resultado: BuscaConsulta = {
      id: 'busca-1',
      status: 'Concluida',
      etapa: 'Resultado',
      parcial: false,
      motivo: null,
      codigoErro: null,
      fontesConsultadas: 1,
      fontesComSucesso: 1,
      fontesComFalha: 0,
      ofertas: [
        {
          ofertaId: 'oferta-1',
          varianteId: 'variante-1',
          nome: 'iPhone 17',
          url: 'https://www.kabum.com.br/produto/1',
          preco: 5000,
          condicaoPreco: 'No PIX',
          imagemUrl: null,
          fonte: 'kabum',
          observadaEm: '2026-10-12T11:45:05Z',
          validaAte: '2026-10-12T12:00:05Z',
          posicao: 1,
          pontuacao: 80,
          explicacao: 'Oferta observada',
          pontuacaoPreco: 100,
          pontuacaoConfianca: 50,
          pontuacaoAtualizacao: 100,
        },
      ],
    };

    const obter = vi.fn(() => of(resultado));

    TestBed.configureTestingModule({
      imports: [ResultadosComponent],
      providers: [
        {
          provide: AUTENTICACAO_CONFIG,
          useValue: { url: 'https://login.example.test', realm: 'teste', clientId: 'teste-web' },
        },
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { queryParamMap: of(convertToParamMap({ id: 'busca-1', q: 'iphone 17' })) },
        },
        { provide: BuscasService, useValue: { obter } },
      ],
    });

    const fixture = TestBed.createComponent(ResultadosComponent);

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('validade vencida');

    await vi.advanceTimersByTimeAsync(10000);

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('validade vencida');

    expect(fixture.nativeElement.textContent).toContain('Consultar loja');

    expect(resultado.status).toBe('Concluida');

    expect(obter).toHaveBeenCalledOnce();

    fixture.destroy();
  });
});
