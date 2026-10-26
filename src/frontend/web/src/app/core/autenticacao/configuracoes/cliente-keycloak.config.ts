import { inject, InjectionToken } from '@angular/core';
import Keycloak from 'keycloak-js';
import { AUTENTICACAO_CONFIG } from './autenticacao.config';

export const CLIENTE_KEYCLOAK = new InjectionToken<Keycloak>('CLIENTE_KEYCLOAK', {
  providedIn: 'root',
  factory: () => new Keycloak(inject(AUTENTICACAO_CONFIG)),
});
