import { InjectionToken } from '@angular/core';
import { KeycloakConfig } from 'keycloak-js';

export const AUTENTICACAO_CONFIG = new InjectionToken<KeycloakConfig>('AUTENTICACAO_CONFIG');
