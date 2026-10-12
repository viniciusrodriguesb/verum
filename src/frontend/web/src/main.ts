import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { AUTENTICACAO_CONFIG } from './app/core/autenticacao/configuracoes/autenticacao.config';

async function iniciar(): Promise<void> {
  const resposta = await fetch('/configuracao.json', { cache: 'no-store' });

  if (!resposta.ok) throw new Error('Não foi possível carregar a configuração da aplicação.');

  const configuracao = await resposta.json();

  if (
    !configuracao.keycloak?.url ||
    !configuracao.keycloak?.realm ||
    !configuracao.keycloak?.clientId
  ) {
    throw new Error('Configure url, realm e clientId do Keycloak.');
  }

  await bootstrapApplication(App, {
    ...appConfig,
    providers: [
      ...appConfig.providers,
      { provide: AUTENTICACAO_CONFIG, useValue: configuracao.keycloak },
    ],
  });
}

iniciar().catch((err) => console.error(err));
