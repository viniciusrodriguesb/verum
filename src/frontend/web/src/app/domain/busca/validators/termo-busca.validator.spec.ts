import { FormControl } from '@angular/forms';
import { validarTermoBusca } from './termo-busca.validator';
describe('Contrato da consulta', () => {
  it('aceita 500 caracteres e rejeita 501', () => {
    expect(validarTermoBusca(new FormControl('a'.repeat(500)))).toBeNull();
    expect(validarTermoBusca(new FormControl('a'.repeat(501)))).toHaveProperty('termoMuitoLongo');
  });
});
