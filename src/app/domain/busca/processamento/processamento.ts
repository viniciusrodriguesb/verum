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
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideArrowLeft,
  LucideCheck,
  LucidePencil,
  LucideRefreshCw,
  LucideSearch,
  LucideShieldCheck,
  LucideTag,
} from '@lucide/angular';
import { Subscription, distinctUntilChanged, map, take, timer } from 'rxjs';

import { IlustracaoRadarComponent } from '@shared/components/ilustracao-radar/ilustracao-radar';

import { Header } from '@shared/components/header/header';
import { NavegacaoMobileComponent } from '@shared/components/navegacao-mobile/navegacao-mobile';

import { normalizarTermoBusca, validarTermoBusca } from '../validators/termo-busca.validator';

type EstadoEtapa = 'concluida' | 'ativa' | 'pendente';

type CodigoEtapa = 'entendimento' | 'consulta-lojas' | 'comparacao' | 'ranking';

type IconeAvaliacao = 'preco' | 'confianca' | 'atualizacao';

interface EtapaProcessamento {
  readonly codigo: CodigoEtapa;
  readonly titulo: string;
  readonly descricao: string;
}

interface EtapaProcessamentoApresentacao extends EtapaProcessamento {
  readonly estado: EstadoEtapa;
}

interface InformacaoAvaliacao {
  readonly icone: IconeAvaliacao;
  readonly titulo: string;
  readonly descricao: string;
}

@Component({
  selector: 'verum-processamento-busca',
  standalone: true,
  imports: [
    IlustracaoRadarComponent,
    Header,
    NavegacaoMobileComponent,
    NgClass,
    ReactiveFormsModule,
    LucideArrowLeft,
    LucideCheck,
    LucidePencil,
    LucideRefreshCw,
    LucideSearch,
    LucideShieldCheck,
    LucideTag,
  ],
  host: {
    class:
      'block min-h-dvh bg-white pb-[calc(66px+env(safe-area-inset-bottom))] text-verum-navy lg:pb-0',
  },
  templateUrl: './processamento.html',
  styleUrl: './processamento.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProcessamentoComponent {
  private static readonly INTERVALO_ETAPAS_MS = 3_200;
  private static readonly INDICE_ETAPA_INICIAL = 1;

  private readonly rota = inject(ActivatedRoute);
  private readonly roteador = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly inputBuscaDesktop =
    viewChild.required<ElementRef<HTMLInputElement>>('inputBuscaDesktop');

  private readonly indiceEtapaAtual = signal(ProcessamentoComponent.INDICE_ETAPA_INICIAL);

  private simulacaoEtapas?: Subscription;

  protected readonly formularioBusca = new FormGroup({
    termo: new FormControl('', {
      nonNullable: true,
      validators: [validarTermoBusca],
    }),
  });

  private readonly etapas: readonly EtapaProcessamento[] = [
    {
      codigo: 'entendimento',
      titulo: 'Entendendo sua busca',
      descricao: 'Identificamos o produto e as melhores variantes.',
    },
    {
      codigo: 'consulta-lojas',
      titulo: 'Consultando lojas confiáveis',
      descricao: 'Buscando ofertas atualizadas em lojas verificadas.',
    },
    {
      codigo: 'comparacao',
      titulo: 'Comparando preços e histórico',
      descricao: 'Analisando preços, histórico e reputação das lojas.',
    },
    {
      codigo: 'ranking',
      titulo: 'Preparando o ranking',
      descricao: 'Organizando as melhores opções para você.',
    },
  ];

  protected readonly etapasProcessamento = computed<readonly EtapaProcessamentoApresentacao[]>(
    () => {
      const indiceAtual = this.indiceEtapaAtual();

      return this.etapas.map((etapa, indice) => ({
        ...etapa,
        estado: this.obterEstadoEtapa(indice, indiceAtual),
      }));
    },
  );

  protected readonly etapaAtiva = computed(() =>
    this.etapasProcessamento().find((etapa) => etapa.estado === 'ativa'),
  );

  protected readonly informacoesAvaliacao: readonly InformacaoAvaliacao[] = [
    {
      icone: 'preco',
      titulo: 'Preço justo',
      descricao:
        'Analisamos o preço atual e o histórico para indicar o melhor momento para comprar.',
    },
    {
      icone: 'confianca',
      titulo: 'Confiança da loja',
      descricao:
        'Verificamos reputação, avaliações e segurança para garantir uma compra tranquila.',
    },
    {
      icone: 'atualizacao',
      titulo: 'Atualização da oferta',
      descricao: 'Monitoramos as lojas para mostrar apenas ofertas atualizadas e disponíveis.',
    },
  ];

  protected readonly identificadoresEsqueleto = [1, 2, 3] as const;

  constructor() {
    this.observarTermoDaRota();
  }

  protected get buscaInvalida(): boolean {
    const campoTermo = this.formularioBusca.controls.termo;

    return campoTermo.invalid && campoTermo.touched;
  }

  protected buscarNovamente(): void {
    this.formularioBusca.markAllAsTouched();

    if (this.formularioBusca.invalid) {
      this.focarCampoBusca();
      return;
    }

    const termoNormalizado = normalizarTermoBusca(this.formularioBusca.controls.termo.value);

    const termoAtual = this.rota.snapshot.queryParamMap.get('q') ?? '';

    if (termoNormalizado === termoAtual) {
      this.iniciarSimulacaoVisual();
      return;
    }

    void this.roteador.navigate([], {
      relativeTo: this.rota,
      queryParams: {
        q: termoNormalizado,
      },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  protected alterarBusca(): void {
    const input = this.inputBuscaDesktop().nativeElement;

    input.focus();
    input.select();
  }

  protected voltarParaBusca(): void {
    void this.roteador.navigate(['/']);
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

        this.iniciarSimulacaoVisual();
      });
  }

  private iniciarSimulacaoVisual(): void {
    this.simulacaoEtapas?.unsubscribe();

    const indiceInicial = ProcessamentoComponent.INDICE_ETAPA_INICIAL;

    const quantidadeAvancos = this.etapas.length - indiceInicial - 1;

    this.indiceEtapaAtual.set(indiceInicial);

    this.simulacaoEtapas = timer(
      ProcessamentoComponent.INTERVALO_ETAPAS_MS,
      ProcessamentoComponent.INTERVALO_ETAPAS_MS,
    )
      .pipe(take(quantidadeAvancos + 1), takeUntilDestroyed(this.destroyRef))
      .subscribe((emissao) => {
        const proximoIndice = indiceInicial + emissao + 1;

        if (proximoIndice < this.etapas.length) {
          this.indiceEtapaAtual.set(proximoIndice);
          return;
        }

        void this.roteador.navigate(['/busca/resultado'], {
          queryParams: {
            q: this.formularioBusca.controls.termo.value,
          },
          replaceUrl: true,
        });
      });
  }

  private obterEstadoEtapa(indice: number, indiceAtual: number): EstadoEtapa {
    if (indice < indiceAtual) {
      return 'concluida';
    }

    if (indice === indiceAtual) {
      return 'ativa';
    }

    return 'pendente';
  }

  private focarCampoBusca(): void {
    this.inputBuscaDesktop().nativeElement.focus();
  }
}
