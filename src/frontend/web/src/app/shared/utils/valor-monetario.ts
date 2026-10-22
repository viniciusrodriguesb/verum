/** Aceita BRL colado e decimal com vírgula ou ponto; valores ambíguos são inválidos. */
export function valorMonetario(texto: string): number | null {
  const valor = texto
    .trim()
    .replace(/^R\$\s*/, '')
    .replace(/\s/g, '');

  if (!valor) return null;

  const brasileiro = /^\d{1,3}(?:\.\d{3})+,\d{1,2}$/.test(valor);

  const simples = /^\d+(?:[.,]\d{1,2})?$/.test(valor);

  if (!brasileiro && !simples) return null;

  const numero = Number((brasileiro ? valor.replaceAll('.', '') : valor).replace(',', '.'));

  return Number.isFinite(numero) && numero > 0 ? numero : null;
}
