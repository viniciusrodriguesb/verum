import { NgClass } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideArrowLeft,
  LucideCircleDollarSign,
  LucideClock,
  LucideSearch,
  LucideShieldCheck,
  LucideSlidersHorizontal,
  LucideSparkles,
  LucideStar,
  LucideTag,
  LucideTruck,
} from '@lucide/angular';
import { distinctUntilChanged, map, startWith } from 'rxjs';

import { Header } from '@shared/components/header/header';
import { NavegacaoMobileComponent } from '@shared/components/navegacao-mobile/navegacao-mobile';

import { OfertaResultado } from '../models/oferta-resultado.model';
import { normalizarTermoBusca, validarTermoBusca } from '../validators/termo-busca.validator';
import { CartaoOfertaComponent } from '../cartao-oferta/cartao-oferta';

type OrdemResultado = 'melhor-escolha' | 'menor-preco' | 'mais-confiavel';

type IconeOrdenacao = 'melhor-escolha' | 'menor-preco' | 'mais-confiavel';

interface OpcaoOrdenacao {
  readonly codigo: OrdemResultado;
  readonly rotulo: string;
  readonly icone: IconeOrdenacao;
}

@Component({
  selector: 'verum-resultado-busca',
  standalone: true,
  imports: [
    Header,
    NavegacaoMobileComponent,
    CartaoOfertaComponent,
    NgClass,
    ReactiveFormsModule,
    LucideArrowLeft,
    LucideCircleDollarSign,
    LucideClock,
    LucideSearch,
    LucideShieldCheck,
    LucideSlidersHorizontal,
    LucideSparkles,
    LucideStar,
    LucideTag,
    LucideTruck,
  ],
  host: {
    class:
      'block min-h-dvh bg-white pb-[calc(66px+env(safe-area-inset-bottom))] text-verum-navy lg:pb-0',
  },
  templateUrl: './resultados.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResultadosComponent {
  private readonly rota = inject(ActivatedRoute);
  private readonly roteador = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly inputBuscaDesktop =
    viewChild.required<ElementRef<HTMLInputElement>>('inputBuscaDesktop');

  protected readonly formularioBusca = new FormGroup({
    termo: new FormControl('', {
      nonNullable: true,
      validators: [validarTermoBusca],
    }),
  });

  protected readonly formularioFiltros = new FormGroup({
    precoMinimo: new FormControl<number | null>(null),
    precoMaximo: new FormControl<number | null>(null),
    freteGratis: new FormControl(false, {
      nonNullable: true,
    }),
    somenteLojasConfiaveis: new FormControl(false, {
      nonNullable: true,
    }),
    somenteProdutosNovos: new FormControl(true, {
      nonNullable: true,
    }),
  });

  private readonly filtrosAtuais = toSignal(
    this.formularioFiltros.valueChanges.pipe(startWith(this.formularioFiltros.getRawValue())),
    {
      initialValue: this.formularioFiltros.getRawValue(),
    },
  );

  private readonly ordemAtual = signal<OrdemResultado>('melhor-escolha');

  private readonly favoritos = signal<ReadonlySet<string>>(new Set());

  protected readonly totalOfertasAnalisadas = 57;

  protected readonly opcoesOrdenacao: readonly OpcaoOrdenacao[] = [
    {
      codigo: 'melhor-escolha',
      rotulo: 'Melhor escolha',
      icone: 'melhor-escolha',
    },
    {
      codigo: 'menor-preco',
      rotulo: 'Menor preço',
      icone: 'menor-preco',
    },
    {
      codigo: 'mais-confiavel',
      rotulo: 'Mais confiável',
      icone: 'mais-confiavel',
    },
  ];

  private readonly ofertas: readonly OfertaResultado[] = [
    {
      id: 'magazine-luiza-iphone-16',
      produtoId: 'iphone-16-256-gb',
      produto: 'iPhone 16 256 GB',
      fabricante: 'Apple',
      preco: 4499,
      quantidadeParcelas: 12,
      valorParcela: 374.92,
      freteGratis: true,
      produtoNovo: true,
      verificadaHaMinutos: 10,
      pontuacaoRanking: 9.6,
      destaque: 'melhor-escolha',
      loja: {
        codigo: 'magazine-luiza',
        nome: 'Magazine Luiza',
        confiavel: true,
      },
      avaliacao: {
        preco: 9.6,
        confianca: 9.2,
        atualizacao: 9.8,
        resumo: 'Bom equilíbrio entre preço, reputação da loja e atualização da oferta.',
      },
      imagemUrl: null,
      urlOferta: 'https://www.magazineluiza.com.br/',
    },
    {
      id: 'amazon-iphone-16',
      produtoId: 'iphone-16-256-gb',
      produto: 'iPhone 16 256 GB',
      fabricante: 'Apple',
      preco: 4589,
      quantidadeParcelas: 12,
      valorParcela: 382.42,
      freteGratis: true,
      produtoNovo: true,
      verificadaHaMinutos: 25,
      pontuacaoRanking: 9.4,
      destaque: 'mais-confiavel',
      loja: {
        codigo: 'amazon',
        nome: 'Amazon.com.br',
        confiavel: true,
      },
      avaliacao: {
        preco: 9.1,
        confianca: 9.8,
        atualizacao: 9.5,
        resumo: 'Loja com excelente reputação e oferta verificada recentemente.',
      },
      imagemUrl: null,
      urlOferta: 'https://www.amazon.com.br/',
    },
    {
      id: 'kabum-iphone-16',
      produtoId: 'iphone-16-256-gb',
      produto: 'iPhone 16 256 GB',
      fabricante: 'Apple',
      preco: 4399,
      quantidadeParcelas: 12,
      valorParcela: 366.58,
      freteGratis: false,
      produtoNovo: true,
      verificadaHaMinutos: 40,
      pontuacaoRanking: 9.1,
      destaque: 'menor-preco',
      loja: {
        codigo: 'kabum',
        nome: 'KaBuM!',
        confiavel: true,
      },
      avaliacao: {
        preco: 9.9,
        confianca: 9.5,
        atualizacao: 9,
        resumo: 'Menor preço entre as ofertas selecionadas, em uma loja confiável.',
      },
      imagemUrl: null,
      urlOferta: 'https://www.kabum.com.br/',
    },
  ];

  protected readonly ofertasExibidas = computed(() => {
    const filtros = this.filtrosAtuais();

    const ofertasFiltradas = this.ofertas.filter((oferta) => {
      if (
        filtros.precoMinimo !== null &&
        filtros.precoMinimo !== undefined &&
        oferta.preco < filtros.precoMinimo
      ) {
        return false;
      }

      if (
        filtros.precoMaximo !== null &&
        filtros.precoMaximo !== undefined &&
        oferta.preco > filtros.precoMaximo
      ) {
        return false;
      }

      if (filtros.freteGratis && !oferta.freteGratis) {
        return false;
      }

      if (filtros.somenteLojasConfiaveis && !oferta.loja.confiavel) {
        return false;
      }

      if (filtros.somenteProdutosNovos && !oferta.produtoNovo) {
        return false;
      }

      return true;
    });

    return this.ordenarOfertas(ofertasFiltradas, this.ordemAtual());
  });

  protected readonly possuiFiltrosAtivos = computed(() => {
    const filtros = this.filtrosAtuais();

    return (
      filtros.precoMinimo !== null ||
      filtros.precoMaximo !== null ||
      Boolean(filtros.freteGratis) ||
      Boolean(filtros.somenteLojasConfiaveis) ||
      Boolean(filtros.somenteProdutosNovos)
    );
  });

  constructor() {
    this.observarTermoDaRota();
  }

  protected get buscaInvalida(): boolean {
    const campoTermo = this.formularioBusca.controls.termo;

    return campoTermo.invalid && campoTermo.touched;
  }

  protected ordemSelecionada(): OrdemResultado {
    return this.ordemAtual();
  }

  protected selecionarOrdenacao(ordem: OrdemResultado): void {
    this.ordemAtual.set(ordem);
  }

  protected buscarNovamente(): void {
    this.formularioBusca.markAllAsTouched();

    if (this.formularioBusca.invalid) {
      this.inputBuscaDesktop().nativeElement.focus();
      return;
    }

    const termoNormalizado = normalizarTermoBusca(this.formularioBusca.controls.termo.value);

    void this.roteador.navigate(['/busca/processamento'], {
      queryParams: {
        q: termoNormalizado,
      },
    });
  }

  protected voltarParaBusca(): void {
    void this.roteador.navigate(['/']);
  }

  protected limparFiltros(): void {
    this.formularioFiltros.reset({
      precoMinimo: null,
      precoMaximo: null,
      freteGratis: false,
      somenteLojasConfiaveis: false,
      somenteProdutosNovos: false,
    });
  }

  protected alternarFavorito(ofertaId: string): void {
    this.favoritos.update((favoritosAtuais) => {
      const novosFavoritos = new Set(favoritosAtuais);

      if (novosFavoritos.has(ofertaId)) {
        novosFavoritos.delete(ofertaId);
      } else {
        novosFavoritos.add(ofertaId);
      }

      return novosFavoritos;
    });
  }

  protected ofertaFavorita(ofertaId: string): boolean {
    return this.favoritos().has(ofertaId);
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
      return 'A pesquisa deve ter no máximo 120 caracteres.';
    }

    return null;
  }

  private observarTermoDaRota(): void {
    this.rota.queryParamMap
      .pipe(
        map((parametros) => parametros.get('q')?.trim() ?? ''),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((termo) => {
        if (!termo) {
          void this.roteador.navigate(['/']);
          return;
        }

        this.formularioBusca.controls.termo.setValue(termo, {
          emitEvent: false,
        });
      });
  }

  private ordenarOfertas(
    ofertas: readonly OfertaResultado[],
    ordem: OrdemResultado,
  ): readonly OfertaResultado[] {
    const ofertasOrdenadas = [...ofertas];

    switch (ordem) {
      case 'menor-preco':
        return ofertasOrdenadas.sort((primeira, segunda) => primeira.preco - segunda.preco);

      case 'mais-confiavel':
        return ofertasOrdenadas.sort(
          (primeira, segunda) => segunda.avaliacao.confianca - primeira.avaliacao.confianca,
        );

      case 'melhor-escolha':
        return ofertasOrdenadas.sort(
          (primeira, segunda) => segunda.pontuacaoRanking - primeira.pontuacaoRanking,
        );
    }
  }
}
