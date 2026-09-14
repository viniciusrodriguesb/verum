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
import { NavegacaoMobileComponent } from '@shared/components/navegacao-mobile/navegacao-mobile';
import { GraficoPrecoComponent } from '../components/grafico-preco/grafico-preco';
import { Header } from '@shared/components/header/header';

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

const RADARES_INICIAIS: readonly RadarMonitorado[] = [
  {
    id: 'radar-iphone-16',
    produtoSlug: 'iphone-16-256-gb',
    produto: 'iPhone 16 256 GB',
    loja: 'Magazine Luiza',
    criadoEm: '20/05/2024',
    criadoEmOrdem: 20240520,
    precoAtual: 'R$ 4.499,00',
    precoAlvo: 'R$ 4.500,00',
    estado: 'preco-atingido',
    atualizadoEm: 'há 10 min',
    economiaPotencial: 500,
    iconeProduto: 'celular',
    corGrafico: 'verde',
    pontosGrafico:
      '0,42 18,36 34,45 50,27 66,41 82,24 99,48 116,42 134,31 151,47 168,39 185,45 202,61 220,67 239,60 260,62',
  },
  {
    id: 'radar-notebook-lenovo',
    produtoSlug: 'notebook-lenovo-ideapad-5',
    produto: 'Notebook Lenovo IdeaPad 5',
    loja: 'Amazon',
    criadoEm: '18/05/2024',
    criadoEmOrdem: 20240518,
    precoAtual: 'R$ 2.799,00',
    precoAlvo: 'R$ 2.600,00',
    estado: 'monitorando',
    atualizadoEm: 'há 25 min',
    economiaPotencial: 199,
    iconeProduto: 'notebook',
    corGrafico: 'azul',
    pontosGrafico:
      '0,47 18,43 35,57 52,25 70,35 87,45 104,63 122,34 139,24 156,53 174,49 191,32 208,58 225,67 242,57 260,59',
  },
  {
    id: 'radar-air-fryer',
    produtoSlug: 'air-fryer-philco-56l',
    produto: 'Air Fryer Philco 5,6 L',
    loja: 'Casas Bahia',
    criadoEm: '15/05/2024',
    criadoEmOrdem: 20240515,
    precoAtual: 'Sem estoque',
    precoAlvo: 'Avisar quando voltar',
    estado: 'sem-estoque',
    atualizadoEm: 'há 2 h',
    economiaPotencial: 81,
    iconeProduto: 'eletrodomestico',
    corGrafico: 'cinza',
    pontosGrafico:
      '0,39 18,46 35,62 52,33 70,54 87,42 104,59 122,38 139,45 156,66 174,52 191,43 208,61 225,56 242,68 260,65',
  },
];

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

  private readonly radares = signal<readonly RadarMonitorado[]>(RADARES_INICIAIS);

  protected readonly filtros = FILTROS;
  protected readonly filtroSelecionado = signal<FiltroRadar>('todos');
  protected readonly ordenacaoSelecionada = signal<OrdenacaoRadar>('mais-recentes');

  protected readonly resumo = {
    radaresAtivos: 6,
    oportunidades: 2,
    economiaPotencial: 'R$ 780',
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
