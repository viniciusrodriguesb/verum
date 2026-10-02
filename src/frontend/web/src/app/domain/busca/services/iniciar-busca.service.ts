import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AvisosApiService } from '@shared/services/avisos-api.service';
import { mensagemErroApi } from '@shared/utils/mensagem-erro-api';

import { BuscasService } from './buscas.service';

@Injectable()
export class IniciarBuscaService {
  private readonly buscas = inject(BuscasService);

  private readonly roteador = inject(Router);

  private readonly avisos = inject(AvisosApiService);

  private readonly destroyRef = inject(DestroyRef);

  readonly enviando = signal(false);

  iniciar(consulta: string): void {
    if (this.enviando()) return;

    this.enviando.set(true);

    this.avisos.fechar();

    this.buscas
      .solicitar({ consulta, chaveIdempotencia: crypto.randomUUID() })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.enviando.set(false)),
      )
      .subscribe({
        next: (aceita) => {
          void this.roteador.navigate(['/busca/processamento'], {
            queryParams: { q: consulta, id: aceita.id },
          });
        },
        error: (erro: unknown) => this.avisos.mostrar(mensagemErroApi(erro)),
      });
  }
}
