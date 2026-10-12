import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { finalize, switchMap, TimeoutError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

import { AvisosApiService } from '@shared/services/avisos-api.service';
import { mensagemErroApi } from '@shared/utils/mensagem-erro-api';

import { BuscasService } from './buscas.service';
import { SolicitacoesPendentesService } from './solicitacoes-pendentes.service';

@Injectable()
export class IniciarBuscaService {
  private readonly buscas = inject(BuscasService);

  private readonly roteador = inject(Router);

  private readonly avisos = inject(AvisosApiService);

  private readonly destroyRef = inject(DestroyRef);

  private readonly pendentes = inject(SolicitacoesPendentesService);

  readonly enviando = signal(false);

  iniciar(consulta: string): void {
    if (this.enviando()) return;

    this.enviando.set(true);

    this.avisos.fechar();

    const pedido = this.pendentes.obter(consulta);

    this.buscas
      .prepararVisitante()
      .pipe(
        switchMap(() => this.buscas.solicitar(pedido)),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.enviando.set(false)),
      )
      .subscribe({
        next: (aceita) => {
          this.pendentes.concluir(consulta);

          void this.roteador.navigate(['/busca/processamento'], {
            queryParams: { q: consulta, id: aceita.id },
          });
        },
        error: (erro: unknown) => {
          const incerta =
            erro instanceof TimeoutError ||
            (erro instanceof HttpErrorResponse && (erro.status === 0 || erro.status >= 500));

          this.avisos.mostrar(
            incerta
              ? 'Não foi possível confirmar a solicitação. Tente novamente a mesma busca para recuperá-la sem duplicar o pedido.'
              : mensagemErroApi(erro),
          );
        },
      });
  }
}
