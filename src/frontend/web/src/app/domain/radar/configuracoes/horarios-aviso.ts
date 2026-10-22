export const HORAS_AVISO = Array.from({ length: 24 }, (_, hora) => hora);

export const FUSOS_AVISO = [
  { valor: 'America/Sao_Paulo', nome: 'Horário de Brasília' },
  { valor: 'America/Manaus', nome: 'Horário de Manaus' },
  { valor: 'America/Rio_Branco', nome: 'Horário do Acre' },
  { valor: 'America/Noronha', nome: 'Fernando de Noronha' },
  { valor: 'UTC', nome: 'UTC' },
] as const;
