import { ValidationErrors, ValidatorFn } from '@angular/forms';

const TAMANHO_MINIMO_TERMO = 2;
const TAMANHO_MAXIMO_TERMO = 120;

export function normalizarTermoBusca(valor: string): string {
  return valor.trim().replace(/\s+/g, ' ');
}

export const validarTermoBusca: ValidatorFn = (controle): ValidationErrors | null => {
  const valor = typeof controle.value === 'string' ? controle.value : '';
  const termoNormalizado = normalizarTermoBusca(valor);

  if (!termoNormalizado) {
    return {
      termoObrigatorio: true,
    };
  }

  if (termoNormalizado.length < TAMANHO_MINIMO_TERMO) {
    return {
      termoMuitoCurto: {
        tamanhoMinimo: TAMANHO_MINIMO_TERMO,
        tamanhoAtual: termoNormalizado.length,
      },
    };
  }

  if (termoNormalizado.length > TAMANHO_MAXIMO_TERMO) {
    return {
      termoMuitoLongo: {
        tamanhoMaximo: TAMANHO_MAXIMO_TERMO,
        tamanhoAtual: termoNormalizado.length,
      },
    };
  }

  return null;
};
