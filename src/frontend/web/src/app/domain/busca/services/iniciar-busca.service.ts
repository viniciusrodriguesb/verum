import { HttpErrorResponse } from '@angular/common/http';
import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { finalize, switchMap, TimeoutError } from 'rxjs';

import { AvisosApiService } from '@shared/services/avisos-api.service';
import { mensagemErroApi } from '@shared/utils/mensagem-erro-api';

import { BuscasService } from './buscas.service';
import { SolicitacoesPendentesService } from './solicitacoes-pendentes.service';
import { AcessoBusca } from '../dtos/acesso-busca.dto';

@Injectable()
export class IniciarBuscaService {
  private readonly buscas = inject(BuscasService);

  private readonly roteador = inject(Router);

  private readonly avisos = inject(AvisosApiService);

  private readonly destroyRef = inject(DestroyRef);

  private readonly pendentes = inject(SolicitacoesPendentesService);

  readonly enviando = signal(false);

  readonly acesso = signal<AcessoBusca | null>(null);

  readonly erro = signal<string | null>(null);

  readonly respostaIncerta = signal(false);

  readonly atualizandoAcesso = signal(false);

  readonly limiteAtingido = computed(() => this.acesso()?.restante === 0);

  bloqueada(consulta: string): boolean {
    return this.limiteAtingido() && !this.pendentes.existe(consulta);
  }

  consultarAcesso(): void {
    if (this.atualizandoAcesso()) return;

    this.atualizandoAcesso.set(true);

    this.buscas
      .obterAcesso()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.atualizandoAcesso.set(false)),
      )
      .subscribe({
        next: (acesso) => {
          const reposto = this.limiteAtingido() && acesso.restante > 0;

          this.acesso.set(acesso);

          if (reposto && !this.respostaIncerta()) this.erro.set(null);
        },
        error: () => {
          /* A API continua validando o limite quando a consulta de saldo falha. */
        },
      });
  }

  iniciar(consulta: string): void {
    if (this.enviando() || this.bloqueada(consulta)) return;

    this.enviando.set(true);

    this.avisos.fechar();

    this.erro.set(null);

    this.respostaIncerta.set(false);

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

          const mensagem = incerta
            ? 'Não foi possível confirmar a solicitação. Tente novamente a mesma busca para recuperá-la sem duplicar o pedido.'
            : mensagemErroApi(erro);

          this.erro.set(mensagem);

          this.respostaIncerta.set(incerta);

          this.avisos.mostrar(mensagem);

          if (!incerta) this.pendentes.concluir(consulta);

          if (erro instanceof HttpErrorResponse && erro.status === 429) this.consultarAcesso();
        },
      });
  }
}
