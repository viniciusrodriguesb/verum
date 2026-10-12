export interface UsuarioAutenticado {
  readonly sujeito: string;
  readonly email: string | null;
  readonly username: string | null;
  readonly nome: string;
  readonly emailConfirmado: boolean;
}
