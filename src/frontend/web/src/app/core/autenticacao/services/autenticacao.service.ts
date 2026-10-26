import { HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { AvisosApiService } from '@shared/services/avisos-api.service';
import { CLIENTE_KEYCLOAK } from '../configuracoes/cliente-keycloak.config';
import { UsuarioAutenticado } from '../models/usuario-autenticado';

@Injectable({ providedIn: 'root' })
export class AutenticacaoService {
  private readonly cliente = inject(CLIENTE_KEYCLOAK);

  private readonly avisos = inject(AvisosApiService);

  private readonly usuarioAtual = signal<UsuarioAutenticado | null>(null);

  readonly usuario = this.usuarioAtual.asReadonly();

  readonly autenticado = computed(() => this.usuario() !== null);

  readonly nome = computed(() => this.usuario()?.nome ?? 'Visitante');

  async inicializar(): Promise<void> {
    this.cliente.onAuthSuccess = () => this.atualizarUsuario();

    this.cliente.onAuthRefreshSuccess = () => this.atualizarUsuario();

    this.cliente.onAuthLogout = () => this.usuarioAtual.set(null);

    this.cliente.onAuthRefreshError = () => this.cliente.clearToken();

    try {
      await this.cliente.init({
        onLoad: 'check-sso',
        pkceMethod: 'S256',
        locale: 'pt-BR',
        silentCheckSsoRedirectUri: `${location.origin}/silent-check-sso.html`,
        silentCheckSsoFallback: false,
        checkLoginIframe: false,
        messageReceiveTimeout: 5000,
      });

      this.atualizarUsuario();
    } catch {
      this.cliente.clearToken();

      this.avisos.mostrar(
        'Não foi possível conectar ao login. Você pode continuar buscando como visitante.',
      );
    }
  }

  async entrar(redirectUri = location.href): Promise<void> {
    await this.executar(() => this.cliente.login({ redirectUri, locale: 'pt-BR' }));
  }

  async cadastrar(): Promise<void> {
    await this.executar(() =>
      this.cliente.register({ redirectUri: location.href, locale: 'pt-BR' }),
    );
  }

  async sair(): Promise<void> {
    await this.executar(() => this.cliente.logout({ redirectUri: location.origin }));
  }

  async gerenciarConta(): Promise<void> {
    await this.executar(() =>
      this.autenticado()
        ? this.cliente.accountManagement()
        : this.cliente.login({ redirectUri: location.href, locale: 'pt-BR' }),
    );
  }

  async obterToken(): Promise<string | null> {
    if (!this.cliente.authenticated) return null;

    try {
      await this.cliente.updateToken(30);

      return this.cliente.token ?? null;
    } catch {
      this.cliente.clearToken();

      throw new HttpErrorResponse({ status: 401, statusText: 'Sessão expirada' });
    }
  }

  private atualizarUsuario(): void {
    const token = this.cliente.tokenParsed;

    this.usuarioAtual.set(
      this.cliente.authenticated && token?.sub
        ? {
            sujeito: token.sub,
            email: token['email'] ?? null,
            username: token['preferred_username'] ?? null,
            nome: token['name'] || token['preferred_username'] || token['email'] || 'Usuário',
            emailConfirmado: token['email_verified'] === true,
          }
        : null,
    );
  }

  private async executar(acao: () => Promise<void>): Promise<void> {
    try {
      await acao();
    } catch {
      this.avisos.mostrar('Não foi possível acessar o login. Tente novamente.');
    }
  }
}
