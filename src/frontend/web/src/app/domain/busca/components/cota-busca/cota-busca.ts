import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { timer } from 'rxjs';
import { IniciarBuscaService } from '../../services/iniciar-busca.service';

@Component({
  selector: 'verum-cota-busca',
  imports: [DatePipe, RouterLink],
  templateUrl: './cota-busca.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CotaBuscaComponent {
  protected readonly inicio = inject(IniciarBuscaService);

  protected readonly autenticacao = inject(AutenticacaoService);

  constructor() {
    effect(() => {
      const sujeito = this.autenticacao.usuario()?.sujeito;

      untracked(() => {
        this.inicio.acesso.set(null);

        if (sujeito) this.inicio.consultarAcesso();
      });
    });

    timer(60000, 60000)
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        const acesso = this.inicio.acesso();

        if (acesso && Date.parse(acesso.reiniciaEm) <= Date.now()) this.inicio.consultarAcesso();
      });
  }
}
