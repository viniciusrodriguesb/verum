import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

type CorGrafico = 'verde' | 'azul' | 'cinza';

interface PontoGrafico {
  readonly x: number;
  readonly y: number;
}

const CORES_GRAFICO: Readonly<Record<CorGrafico, string>> = {
  verde: '#059669',
  azul: '#2563eb',
  cinza: '#94a3b8',
};

@Component({
  selector: 'verum-grafico-preco',
  standalone: true,
  templateUrl: './grafico-preco.html',
  styleUrl: './grafico-preco.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GraficoPrecoComponent {
  readonly identificador = input.required<string>();
  readonly pontos = input.required<string>();
  readonly cor = input.required<CorGrafico>();

  protected readonly corHexadecimal = computed(() => CORES_GRAFICO[this.cor()]);

  protected readonly identificadorGradiente = computed(
    () => `gradiente-${this.identificador().replace(/[^a-zA-Z0-9_-]/g, '')}`,
  );

  protected readonly pontosArea = computed(() => `0,90 ${this.pontos()} 260,90`);

  protected readonly pontoFinal = computed<PontoGrafico>(() => {
    const pontosSeparados = this.pontos().trim().split(/\s+/);
    const ultimoPonto = pontosSeparados.at(-1) ?? '260,45';
    const [x, y] = ultimoPonto.split(',').map(Number);

    return {
      x: Number.isFinite(x) ? x : 260,
      y: Number.isFinite(y) ? y : 45,
    };
  });
}
