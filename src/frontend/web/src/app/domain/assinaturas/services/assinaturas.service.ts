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
      .pipe(timeout(15000));
  }

  listar() {
    return this.http.get<readonly AssinaturaConsulta[]>('/api/v1/assinaturas').pipe(timeout(15000));
  }

  criar(pedido: SolicitarAssinatura) {
    // Preserve a chave em uma nova tentativa. Não há retry automático nem liberação pelo redirect.
    return this.http.post<AssinaturaConsulta>('/api/v1/assinaturas', pedido).pipe(timeout(45000));
  }

  obter(id: string) {
    return this.http
      .get<AssinaturaConsulta>(`/api/v1/assinaturas/${encodeURIComponent(id)}`)
      .pipe(timeout(15000));
  }

  cancelar(id: string) {
    return this.http
      .post<void>(`/api/v1/assinaturas/${encodeURIComponent(id)}/cancelamento`, {})
      .pipe(timeout(15000));
  }
}
