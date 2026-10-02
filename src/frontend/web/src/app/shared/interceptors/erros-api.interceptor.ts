import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { AvisosApiService } from '../services/avisos-api.service';
import { mensagemErroApi } from '../utils/mensagem-erro-api';

export const errosApiInterceptor: HttpInterceptorFn = (requisicao, next) => {
  const avisos = inject(AvisosApiService);

  return next(requisicao).pipe(
    catchError((erro: unknown) => {
      avisos.mostrar(mensagemErroApi(erro));

      return throwError(() => erro);
    }),
  );
};
