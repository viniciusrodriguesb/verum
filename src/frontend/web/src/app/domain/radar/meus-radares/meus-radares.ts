import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  LucideArrowRight,
  LucideBadgeCheck,
  LucideBell,
  LucideBookmark,
  LucideChartNoAxesColumnIncreasing,
  LucideCheck,
  LucideChevronDown,
  LucideLaptop,
  LucidePackage,
  LucidePencil,
  LucideRadar,
  LucideRefreshCw,
  LucideShieldCheck,
  LucideSmartphone,
  LucideStar,
  LucideTag,
} from '@lucide/angular';
import { NavegacaoMobileComponent } from '@core/components/navegacao-mobile/navegacao-mobile';
import { GraficoPrecoComponent } from '../components/grafico-preco/grafico-preco';
import { Header } from '@core/components/header/header';

type EstadoRadar = 'preco-atingido' | 'monitorando' | 'sem-estoque' | 'pausado';

type FiltroRadar = 'todos' | 'ativos' | 'preco-atingido' | 'pausados';

type OrdenacaoRadar = 'mais-recentes' | 'mais-antigos' | 'maior-economia';

type IconeProduto = 'celular' | 'notebook' | 'eletrodomestico';
type CorGrafico = 'verde' | 'azul' | 'cinza';

interface OpcaoFiltro {
  readonly valor: FiltroRadar;
  readonly rotulo: string;
}

interface RadarMonitorado {
  readonly id: string;
  readonly produtoSlug: string;
  readonly produto: string;
  readonly loja: string;
  readonly criadoEm: string;
  readonly criadoEmOrdem: number;
  readonly precoAtual: string;
  readonly precoAlvo: string;
  readonly estado: EstadoRadar;
  readonly atualizadoEm: string;
  readonly economiaPotencial: number;
  readonly iconeProduto: IconeProduto;
  readonly corGrafico: CorGrafico;
  readonly pontosGrafico: string;
}

const FILTROS: readonly OpcaoFiltro[] = [
  {
    valor: 'todos',
    rotulo: 'Todos',
  },
  {
    valor: 'ativos',
    rotulo: 'Ativos',
  },
  {
    valor: 'preco-atingido',
    rotulo: 'Preço atingido',
  },
  {
    valor: 'pausados',
    rotulo: 'Pausados',
  },
];

@Component({
  selector: 'verum-meus-radares',
  standalone: true,
  imports: [
    RouterLink,
    Header,
    GraficoPrecoComponent,
    NavegacaoMobileComponent,
    LucideArrowRight,
    LucideBadgeCheck,
    LucideBell,
    LucideBookmark,
    LucideChartNoAxesColumnIncreasing,
    LucideCheck,
    LucideLaptop,
    LucidePackage,
    LucidePencil,
    LucideRadar,
    LucideRefreshCw,
    LucideShieldCheck,
    LucideSmartphone,
    LucideChevronDown,
    LucideStar,
    LucideTag,
    Header,
  ],
  templateUrl: './meus-radares.html',
  styleUrl: './meus-radares.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MeusRadaresComponent {
  private readonly roteador = inject(Router);

  private readonly radares = signal<readonly RadarMonitorado[]>([]);

  protected readonly filtros = FILTROS;
  protected readonly filtroSelecionado = signal<FiltroRadar>('todos');
  protected readonly ordenacaoSelecionada = signal<OrdenacaoRadar>('mais-recentes');

  protected readonly resumo = {
    radaresAtivos: 0,
    oportunidades: 0,
    economiaPotencial: '—',
  } as const;

  protected readonly radaresFiltrados = computed(() => {
    const radaresFiltrados = this.radares().filter((radar) =>
      this.correspondeAoFiltro(radar, this.filtroSelecionado()),
    );

    return this.ordenarRadares(radaresFiltrados, this.ordenacaoSelecionada());
  });

  protected readonly oportunidadeDestaque = computed(() =>
    this.radares().find((radar) => radar.estado === 'preco-atingido'),
  );

  protected selecionarFiltro(filtro: FiltroRadar): void {
    this.filtroSelecionado.set(filtro);
  }

  protected alterarOrdenacao(ordenacao: string): void {
    if (!this.ehOrdenacaoValida(ordenacao)) {
      return;
    }

    this.ordenacaoSelecionada.set(ordenacao);
  }

  protected criarRadar(): void {
    void this.roteador.navigate(['/radar/criar']);
  }

  protected editarRadar(radarId: string): void {
    void this.roteador.navigate(['/radar', radarId, 'editar']);
  }

  protected executarAcaoPrincipal(radar: RadarMonitorado): void {
    if (radar.estado === 'preco-atingido') {
      void this.roteador.navigate(['/busca/resultados'], {
        queryParams: {
          produto: radar.produtoSlug,
        },
      });

      return;
    }

    this.editarRadar(radar.id);
  }

  protected alternarPausa(radarId: string): void {
    this.radares.update((radaresAtuais) =>
      radaresAtuais.map((radar) => {
        if (radar.id !== radarId) {
          return radar;
        }

        return {
          ...radar,
          estado: radar.estado === 'pausado' ? 'monitorando' : 'pausado',
        };
      }),
    );
  }

  protected abrirNotificacoes(): void {
    void this.roteador.navigate(['/radar/notificacoes']);
  }

  protected obterRotuloEstado(estado: EstadoRadar): string {
    const rotulos: Readonly<Record<EstadoRadar, string>> = {
      'preco-atingido': 'Preço atingido',
      monitorando: 'Monitorando',
      'sem-estoque': 'Sem estoque',
      pausado: 'Pausado',
    };

    return rotulos[estado];
  }

  protected obterRotuloAcao(radar: RadarMonitorado): string {
    return radar.estado === 'preco-atingido' ? 'Ver oferta' : 'Editar';
  }

  private correspondeAoFiltro(radar: RadarMonitorado, filtro: FiltroRadar): boolean {
    switch (filtro) {
      case 'ativos':
        return radar.estado !== 'pausado';

      case 'preco-atingido':
        return radar.estado === 'preco-atingido';

      case 'pausados':
        return radar.estado === 'pausado';

      default:
        return true;
    }
  }

  private ordenarRadares(
    radares: readonly RadarMonitorado[],
    ordenacao: OrdenacaoRadar,
  ): readonly RadarMonitorado[] {
    const radaresOrdenados = [...radares];

    switch (ordenacao) {
      case 'mais-antigos':
        return radaresOrdenados.sort(
          (radarA, radarB) => radarA.criadoEmOrdem - radarB.criadoEmOrdem,
        );

      case 'maior-economia':
        return radaresOrdenados.sort(
          (radarA, radarB) => radarB.economiaPotencial - radarA.economiaPotencial,
        );

      default:
        return radaresOrdenados.sort(
          (radarA, radarB) => radarB.criadoEmOrdem - radarA.criadoEmOrdem,
        );
    }
  }

  private ehOrdenacaoValida(ordenacao: string): ordenacao is OrdenacaoRadar {
    return ['mais-recentes', 'mais-antigos', 'maior-economia'].includes(ordenacao);
  }
}
