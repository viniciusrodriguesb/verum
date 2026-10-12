import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AutenticacaoService } from '../services/autenticacao.service';

export const autenticadoGuard: CanActivateFn = async (_route, estado) => {
  const autenticacao = inject(AutenticacaoService);

  if (autenticacao.autenticado()) return true;

  await autenticacao.entrar(`${location.origin}${estado.url}`);

  return false;
};
