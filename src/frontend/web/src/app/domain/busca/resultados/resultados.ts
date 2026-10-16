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
  LucideStar,
  LucideTag,
} from '@lucide/angular';
import { EMPTY, map, startWith, switchMap, timer } from 'rxjs';
import { IniciarBuscaService } from '../services/iniciar-busca.service';

import { BuscaConsulta } from '../dtos/busca-consulta.dto';
import { BuscasService } from '../services/buscas.service';
import { mensagemErroBusca } from '../utils/mensagem-erro-busca';
import { apresentarOferta } from './mappers/oferta-apresentacao.mapper';

import { Header } from '@core/components/header/header';
import { NavegacaoMobileComponent } from '@core/components/navegacao-mobile/navegacao-mobile';

import { normalizarTermoBusca, validarTermoBusca } from '../validators/termo-busca.validator';
import { CartaoOfertaComponent } from './cartao-oferta/cartao-oferta';
import { OfertaResultado } from './models/oferta-resultado.model';

type OrdemResultado = 'melhor-escolha' | 'menor-preco' | 'mais-confiavel';

type IconeOrdenacao = 'melhor-escolha' | 'menor-preco' | 'mais-confiavel';

interface OpcaoOrdenacao {
  readonly codigo: OrdemResultado;
  readonly rotulo: string;
  readonly icone: IconeOrdenacao;
}

@Component({
  providers: [IniciarBuscaService],
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
    LucideStar,
    LucideTag,
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

  protected readonly inicioBusca = inject(IniciarBuscaService);

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
  });

  private readonly filtrosAtuais = toSignal(
    this.formularioFiltros.valueChanges.pipe(startWith(this.formularioFiltros.getRawValue())),
    {
      initialValue: this.formularioFiltros.getRawValue(),
    },
  );

  private readonly ordemAtual = signal<OrdemResultado>('melhor-escolha');

  private readonly favoritos = signal<ReadonlySet<string>>(new Set());

  private readonly buscas = inject(BuscasService);

  protected readonly resultado = signal<BuscaConsulta | null>(null);

  protected readonly erro = signal<string | null>(null);

  protected get totalOfertasAnalisadas(): number {
    return this.resultado()?.ofertas.length ?? 0;
  }

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

  private readonly agora = toSignal(timer(0, 10000).pipe(map(() => Date.now())), {
    initialValue: Date.now(),
  });

  private readonly ofertas = computed(() =>
    (this.resultado()?.ofertas ?? []).map((oferta) => apresentarOferta(oferta, this.agora())),
  );

  protected readonly quantidadeOfertasVencidas = computed(
    () => this.ofertas().filter((oferta) => oferta.vencida).length,
  );

  protected readonly ofertasExibidas = computed(() => {
    const filtros = this.filtrosAtuais();

    const ofertasFiltradas = this.ofertas().filter((oferta) => {
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

      return true;
    });

    return this.ordenarOfertas(ofertasFiltradas, this.ordemAtual());
  });

  protected readonly possuiFiltrosAtivos = computed(() => {
    const filtros = this.filtrosAtuais();

    return filtros.precoMinimo !== null || filtros.precoMaximo !== null;
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

    this.inicioBusca.iniciar(termoNormalizado);
  }

  protected voltarParaBusca(): void {
    void this.roteador.navigate(['/']);
  }

  protected limparFiltros(): void {
    this.formularioFiltros.reset({
      precoMinimo: null,
      precoMaximo: null,
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
      return 'A pesquisa deve ter no máximo 500 caracteres.';
    }

    return null;
  }

  private observarTermoDaRota(): void {
    this.rota.queryParamMap
      .pipe(
        switchMap((parametros) => {
          this.resultado.set(null);
          this.erro.set(null);

          const consulta = parametros.get('q') ?? '';

          this.formularioBusca.controls.termo.setValue(consulta);

          const id = parametros.get('id');

          if (!id) {
            void this.roteador.navigate(['/']);
            return EMPTY;
          }
          return this.buscas.obter(id);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (resultado) => {
          if (resultado.status === 'Recebida' || resultado.status === 'Processando') {
            void this.roteador.navigate(['/busca/processamento'], {
              queryParams: { id: resultado.id, q: this.formularioBusca.controls.termo.value },
              replaceUrl: true,
            });
          } else {
            this.resultado.set(resultado);
          }
        },
        error: (erro) => this.erro.set(mensagemErroBusca(erro)),
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
