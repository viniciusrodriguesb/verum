import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, timeout } from 'rxjs';
import { ContaAutenticada } from '../dtos/conta-autenticada.dto';
import { AutenticacaoService } from './autenticacao.service';

@Injectable({ providedIn: 'root' })
export class ContaService {
  private readonly http = inject(HttpClient);

  private readonly autenticacao = inject(AutenticacaoService);

  private readonly contaAtual = signal<ContaAutenticada | null>(null);

  readonly conta = computed(() => {
    const conta = this.contaAtual();

    return conta?.sujeitoIdentidade === this.autenticacao.usuario()?.sujeito ? conta : null;
  });

  obter() {
    return this.http.get<ContaAutenticada>('/api/v1/contas/me').pipe(timeout(15000));
  }

  async sincronizar(): Promise<void> {
    this.contaAtual.set(
      await firstValueFrom(
        this.http.post<ContaAutenticada>('/api/v1/contas/me', null).pipe(timeout(15000)),
      ),
    );
  }
}
