import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { CurrencyPipe, DecimalPipe, NgClass } from '@angular/common';
import {
  LucideBadgeCheck,
  LucideClock,
  LucideHeart,
  LucideRadar,
  LucideShieldCheck,
  LucideSmartphone,
  LucideStar,
  LucideTag,
  LucideTruck,
} from '@lucide/angular';
import { DestaqueOferta, OfertaResultado } from '../models/oferta-resultado.model';

const ROTULOS_DESTAQUE: Readonly<Record<DestaqueOferta, string>> = {
  'melhor-escolha': 'Oferta encontrada',
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
    LucideBadgeCheck,
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
  readonly favorita = input(false);

  readonly favoritoAlterado = output<string>();

  protected readonly explicacaoAberta = signal(false);

  protected readonly rotuloDestaque = computed(() => ROTULOS_DESTAQUE[this.oferta().destaque]);

  protected alternarFavorito(): void {
    this.favoritoAlterado.emit(this.oferta().id);
  }

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
