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
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import {
  LucideArrowLeft,
  LucideCheck,
  LucidePencil,
  LucideRefreshCw,
  LucideSearch,
  LucideShieldCheck,
  LucideTag,
} from '@lucide/angular';
import { EMPTY, switchMap, tap, timer } from 'rxjs';
import { IniciarBuscaService } from '../services/iniciar-busca.service';

import { IlustracaoRadarComponent } from '@shared/components/ilustracao-radar/ilustracao-radar';

import { BuscaConsulta } from '../dtos/busca-consulta.dto';
import { BuscasService } from '../services/buscas.service';

import { Header } from '@core/components/header/header';
import { NavegacaoMobileComponent } from '@core/components/navegacao-mobile/navegacao-mobile';

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
  providers: [IniciarBuscaService],
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
  private readonly rota = inject(ActivatedRoute);

  private readonly roteador = inject(Router);

  protected readonly inicioBusca = inject(IniciarBuscaService);

  private readonly destroyRef = inject(DestroyRef);

  private readonly inputBuscaDesktop =
    viewChild.required<ElementRef<HTMLInputElement>>('inputBuscaDesktop');

  private readonly buscas = inject(BuscasService);

  protected readonly busca = signal<BuscaConsulta | null>(null);

  protected readonly erro = signal<string | null>(null);

  protected readonly consulta = signal('');

  protected readonly demorada = signal(false);

  private iniciadaEm = Date.now();

  // Etapas observadas no backend; sem avanço temporal artificial.
  private readonly indiceEtapaAtual = signal(0);

  private ultimoAvanco = Date.now();

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
      descricao: 'Interpretamos os termos e o modelo informado.',
    },
    {
      codigo: 'consulta-lojas',
      titulo: 'Consultando as fontes disponíveis',
      descricao: 'Buscando ofertas nas fontes disponíveis.',
    },
    {
      codigo: 'comparacao',
      titulo: 'Validando as ofertas',
      descricao: 'Conferindo correspondência e condições das ofertas.',
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
      descricao: 'Comparamos ofertas equivalentes e usamos histórico quando há dados suficientes.',
    },
    {
      icone: 'confianca',
      titulo: 'Confiança da loja',
      descricao: 'A qualidade inicial da fonte não confirma a reputação do vendedor.',
    },
    {
      icone: 'atualizacao',
      titulo: 'Atualização da oferta',
      descricao: 'Mostramos quando o preço foi observado; confirme preço e estoque na loja.',
    },
  ];

  protected readonly identificadoresEsqueleto = [1, 2, 3] as const;

  constructor() {
    this.observarTermoDaRota();

    timer(650, 650)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.demorada.set(Date.now() - this.iniciadaEm >= 30000);
        this.avancarStepper();
      });
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

    this.inicioBusca.iniciar(termoNormalizado);
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
      return 'A pesquisa deve ter no máximo 500 caracteres.';
    }

    return null;
  }

  private observarTermoDaRota(): void {
    this.rota.queryParamMap
      .pipe(
        switchMap((parametros) => this.obterAcompanhamento(parametros)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (busca) => {
          this.busca.set(busca);
        },
        error: () => {
          void this.roteador.navigate(['/']);
        },
      });
  }

  private obterAcompanhamento(parametros: ParamMap) {
    this.erro.set(null);
    this.busca.set(null);

    this.iniciadaEm = Date.now();

    this.demorada.set(false);

    this.indiceEtapaAtual.set(0);

    this.ultimoAvanco = Date.now();

    const consulta = parametros.get('q') ?? '';

    this.consulta.set(consulta);
    this.formularioBusca.controls.termo.setValue(consulta);

    const id = parametros.get('id');

    if (id)
      return this.buscas.acompanhar(id).pipe(
        tap({
          complete: () => {
            const status = this.busca()?.status;

            if (!status || status === 'Recebida' || status === 'Processando') {
              this.erro.set(
                'A busca está demorando. Retome o acompanhamento para consultar a mesma solicitação.',
              );
            }
          },
        }),
      );
    if (!consulta.trim() || consulta.length > 500) {
      void this.roteador.navigate(['/']);
      return EMPTY;
    }
    void this.roteador.navigate(['/'], { queryParams: { q: consulta }, replaceUrl: true });
    return EMPTY;
  }

  protected retomar(): void {
    window.location.reload();
  }

  private avancarStepper(): void {
    if (this.erro()) return;

    const busca = this.busca();

    if (!busca) return;

    const finalizada = busca.status !== 'Recebida' && busca.status !== 'Processando';

    const agora = Date.now();

    const indice = this.indiceEtapaAtual();

    const destino = finalizada
      ? 3
      : ((
          { Recebimento: 0, Catalogo: 0, Ofertas: 1, Ranking: 2, Resultado: 3 } as Record<
            string,
            number
          >
        )[busca.etapa] ?? 0);

    if (indice < destino) {
      this.indiceEtapaAtual.update((atual) => atual + 1);

      this.ultimoAvanco = agora;

      return;
    }

    if (finalizada && indice === 3 && agora - this.ultimoAvanco >= 650) {
      void this.roteador.navigate(['/busca/resultado'], {
        queryParams: { id: busca.id, q: this.consulta() },
        replaceUrl: true,
      });
    }
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
