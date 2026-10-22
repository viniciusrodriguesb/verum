import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideCircleUserRound, LucideRadar, LucideSearch } from '@lucide/angular';

type IconeNavegacao = 'busca' | 'radar' | 'conta';

interface ItemNavegacao {
  readonly icone: IconeNavegacao;
  readonly rotulo: string;
  readonly rota: string;
  readonly correspondenciaExata: boolean;
}

@Component({
  selector: 'verum-navegacao-mobile',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LucideSearch, LucideRadar, LucideCircleUserRound],
  templateUrl: './navegacao-mobile.html',
  styleUrl: './navegacao-mobile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavegacaoMobileComponent {
  protected readonly itensNavegacao: readonly ItemNavegacao[] = [
    {
      icone: 'busca',
      rotulo: 'Buscar',
      rota: '/',
      correspondenciaExata: true,
    },
    {
      icone: 'radar',
      rotulo: 'Radar',
      rota: '/radar',
      correspondenciaExata: false,
    },
    {
      icone: 'conta',
      rotulo: 'Conta',
      rota: '/conta',
      correspondenciaExata: false,
    },
  ];
}
