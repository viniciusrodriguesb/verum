import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AvisosApiService {
  readonly mensagem = signal<string | null>(null);

  mostrar(mensagem: string): void {
    this.mensagem.set(mensagem);
  }

  fechar(): void {
    this.mensagem.set(null);
  }
}
