import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

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
  protected readonly itensMenu: readonly ItemMenu[] = [
    {
      rotulo: 'Produtos',
      rota: '/',
      fragmento: 'produtos',
    },
    {
      rotulo: 'Como funciona',
      rota: '/',
      fragmento: 'como-funciona',
    },
    {
      rotulo: 'Para empresas',
      rota: '/para-empresas',
    },
  ];
}
