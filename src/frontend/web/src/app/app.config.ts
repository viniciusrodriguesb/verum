import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import localePtBr from '@angular/common/locales/pt';
import {
  ApplicationConfig,
  inject,
  LOCALE_ID,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { autenticacaoInterceptor } from './core/autenticacao/interceptors/autenticacao.interceptor';
import { AutenticacaoService } from './core/autenticacao/services/autenticacao.service';
import { ContaService } from './core/autenticacao/services/conta.service';
import { errosApiInterceptor } from './shared/interceptors/erros-api.interceptor';

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
