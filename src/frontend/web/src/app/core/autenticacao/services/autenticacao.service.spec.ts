import { TestBed } from '@angular/core/testing';
import { AvisosApiService } from '@shared/services/avisos-api.service';
import { vi } from 'vitest';
import { AutenticacaoService } from './autenticacao.service';

const { cliente } = vi.hoisted(() => ({
  cliente: {
    authenticated: false,
    token: undefined as string | undefined,
    tokenParsed: undefined as Record<string, unknown> | undefined,
    init: vi.fn(),
    updateToken: vi.fn(),
    clearToken: vi.fn(),
    login: vi.fn(),
    logout: vi.fn(),
    register: vi.fn(),
    accountManagement: vi.fn(),
    onAuthLogout: undefined as (() => void) | undefined,
  },
}));

vi.mock('keycloak-js', () => ({
  default: class {
    constructor() {
      return cliente;
    }
  },
}));

describe('AutenticacaoService', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    cliente.authenticated = false;

    cliente.token = undefined;

    cliente.tokenParsed = undefined;

    cliente.init.mockResolvedValue(false);

    cliente.updateToken.mockResolvedValue(false);

    cliente.clearToken.mockImplementation(() => {
      cliente.authenticated = false;

      cliente.token = undefined;

      cliente.onAuthLogout?.();
    });
  });

  it('permite visitante e informa indisponibilidade sem bloquear a inicialização', async () => {
    cliente.init.mockRejectedValue(new Error('offline'));

    const service = TestBed.inject(AutenticacaoService);

    await service.inicializar();

    expect(service.usuario()).toBeNull();

    expect(await service.obterToken()).toBeNull();

    expect(TestBed.inject(AvisosApiService).mensagem()).toContain('visitante');
  });

  it('extrai as claims e renova o token antes de uma requisição', async () => {
    cliente.authenticated = true;

    cliente.tokenParsed = {
      sub: 'usuario-real',
      name: 'Maria',
      email: 'maria@example.test',
      email_verified: true,
    };

    cliente.token = 'token-renovado';

    const service = TestBed.inject(AutenticacaoService);

    await service.inicializar();

    expect(service.nome()).toBe('Maria');

    expect(service.usuario()?.emailConfirmado).toBe(true);

    expect(await service.obterToken()).toBe('token-renovado');

    expect(cliente.updateToken).toHaveBeenCalledWith(30);
  });

  it('limpa identidade e impede envio de token quando a renovação falha', async () => {
    cliente.authenticated = true;

    cliente.tokenParsed = { sub: 'usuario-real' };

    const service = TestBed.inject(AutenticacaoService);

    await service.inicializar();

    cliente.updateToken.mockRejectedValue(new Error('expired'));

    await expect(service.obterToken()).rejects.toMatchObject({ status: 401 });

    expect(service.autenticado()).toBe(false);
  });
});
