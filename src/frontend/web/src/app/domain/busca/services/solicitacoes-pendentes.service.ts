import { inject, Injectable } from '@angular/core';
import { AutenticacaoService } from '@core/autenticacao/services/autenticacao.service';

import { SolicitarBusca } from '../dtos/solicitar-busca.dto';

@Injectable({ providedIn: 'root' })
export class SolicitacoesPendentesService {
  private readonly pedidos = new Map<string, SolicitarBusca>();

  private readonly autenticacao = inject(AutenticacaoService);

  obter(consulta: string): SolicitarBusca {
    const existente = this.pedidos.get(this.chave(consulta)) ?? this.ler(consulta);

    if (existente) return existente;

    const pedido = { consulta, chaveIdempotencia: crypto.randomUUID() };

    this.pedidos.set(this.chave(consulta), pedido);

    try {
      sessionStorage.setItem(this.chave(consulta), JSON.stringify(pedido));
    } catch {
      // Se o armazenamento estiver bloqueado, preserva a chave durante a navegação da aplicação.
    }

    return pedido;
  }

  concluir(consulta: string): void {
    this.pedidos.delete(this.chave(consulta));

    try {
      sessionStorage.removeItem(this.chave(consulta));
    } catch {
      // O navegador pode bloquear o armazenamento.
    }
  }

  private ler(consulta: string): SolicitarBusca | null {
    try {
      const valor = sessionStorage.getItem(this.chave(consulta));

      const pedido = valor ? (JSON.parse(valor) as Partial<SolicitarBusca>) : null;

      if (
        pedido?.consulta === consulta &&
        typeof pedido.chaveIdempotencia === 'string' &&
        pedido.chaveIdempotencia.length > 0 &&
        pedido.chaveIdempotencia.length <= 100
      ) {
        this.pedidos.set(this.chave(consulta), pedido as SolicitarBusca);

        return pedido as SolicitarBusca;
      }
    } catch {
      // Conteúdo inválido não impede uma nova solicitação.
    }

    return null;
  }

  private chave(consulta: string): string {
    const sujeito = this.autenticacao.usuario()?.sujeito;

    return sujeito
      ? `verum.busca.pendente:conta:${sujeito}:${consulta}`
      : `verum.busca.pendente:${consulta}`;
  }
}
