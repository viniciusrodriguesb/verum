import { RouterLink } from '@angular/router';
import { CurrencyPipe, DecimalPipe, NgClass } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import {
  LucideBadgeCheck,
  LucideClock,
  LucideRadar,
  LucideShieldCheck,
  LucideSmartphone,
  LucideStar,
  LucideTag,
  LucideTruck,
} from '@lucide/angular';
import { DestaqueOferta, OfertaResultado } from '../models/oferta-resultado.model';

const ROTULOS_DESTAQUE: Readonly<Record<DestaqueOferta, string>> = {
  comum: 'Oferta correspondente',
  'melhor-escolha': 'Oferta encontrada',
  'mais-confiavel': 'Mais confiável',
  'menor-preco': 'Menor preço',
};

@Component({
  selector: 'verum-cartao-oferta',
  standalone: true,
  imports: [
    RouterLink,
    CurrencyPipe,
    DecimalPipe,
    NgClass,
    LucideBadgeCheck,
    LucideClock,
    LucideRadar,
    LucideShieldCheck,
    LucideSmartphone,
    LucideStar,
    LucideTag,
    LucideTruck,
  ],
  host: {
    class: 'block min-w-0',
    '(document:click)': 'fecharAoClicarFora($event)',
    '(document:keydown.escape)': 'fecharExplicacao()',
  },
  templateUrl: './cartao-oferta.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartaoOfertaComponent {
  private readonly elemento = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly oferta = input.required<OfertaResultado>();

  protected readonly explicacaoAberta = signal(false);

  protected readonly rotuloDestaque = computed(() => ROTULOS_DESTAQUE[this.oferta().destaque]);

  protected fecharAoClicarFora(evento: MouseEvent): void {
    if (!this.elemento.nativeElement.contains(evento.target as Node)) {
      this.fecharExplicacao();
    }
  }

  protected fecharExplicacao(): void {
    this.explicacaoAberta.set(false);
  }

  protected alternarExplicacao(): void {
    this.explicacaoAberta.update((aberta) => !aberta);
  }
}
