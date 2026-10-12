import { InjectionToken } from '@angular/core';
import { KeycloakConfig } from 'keycloak-js';

export const AUTENTICACAO_CONFIG = new InjectionToken<KeycloakConfig>('AUTENTICACAO_CONFIG', {
  providedIn: 'root',
  factory: () => ({ url: 'http://localhost:8081', realm: 'verum', clientId: 'verum-web' }),
});
