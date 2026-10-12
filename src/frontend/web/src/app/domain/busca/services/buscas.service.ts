import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { exhaustMap, takeUntil, takeWhile, timer, timeout } from 'rxjs';
import { BuscaAceita } from '../dtos/busca-aceita.dto';
import { BuscaConsulta } from '../dtos/busca-consulta.dto';
import { SolicitarBusca } from '../dtos/solicitar-busca.dto';

@Injectable({ providedIn: 'root' })
export class BuscasService {
  private readonly http = inject(HttpClient);

  prepararVisitante() {
    return this.http
      .get<void>('/api/v1/buscas/visitante', { withCredentials: true })
      .pipe(timeout(15000));
  }

  solicitar(pedido: SolicitarBusca) {
    // Não repetir POST automaticamente: a solicitação pendente preserva a chave para nova tentativa explícita.
    return this.http
      .post<BuscaAceita>('/api/v1/buscas', pedido, { withCredentials: true })
      .pipe(timeout(15000));
  }

  obter(id: string) {
    return this.http
      .get<BuscaConsulta>(`/api/v1/buscas/${encodeURIComponent(id)}`, { withCredentials: true })
      .pipe(timeout(15000));
  }

  acompanhar(id: string) {
    return timer(0, 2000).pipe(
      exhaustMap(() => this.obter(id)),
      takeWhile((busca) => busca.status === 'Recebida' || busca.status === 'Processando', true),
      takeUntil(timer(240000)),
    );
  }
}
