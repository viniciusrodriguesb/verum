import { errosApiInterceptor } from './shared/interceptors/erros-api.interceptor';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideAppInitializer,
  inject,
  LOCALE_ID,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import localePtBr from '@angular/common/locales/pt';
import { routes } from './app.routes';
import { registerLocaleData } from '@angular/common';
import { autenticacaoInterceptor } from './core/autenticacao/interceptors/autenticacao.interceptor';
import { AutenticacaoService } from './core/autenticacao/services/autenticacao.service';
import { ContaService } from './core/autenticacao/services/conta.service';

registerLocaleData(localePtBr);

export const appConfig: ApplicationConfig = {
  providers: [
    provideAppInitializer(() => {
      const autenticacao = inject(AutenticacaoService);

      const conta = inject(ContaService);

      return autenticacao.inicializar().then(async () => {
        if (autenticacao.autenticado()) {
          try {
            await conta.sincronizar();
          } catch {
            // O interceptor apresenta o erro; falha na API não impede a navegação pública.
          }
        }
      });
    }),
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([errosApiInterceptor, autenticacaoInterceptor])),
    provideRouter(routes),
    {
      provide: LOCALE_ID,
      useValue: 'pt-BR',
    },
  ],
};
