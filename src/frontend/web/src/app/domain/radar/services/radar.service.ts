import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { timeout } from 'rxjs';
import {
  CapacidadeRadar,
  MonitoramentoConsulta,
  NotificacaoRadar,
  OfertaRadar,
  PreferenciasAviso,
} from '../dtos/radar.dto';

@Injectable({ providedIn: 'root' })
export class RadarService {
  private readonly http = inject(HttpClient);

  capacidade() {
    return this.http.get<CapacidadeRadar>('/api/v1/radar/capacidade').pipe(timeout(15000));
  }

  listar() {
    return this.http.get<readonly MonitoramentoConsulta[]>('/api/v1/radar').pipe(timeout(15000));
  }

  oferta(id: string) {
    return this.http
      .get<OfertaRadar>(`/api/v1/radar/ofertas/${encodeURIComponent(id)}`)
      .pipe(timeout(15000));
  }

  criar(oferta: OfertaRadar, precoAlvo: number, chaveIdempotencia: string) {
    return this.http
      .post<MonitoramentoConsulta>('/api/v1/radar', {
        ofertaId: oferta.id,
        varianteId: oferta.varianteId,
        precoAlvo,
        chaveIdempotencia,
        identidadeConfirmada: true,
        emailAutorizado: false,
      })
      .pipe(timeout(15000));
  }

  alterar(id: string, precoAlvo: number, acao: string) {
    return this.http
      .patch<void>(`/api/v1/radar/${encodeURIComponent(id)}`, { precoAlvo, acao })
      .pipe(timeout(15000));
  }

  preferencias() {
    return this.http
      .get<PreferenciasAviso>('/api/v1/notificacoes/preferencias')
      .pipe(timeout(15000));
  }

  salvarPreferencias(pedido: PreferenciasAviso) {
    return this.http.put<void>('/api/v1/notificacoes/preferencias', pedido).pipe(timeout(15000));
  }

  notificacoes() {
    return this.http.get<readonly NotificacaoRadar[]>('/api/v1/notificacoes').pipe(timeout(15000));
  }
}
