import { CotaBuscaComponent } from '../components/cota-busca/cota-busca';
import { NavegacaoMobileComponent } from '@core/components/navegacao-mobile/navegacao-mobile';
import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, inject, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  LucideArrowRight,
  LucideBadgeCheck,
  LucideChartNoAxesColumnIncreasing,
  LucideSearch,
  LucideShieldCheck,
  LucideStar,
  LucideStore,
  LucideTag,
} from '@lucide/angular';
import { IniciarBuscaService } from '../services/iniciar-busca.service';

import { IlustracaoRadarComponent } from '@shared/components/ilustracao-radar/ilustracao-radar';

import { Header } from '@core/components/header/header';

import { normalizarTermoBusca, validarTermoBusca } from '../validators/termo-busca.validator';

type IconeEtapa = 'loja' | 'preco' | 'recomendacao';

interface EtapaFuncionamento {
  readonly icone: IconeEtapa;
  readonly titulo: string;
  readonly descricao: string;
}

@Component({
  providers: [IniciarBuscaService],
  selector: 'verum-inicio',
  standalone: true,
  imports: [
    Header,
    CotaBuscaComponent,
    NavegacaoMobileComponent,
    NgClass,
    ReactiveFormsModule,
    IlustracaoRadarComponent,
    RouterLink,

    LucideArrowRight,
    LucideBadgeCheck,

    LucideChartNoAxesColumnIncreasing,

    LucideSearch,
    LucideShieldCheck,

    LucideStar,
    LucideStore,
    LucideTag,
  ],
  host: {
    class: 'block min-h-dvh bg-verum-surface',
  },
  templateUrl: './inicio.html',
  styleUrl: './inicio.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InicioComponent {
  protected readonly inicioBusca = inject(IniciarBuscaService);

  private readonly inputBusca = viewChild.required<ElementRef<HTMLInputElement>>('inputBusca');

  protected readonly formularioBusca = new FormGroup({
    termo: new FormControl('', {
      nonNullable: true,
      validators: [validarTermoBusca],
    }),
  });

  protected readonly etapasFuncionamento: readonly EtapaFuncionamento[] = [
    {
      icone: 'loja',
      titulo: 'Consultamos fontes identificadas',
      descricao: 'Consultamos as fontes disponíveis para o produto e modelo que você procura.',
    },
    {
      icone: 'preco',
      titulo: 'Verificamos preço e histórico',
      descricao:
        'Comparamos preços da mesma variante e usamos histórico quando há dados suficientes.',
    },
    {
      icone: 'recomendacao',
      titulo: 'Explicamos a recomendação',
      descricao:
        'Organizamos as ofertas por correspondência, preço, confiança da fonte e atualização.',
    },
  ];

  protected get buscaInvalida(): boolean {
    const campoTermo = this.formularioBusca.controls.termo;

    return campoTermo.invalid && campoTermo.touched;
  }

  protected get buscaBloqueada(): boolean {
    return this.inicioBusca.bloqueada(
      normalizarTermoBusca(this.formularioBusca.controls.termo.value),
    );
  }

  protected focarBusca(): void {
    this.focarCampoBusca();
    this.inputBusca().nativeElement.scrollIntoView({ block: 'center', behavior: 'instant' });
  }

  protected buscar(): void {
    this.formularioBusca.markAllAsTouched();

    if (this.formularioBusca.invalid) {
      this.focarCampoBusca();
      return;
    }

    const termoNormalizado = normalizarTermoBusca(this.formularioBusca.controls.termo.value);

    this.inicioBusca.iniciar(termoNormalizado);
  }

  protected obterMensagemErroBusca(): string | null {
    const campoTermo = this.formularioBusca.controls.termo;

    if (campoTermo.hasError('termoObrigatorio')) {
      return 'Informe o produto que você deseja encontrar.';
    }

    if (campoTermo.hasError('termoMuitoCurto')) {
      return 'Digite pelo menos 2 caracteres para realizar a busca.';
    }

    if (campoTermo.hasError('termoMuitoLongo')) {
      return 'A pesquisa deve ter no máximo 500 caracteres.';
    }

    return null;
  }

  private focarCampoBusca(): void {
    this.inputBusca().nativeElement.focus();
  }
}
