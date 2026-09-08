import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners, LOCALE_ID } from '@angular/core';
import { provideRouter } from '@angular/router';
import localePtBr from '@angular/common/locales/pt';
import { routes } from './app.routes';
import { registerLocaleData } from '@angular/common';

registerLocaleData(localePtBr);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    provideRouter(routes),
    {
      provide: LOCALE_ID,
      useValue: 'pt-BR',
    },
  ],
};
