import { valorMonetario } from './valor-monetario';

describe('Valor monetário em BRL', () => {
  it.each(['1234,56', '1234.56', 'R$ 1.234,56'])('preserva centavos de %s', (texto) => {
    expect(valorMonetario(texto)).toBe(1234.56);
  });

  it.each(['', '-1', '1.234', '1,234', '1e5', 'invalido'])(
    'rejeita valor inválido ou ambíguo %s',
    (texto) => {
      expect(valorMonetario(texto)).toBeNull();
    },
  );
});
