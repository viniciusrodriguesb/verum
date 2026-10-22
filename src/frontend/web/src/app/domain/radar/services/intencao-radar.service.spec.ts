import { IntencaoRadarService } from './intencao-radar.service';

describe('IntencaoRadarService', () => {
  const service = new IntencaoRadarService();

  afterEach(() => sessionStorage.clear());

  it('preserva apenas a intenção com prazo e não aceita IDs arbitrários', () => {
    service.salvar('01a0f9bc-332d-7389-ab10-f0fb1e38b66d', 900, 'mesma-operacao');

    expect(service.obter()?.precoAlvo).toBe(900);

    service.salvar('https://externo.example', 900, 'outra');

    expect(service.obter()).toBeNull();
  });

  it('descarta intenção vencida ou armazenamento corrompido', () => {
    sessionStorage.setItem(
      'verum.radar.intencao',
      JSON.stringify({
        ofertaId: '01a0f9bc-332d-7389-ab10-f0fb1e38b66d',
        precoAlvo: 900,
        chave: 'teste',
        expiraEm: 1,
      }),
    );

    expect(service.obter()).toBeNull();

    sessionStorage.setItem('verum.radar.intencao', '{');

    expect(service.obter()).toBeNull();
  });
});
