import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { ContaService } from '@core/autenticacao/services/conta.service';
import { ContaAutenticada } from '@core/autenticacao/dtos/conta-autenticada.dto';
import { Header } from '@core/components/header/header';
import { NavegacaoMobileComponent } from '@core/components/navegacao-mobile/navegacao-mobile';
import { IntencaoRadarService } from '@domain/radar/services/intencao-radar.service';
import { AssinaturaConsulta } from '@domain/assinaturas/dtos/assinatura-consulta.dto';
import { PlanoConsulta } from '@domain/assinaturas/dtos/plano-consulta.dto';
import { AssinaturasService } from '@domain/assinaturas/services/assinaturas.service';
import { AcessoBusca } from '@domain/busca/dtos/acesso-busca.dto';
import { BuscasService } from '@domain/busca/services/buscas.service';
import {
  LucideArrowLeft,
  LucideArrowUpRight,
  LucideCheck,
  LucideCreditCard,
  LucideRefreshCw,
  LucideShieldCheck,
  LucideUserRound,
} from '@lucide/angular';
import { AvisosApiService } from '@shared/services/avisos-api.service';
import { firstValueFrom, forkJoin, timer } from 'rxjs';
import { checkoutSeguro, nomePlano, nomeStatus, tempoRestante } from '../utils/plano-apresentacao';

@Component({
  selector: 'verum-minha-conta',
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    Header,
    NavegacaoMobileComponent,
    LucideArrowLeft,
    LucideArrowUpRight,
    LucideCheck,
    LucideCreditCard,
    LucideRefreshCw,
    LucideShieldCheck,
    LucideUserRound,
  ],
  templateUrl: './minha-conta.html',
  styleUrl: './minha-conta.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MinhaContaComponent implements OnInit {
  protected readonly autenticacao = inject(AutenticacaoService);

  protected readonly possuiIntencao = !!inject(IntencaoRadarService).obter();

  private readonly contas = inject(ContaService);

  private readonly buscas = inject(BuscasService);

  private readonly assinaturasApi = inject(AssinaturasService);

  private readonly avisos = inject(AvisosApiService);

  protected readonly perfil = signal<ContaAutenticada | null>(null);

  protected readonly acesso = signal<AcessoBusca | null>(null);

  protected readonly assinaturas = signal<readonly AssinaturaConsulta[]>([]);

  protected readonly planos = signal<readonly PlanoConsulta[]>([]);

  protected readonly aba = signal<'conta' | 'plano'>('conta');

  protected readonly carregando = signal(true);

  protected readonly erro = signal(false);

  protected readonly processando = signal(false);

  protected readonly confirmarCancelamento = signal(false);

  protected readonly planoSelecionado = signal<PlanoConsulta | null>(null);

  protected readonly agora = signal(Date.now());

  protected readonly ultimaAtualizacao = signal<number | null>(null);

  protected readonly assinatura = computed(
    () =>
      this.assinaturas().find(
        (item) => item.status !== 'Cancelada' && item.status !== 'Expirada',
      ) ??
      this.assinaturas()[0] ??
      null,
  );

  protected readonly possuiContratacao = computed(() =>
    this.assinaturas().some((item) => item.status !== 'Cancelada' && item.status !== 'Expirada'),
  );

  protected readonly percentualUso = computed(() => {
    const acesso = this.acesso();

    return acesso && acesso.limite > 0
      ? Math.min(100, Math.round((acesso.restante / acesso.limite) * 100))
      : 0;
  });

  protected readonly prazo = computed(() =>
    tempoRestante(this.acesso()?.concessaoValidaAte ?? null, this.agora()),
  );

  protected readonly prazoAssinatura = computed(() =>
    tempoRestante(this.assinatura()?.periodoTerminaEm ?? null, this.agora()),
  );

  protected readonly nomePlano = nomePlano;

  protected readonly nomeStatus = nomeStatus;

  protected readonly checkoutSeguro = checkoutSeguro;

  constructor() {
    timer(0, 60000)
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.agora.set(Date.now()));
  }

  ngOnInit(): void {
    if (location.hash === '#plano') this.aba.set('plano');

    void this.carregar();
  }

  @HostListener('window:focus')
  protected aoRetornar(): void {
    if (!this.carregando() && !this.processando()) void this.carregar();
  }

  protected async carregar(): Promise<void> {
    if (this.processando()) return;

    this.carregando.set(true);

    this.erro.set(false);

    try {
      const resultado = await firstValueFrom(
        forkJoin({
          perfil: this.contas.obter(),
          acesso: this.buscas.obterAcesso(),
          assinaturas: this.assinaturasApi.listar(),
          planos: this.assinaturasApi.listarPlanos(),
        }),
      );

      this.perfil.set(resultado.perfil);

      this.acesso.set(resultado.acesso);

      this.assinaturas.set(resultado.assinaturas);

      this.planos.set(resultado.planos);

      this.agora.set(Date.now());

      this.ultimaAtualizacao.set(Date.now());
    } catch {
      this.erro.set(true);
    } finally {
      this.carregando.set(false);
    }
  }

  protected async contratar(): Promise<void> {
    const plano = this.planoSelecionado();

    if (!plano || this.processando() || this.possuiContratacao()) return;

    this.processando.set(true);

    try {
      // A mesma tentativa mantém a chave entre recargas e respostas incertas.
      const armazenamento = `verum.assinatura.${this.autenticacao.usuario()?.sujeito}.${plano.codigo}`;

      const chave = sessionStorage.getItem(armazenamento) ?? crypto.randomUUID();

      sessionStorage.setItem(armazenamento, chave);

      const assinatura = await firstValueFrom(
        this.assinaturasApi.criar({ plano: plano.codigo, chaveIdempotencia: chave }),
      );

      sessionStorage.removeItem(armazenamento);

      this.assinaturas.update((lista) => [
        assinatura,
        ...lista.filter((item) => item.id !== assinatura.id),
      ]);

      this.planoSelecionado.set(null);

      const url = checkoutSeguro(assinatura.urlCheckout);

      if (url) location.assign(url);
      else
        this.avisos.mostrar(
          'Contratação registrada. Atualize o status para acompanhar a confirmação.',
        );
    } catch {
      // O interceptor apresenta a falha; preserve a chave para uma nova tentativa explícita.
    } finally {
      this.processando.set(false);
    }
  }

  protected async cancelar(): Promise<void> {
    const assinatura = this.assinatura();

    if (!assinatura || !this.confirmarCancelamento() || this.processando()) return;

    this.processando.set(true);

    try {
      await firstValueFrom(this.assinaturasApi.cancelar(assinatura.id));

      this.assinaturas.update((lista) =>
        lista.map((item) =>
          item.id === assinatura.id ? { ...item, cancelamentoSolicitado: true } : item,
        ),
      );

      this.confirmarCancelamento.set(false);

      this.avisos.mostrar(
        'Cancelamento solicitado. A confirmação será exibida após o processamento.',
      );
    } catch {
      // O interceptor apresenta o erro; mantenha a confirmação para permitir nova tentativa.
    } finally {
      this.processando.set(false);
    }
  }
}
