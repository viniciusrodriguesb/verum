import { AssinaturaConsulta } from '@domain/assinaturas/dtos/assinatura-consulta.dto';
import { checkoutSeguro, nomeStatus, tempoRestante } from './plano-apresentacao';

describe('Apresentação do plano', () => {
  it('diferencia expiração, prazo curto e acesso sem data final', () => {
    const agora = Date.parse('2026-10-17T12:00:00Z');

    expect(tempoRestante(null, agora)).toBe('Sem data de expiração');
    expect(tempoRestante('2026-10-17T11:00:00Z', agora)).toBe('Período encerrado');
    expect(tempoRestante('2026-10-17T12:30:00Z', agora)).toBe('Menos de 1 hora');
    expect(tempoRestante('2026-10-19T12:00:00Z', agora)).toBe('Faltam 2 dias');
    expect(tempoRestante('inválido', agora)).toBe('Data indisponível');
  });

  it('não apresenta solicitação de cancelamento como cancelamento confirmado', () => {
    const assinatura = { status: 'Ativa', cancelamentoSolicitado: true } as AssinaturaConsulta;

    expect(nomeStatus(assinatura)).toBe('Cancelamento em processamento');
    expect(nomeStatus({ ...assinatura, status: 'Cancelada' })).toBe('Cancelada');
  });

  it('só permite o checkout HTTPS do gateway implementado', () => {
    expect(checkoutSeguro('https://www.mercadopago.com.br/subscriptions/checkout?id=1')).toContain(
      '/subscriptions/checkout',
    );

    for (const url of [
      null,
      'javascript:alert(1)',
      'https://www.mercadopago.com.br.evil.test',
      'http://www.mercadopago.com.br',
      'https://user:pass@www.mercadopago.com.br',
    ]) {
      expect(checkoutSeguro(url)).toBeNull();
    }
  });
});
