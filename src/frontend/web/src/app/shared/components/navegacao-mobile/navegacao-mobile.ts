import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import {
  LucideCircleUserRound,
  LucideHistory,
  LucideHouse,
  LucideRadar,
  LucideSearch,
} from '@lucide/angular';

type IconeNavegacao = 'inicio' | 'busca' | 'radar' | 'historico' | 'conta';

interface ItemNavegacao {
  readonly icone: IconeNavegacao;
  readonly rotulo: string;
  readonly rota: string;
  readonly correspondenciaExata: boolean;
}

@Component({
  selector: 'verum-navegacao-mobile',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    LucideHouse,
    LucideSearch,
    LucideRadar,
    LucideHistory,
    LucideCircleUserRound,
  ],
  templateUrl: './navegacao-mobile.html',
  styleUrl: './navegacao-mobile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavegacaoMobileComponent {
  protected readonly itensNavegacao: readonly ItemNavegacao[] = [
    {
      icone: 'inicio',
      rotulo: 'Início',
      rota: '/',
      correspondenciaExata: true,
    },
    {
      icone: 'busca',
      rotulo: 'Buscar',
      rota: '/busca',
      correspondenciaExata: false,
    },
    {
      icone: 'radar',
      rotulo: 'Radar',
      rota: '/radar',
      correspondenciaExata: false,
    },
    {
      icone: 'historico',
      rotulo: 'Histórico',
      rota: '/historico',
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
