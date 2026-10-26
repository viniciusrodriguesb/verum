import {
  TEMPO_LIMITE_API_MS,
  TEMPO_LIMITE_CHECKOUT_MS,
} from '@shared/configuracoes/requisicoes-api.config';
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { timeout } from 'rxjs';
import { AssinaturaConsulta } from '../dtos/assinatura-consulta.dto';
import { PlanoConsulta } from '../dtos/plano-consulta.dto';
import { SolicitarAssinatura } from '../dtos/solicitar-assinatura.dto';

@Injectable({ providedIn: 'root' })
export class AssinaturasService {
  private readonly http = inject(HttpClient);

  listarPlanos() {
    return this.http
      .get<readonly PlanoConsulta[]>('/api/v1/assinaturas/planos')
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  listar() {
    return this.http
      .get<readonly AssinaturaConsulta[]>('/api/v1/assinaturas')
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  criar(pedido: SolicitarAssinatura) {
    // Preserve a chave em uma nova tentativa. Não há retry automático nem liberação pelo redirect.
    return this.http
      .post<AssinaturaConsulta>('/api/v1/assinaturas', pedido)
      .pipe(timeout(TEMPO_LIMITE_CHECKOUT_MS));
  }

  obter(id: string) {
    return this.http
      .get<AssinaturaConsulta>(`/api/v1/assinaturas/${encodeURIComponent(id)}`)
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }

  cancelar(id: string) {
    return this.http
      .post<void>(`/api/v1/assinaturas/${encodeURIComponent(id)}/cancelamento`, {})
      .pipe(timeout(TEMPO_LIMITE_API_MS));
  }
}
