import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';
import { Header } from '@core/components/header/header';
import { NavegacaoMobileComponent } from '@core/components/navegacao-mobile/navegacao-mobile';
import { ValorBrlDirective } from '@shared/directives/valor-brl.directive';
import { mensagemErroApi } from '@shared/utils/mensagem-erro-api';
import {
  CapacidadeRadar,
  MonitoramentoConsulta,
  NotificacaoRadar,
  OfertaRadar,
} from '../dtos/radar.dto';
import { FUSOS_AVISO, HORAS_AVISO } from '../configuracoes/horarios-aviso';
import { RadarService } from '../services/radar.service';
import { IntencaoRadarService } from '../services/intencao-radar.service';

@Component({
  selector: 'verum-meus-radares',
  imports: [
    RouterLink,
    Header,
    NavegacaoMobileComponent,
    CurrencyPipe,
    DatePipe,
    ReactiveFormsModule,
    ValorBrlDirective,
  ],
  templateUrl: './meus-radares.html',
  styleUrl: './meus-radares.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MeusRadaresComponent implements OnInit {
  protected readonly autenticacao = inject(AutenticacaoService);

  private readonly api = inject(RadarService);

  private readonly rota = inject(ActivatedRoute);

  private readonly intencao = inject(IntencaoRadarService);

  protected readonly capacidade = signal<CapacidadeRadar | null>(null);

  protected readonly radares = signal<readonly MonitoramentoConsulta[]>([]);

  protected readonly notificacoes = signal<readonly NotificacaoRadar[]>([]);

  protected readonly oferta = signal<OfertaRadar | null>(null);

  protected readonly erroAvisos = signal<string | null>(null);

  protected readonly erroLista = signal<string | null>(null);

  protected readonly erroOferta = signal<string | null>(null);

  protected readonly erroPreferencias = signal<string | null>(null);

  protected readonly confirmandoEncerramento = signal<string | null>(null);

  private retornoFoco: HTMLElement | null = null;

  protected readonly silencioAtivo = signal(true);

  protected readonly horas = HORAS_AVISO;

  protected readonly fusos = FUSOS_AVISO;

  protected readonly erro = signal<string | null>(null);

  protected readonly aviso = signal<string | null>(null);

  protected readonly carregando = signal(true);

  protected readonly salvando = signal(false);

  protected readonly preferenciasAbertas = signal(false);

  protected readonly preferenciasCarregadas = signal(false);

  protected readonly editando = signal<MonitoramentoConsulta | null>(null);

  protected readonly inverter = (aberto: boolean) => !aberto;

  private chave: string = crypto.randomUUID();

  protected readonly meta = new FormGroup({
    preco: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(0.01)],
    }),
    confirmado: new FormControl(false, { nonNullable: true, validators: Validators.requiredTrue }),
  });

  protected readonly edicao = new FormControl(1, {
    nonNullable: true,
    validators: [Validators.required, Validators.min(0.01)],
  });

  protected readonly preferencias = new FormGroup({
    emailHabilitado: new FormControl(false, { nonNullable: true }),
    silencioInicio: new FormControl(22, {
      nonNullable: true,
      validators: [Validators.min(0), Validators.max(23)],
    }),
    silencioFim: new FormControl(8, {
      nonNullable: true,
      validators: [Validators.min(0), Validators.max(23)],
    }),
    fuso: new FormControl('America/Sao_Paulo', {
      nonNullable: true,
      validators: Validators.required,
    }),
  });

  ngOnInit(): void {
    void this.carregar();
  }

  protected async carregar(): Promise<void> {
    this.carregando.set(true);
    this.preferenciasCarregadas.set(false);
    this.erro.set(null);
    this.aviso.set(null);

    const carregamentos: Promise<void>[] = [
      this.carregarSecao(async () => {
        this.capacidade.set(await firstValueFrom(this.api.capacidade()));
      }, this.erro).finally(() => this.carregando.set(false)),
    ];

    if (this.autenticacao.autenticado()) {
      carregamentos.push(
        this.carregarSecao(
          async () => this.radares.set(await firstValueFrom(this.api.listar())),
          this.erroLista,
        ),
      );
      carregamentos.push(
        this.carregarSecao(async () => {
          const preferencias = await firstValueFrom(this.api.preferencias());

          this.preferencias.setValue(preferencias);
          this.preferenciasCarregadas.set(true);
          this.silencioAtivo.set(preferencias.silencioInicio !== preferencias.silencioFim);
        }, this.erroPreferencias),
      );
      carregamentos.push(
        this.carregarSecao(
          async () => this.notificacoes.set(await firstValueFrom(this.api.notificacoes())),
          this.erroAvisos,
        ),
      );
    }

    const guardada = this.intencao.obter();

    const id = this.rota.snapshot.queryParamMap.get('oferta') ?? guardada?.ofertaId;

    if (id && /^[0-9a-f-]{36}$/i.test(id))
      carregamentos.push(
        this.carregarSecao(async () => {
          this.oferta.set(await firstValueFrom(this.api.oferta(id)));
          this.chave = guardada?.ofertaId === id ? guardada.chave : crypto.randomUUID();
          this.meta.controls.preco.setValue(
            guardada?.ofertaId === id && guardada.precoAlvo > 0 ? guardada.precoAlvo : null,
          );
        }, this.erroOferta),
      );

    await Promise.all(carregamentos);

    if (id && this.radares().some((radar) => radar.ofertaId === id)) {
      this.oferta.set(null);
      this.intencao.limpar();
    }
  }

  private async carregarSecao(
    acao: () => Promise<void>,
    erro: { set(valor: string | null): void },
  ): Promise<void> {
    erro.set(null);
    try {
      await acao();
    } catch (falha) {
      erro.set(mensagemErroApi(falha));
    }
  }

  protected guardarIntencao(): void {
    const oferta = this.oferta();

    if (oferta) this.intencao.salvar(oferta.id, this.meta.controls.preco.value ?? 0, this.chave);
  }

  protected ofertaExpirada(): boolean {
    return Date.parse(this.oferta()?.validaAte ?? '') <= Date.now();
  }

  protected entrar(): void {
    this.guardarIntencao();
    void this.autenticacao.entrar(location.origin + '/radar');
  }

  protected async criar(): Promise<void> {
    this.meta.markAllAsTouched();

    const oferta = this.oferta();

    if (this.meta.invalid || !oferta || this.meta.controls.preco.value === null) return;
    this.guardarIntencao();
    await this.executar(async () => {
      await firstValueFrom(this.api.criar(oferta, this.meta.controls.preco.value!, this.chave));
      this.intencao.limpar();
      this.oferta.set(null);
      this.chave = crypto.randomUUID();
      this.radares.set(await firstValueFrom(this.api.listar()));
      this.capacidade.set(await firstValueFrom(this.api.capacidade()));
      this.aviso.set('Radar criado. Sua meta está salva e será verificada periodicamente.');
    });
  }

  protected ajustar(item: MonitoramentoConsulta): void {
    this.editando.set(item);
    this.edicao.setValue(item.precoAlvo);
  }

  protected async salvarMeta(): Promise<void> {
    const item = this.editando();

    if (!item || this.edicao.invalid || this.edicao.value === null) return;
    if (await this.acao(item, 'editar', this.edicao.value)) this.editando.set(null);
  }

  protected async acao(
    item: MonitoramentoConsulta,
    acao: string,
    preco = item.precoAlvo,
  ): Promise<boolean> {
    return this.executar(async () => {
      await firstValueFrom(this.api.alterar(item.id, preco, acao));
      this.radares.set(await firstValueFrom(this.api.listar()));
      this.capacidade.set(await firstValueFrom(this.api.capacidade()));
      this.aviso.set('Radar atualizado.');
    });
  }

  protected alterarSilencio(evento: Event): void {
    this.silencioAtivo.set((evento.target as HTMLInputElement).checked);
    if (
      this.silencioAtivo() &&
      this.preferencias.controls.silencioInicio.value ===
        this.preferencias.controls.silencioFim.value
    )
      this.preferencias.patchValue({ silencioInicio: 22, silencioFim: 8 });
  }

  protected resumoSilencio(): string {
    const fuso =
      this.fusos.find((x) => x.valor === this.preferencias.controls.fuso.value)?.nome ??
      this.preferencias.controls.fuso.value;

    return this.silencioAtivo()
      ? 'Não enviar entre ' +
          this.preferencias.controls.silencioInicio.value +
          'h e ' +
          this.preferencias.controls.silencioFim.value +
          'h — ' +
          fuso
      : 'Horário silencioso desativado — ' + fuso;
  }

  protected async salvarPreferencias(): Promise<void> {
    if (this.preferencias.invalid || !this.preferenciasCarregadas() || this.horariosInvalidos())
      return;
    await this.executar(async () => {
      await firstValueFrom(
        this.api.salvarPreferencias({
          ...this.preferencias.getRawValue(),
          ...(!this.silencioAtivo() ? { silencioInicio: 0, silencioFim: 0 } : {}),
        }),
      );
      this.aviso.set('Preferências salvas.');
    });
  }

  protected horariosInvalidos(): boolean {
    return (
      this.silencioAtivo() &&
      this.preferencias.controls.silencioInicio.value ===
        this.preferencias.controls.silencioFim.value
    );
  }

  protected pedirEncerramento(id: string, evento: Event): void {
    this.confirmandoEncerramento.set(id);
    this.retornoFoco = evento.currentTarget as HTMLElement;
  }

  protected cancelarEncerramento(): void {
    this.confirmandoEncerramento.set(null);
    this.retornoFoco?.focus();
  }

  protected async encerrar(item: MonitoramentoConsulta): Promise<void> {
    if (await this.acao(item, 'encerrar')) {
      this.confirmandoEncerramento.set(null);
      document.getElementById('atualizar-radar')?.focus();
    }
  }

  protected rotuloEntrega(codigo: string): string {
    return (
      (
        {
          Pendente: 'Aguardando envio',
          Processando: 'Enviando',
          Entregue: 'Enviado ao servidor de e-mail',
          AguardandoNovaTentativa: 'Aguardando nova tentativa',
          FalhouDefinitivamente: 'Não foi possível enviar; aviso disponível aqui',
          Cancelada: 'Não enviado por preferência ou oferta vencida',
        } as Record<string, string>
      )[codigo] ?? 'Consulte o estado do envio'
    );
  }

  protected estado(codigo: string): string {
    return (
      (
        {
          Aguardando: 'Aguardando primeira checagem',
          Monitorando: 'Monitorando',
          MetaAtingida: 'Meta atingida',
          Pausado: 'Pausado por você',
          SuspensoPorAcesso: 'Acompanhamento suspenso por acesso',
          VerificacaoIndisponivel: 'Verificação indisponível',
        } as Record<string, string>
      )[codigo] ?? codigo
    );
  }

  private async executar(acao: () => Promise<void>): Promise<boolean> {
    if (this.salvando()) return false;
    this.salvando.set(true);
    this.erro.set(null);
    this.aviso.set(null);
    try {
      await acao();
      return true;
    } catch (erro) {
      this.erro.set(mensagemErroApi(erro));
      return false;
    } finally {
      this.salvando.set(false);
    }
  }
}
