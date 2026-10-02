import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { AvisosApiService } from '../../services/avisos-api.service';

@Component({
  selector: 'verum-aviso-api',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (avisos.mensagem(); as mensagem) {
      <aside
        role="alert"
        class="text-verum-navy fixed right-4 bottom-24 left-4 z-50 flex items-start gap-4 rounded-xl border border-blue-200 bg-white p-4 text-sm shadow-lg sm:left-auto sm:max-w-md lg:bottom-6"
      >
        <p class="flex-1">{{ mensagem }}</p>
        <button
          type="button"
          aria-label="Fechar aviso"
          class="text-verum-blue font-semibold"
          (click)="avisos.fechar()"
        >
          Fechar
        </button>
      </aside>
    }
  `,
})
export class AvisoApiComponent {
  protected readonly avisos = inject(AvisosApiService);
}
