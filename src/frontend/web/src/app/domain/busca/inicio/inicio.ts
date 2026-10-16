import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, inject, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  LucideArrowDown,
  LucideArrowRight,
  LucideBadgeCheck,
  LucideBookmark,
  LucideChartNoAxesColumnIncreasing,
  LucideLaptop,
  LucidePackage,
  LucideSearch,
  LucideShieldCheck,
  LucideSmartphone,
  LucideStar,
  LucideStore,
  LucideTag,
  LucideWatch,
} from '@lucide/angular';
import { IniciarBuscaService } from '../services/iniciar-busca.service';

import { IlustracaoRadarComponent } from '@shared/components/ilustracao-radar/ilustracao-radar';

import { Header } from '@core/components/header/header';

import { normalizarTermoBusca, validarTermoBusca } from '../validators/termo-busca.validator';

type IconeEtapa = 'loja' | 'preco' | 'recomendacao';

type IconeProduto = 'celular' | 'eletrodomestico' | 'notebook' | 'relogio';

interface EtapaFuncionamento {
  readonly icone: IconeEtapa;
  readonly titulo: string;
  readonly descricao: string;
}

interface OfertaDestaque {
  readonly id: string;
  readonly nome: string;
  readonly complemento: string;
  readonly preco: string;
  readonly oportunidade: string;
  readonly icone: IconeProduto;
}

@Component({
  providers: [IniciarBuscaService],
  selector: 'verum-inicio',
  standalone: true,
  imports: [
    Header,
    NgClass,
    ReactiveFormsModule,
    IlustracaoRadarComponent,
    RouterLink,
    LucideArrowDown,
    LucideArrowRight,
    LucideBadgeCheck,
    LucideBookmark,
    LucideChartNoAxesColumnIncreasing,
    LucideLaptop,
    LucidePackage,
    LucideSearch,
    LucideShieldCheck,
    LucideSmartphone,
    LucideStar,
    LucideStore,
    LucideTag,
    LucideWatch,
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
      titulo: 'Buscamos em lojas confiáveis',
      descricao:
        'Consultamos diversas lojas parceiras para encontrar as melhores ofertas disponíveis para você.',
    },
    {
      icone: 'preco',
      titulo: 'Verificamos preço e histórico',
      descricao:
        'Analisamos o preço atual, o histórico dos últimos dias e calculamos a média do mercado.',
    },
    {
      icone: 'recomendacao',
      titulo: 'Explicamos a recomendação',
      descricao:
        'Mostramos o contexto, o ranking transparente e indicamos a melhor escolha para você.',
    },
  ];

  protected readonly ofertasDestaque: readonly OfertaDestaque[] = [];

  protected get buscaInvalida(): boolean {
    const campoTermo = this.formularioBusca.controls.termo;

    return campoTermo.invalid && campoTermo.touched;
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
