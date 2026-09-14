import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { CurrencyPipe, DecimalPipe, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  LucideBadgeCheck,
  LucideChevronDown,
  LucideClock,
  LucideHeart,
  LucideRadar,
  LucideShieldCheck,
  LucideSmartphone,
  LucideStar,
  LucideTag,
  LucideTruck,
} from '@lucide/angular';
import { DestaqueOferta, OfertaResultado } from '../oferta-resultado.model';

const ROTULOS_DESTAQUE: Readonly<Record<DestaqueOferta, string>> = {
  'melhor-escolha': 'Melhor escolha',
  'mais-confiavel': 'Mais confiável',
  'menor-preco': 'Menor preço',
};

@Component({
  selector: 'verum-cartao-oferta',
  standalone: true,
  imports: [
    CurrencyPipe,
    DecimalPipe,
    NgClass,
    RouterLink,
    LucideBadgeCheck,
    LucideChevronDown,
    LucideClock,
    LucideHeart,
    LucideRadar,
    LucideShieldCheck,
    LucideSmartphone,
    LucideStar,
    LucideTag,
    LucideTruck,
  ],
  host: {
    class: 'block',
  },
  templateUrl: './cartao-oferta.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartaoOfertaComponent {
  readonly oferta = input.required<OfertaResultado>();
  readonly favorita = input(false);

  readonly favoritoAlterado = output<string>();

  protected readonly explicacaoAberta = signal(false);

  protected readonly rotuloDestaque = computed(() => ROTULOS_DESTAQUE[this.oferta().destaque]);

  protected alternarFavorito(): void {
    this.favoritoAlterado.emit(this.oferta().id);
  }

  protected alternarExplicacao(): void {
    this.explicacaoAberta.update((aberta) => !aberta);
  }
}
