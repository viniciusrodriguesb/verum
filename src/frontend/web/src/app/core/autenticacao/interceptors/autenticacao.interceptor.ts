import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap } from 'rxjs';
import { AutenticacaoService } from '../services/autenticacao.service';

export const autenticacaoInterceptor: HttpInterceptorFn = (request, next) => {
  const destino = new URL(request.url, location.origin);

  if (destino.origin !== location.origin || !destino.pathname.startsWith('/api/')) {
    return next(request);
  }

  const autenticacao = inject(AutenticacaoService);

  return from(autenticacao.obterToken()).pipe(
    switchMap((token) =>
      next(token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request),
    ),
  );
};
