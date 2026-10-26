import { TEMPO_LIMITE_API_MS } from '@shared/configuracoes/requisicoes-api.config';
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { exhaustMap, takeUntil, takeWhile, timeout, timer } from 'rxjs';
import { AcessoBusca } from '../dtos/acesso-busca.dto';
import { BuscaAceita } from '../dtos/busca-aceita.dto';
import { BuscaConsulta } from '../dtos/busca-consulta.dto';
import { BuscaHistorico } from '../dtos/busca-historico.dto';
import { SolicitarBusca } from '../dtos/solicitar-busca.dto';

@Injectable({ providedIn: 'root' })
export class BuscasService {
  private readonly http = inject(HttpClient);

  obterAcesso() {
    return this.http
      .get<AcessoBusca>('/api/v1/buscas/acesso', { withCredentials: true })
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  obterHistorico() {
    return this.http
      .get<readonly BuscaHistorico[]>('/api/v1/buscas/historico', { withCredentials: true })
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  prepararVisitante() {
    return this.http
      .get<void>('/api/v1/buscas/visitante', { withCredentials: true })
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  solicitar(pedido: SolicitarBusca) {
    // Não repetir POST automaticamente: a solicitação pendente preserva a chave para nova tentativa explícita.
    return this.http
      .post<BuscaAceita>('/api/v1/buscas', pedido, { withCredentials: true })
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  obter(id: string) {
    return this.http
      .get<BuscaConsulta>(`/api/v1/buscas/${encodeURIComponent(id)}`, { withCredentials: true })
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  acompanhar(id: string) {
    return timer(0, 2000).pipe(
      exhaustMap(() => this.obter(id)),
      takeWhile((busca) => busca.status === 'Recebida' || busca.status === 'Processando', true),
      takeUntil(timer(240000)),
    );
  }
}
