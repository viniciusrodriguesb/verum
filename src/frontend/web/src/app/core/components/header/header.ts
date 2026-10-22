import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';

interface ItemMenu {
  readonly rotulo: string;
  readonly rota: string;
  readonly fragmento?: string;
}

@Component({
  selector: 'verum-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  protected readonly autenticacao = inject(AutenticacaoService);

  protected readonly itensMenu: readonly ItemMenu[] = [
    {
      rotulo: 'Buscar preços',
      rota: '/',
    },
    {
      rotulo: 'Verum Radar',
      rota: '/radar',
    },
    {
      rotulo: 'Como funciona',
      rota: '/',
      fragmento: 'como-funciona',
    },
  ];
}
