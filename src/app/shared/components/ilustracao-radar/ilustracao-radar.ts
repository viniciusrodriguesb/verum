import { ChangeDetectionStrategy, Component, input } from '@angular/core';

type TamanhoIlustracaoRadar = 'compacto' | 'grande';

@Component({
  selector: 'verum-ilustracao-radar',
  standalone: true,
  templateUrl: './ilustracao-radar.html',
  styleUrl: './ilustracao-radar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IlustracaoRadarComponent {
  readonly tamanho = input<TamanhoIlustracaoRadar>('compacto');

  readonly animada = input(false);
}
