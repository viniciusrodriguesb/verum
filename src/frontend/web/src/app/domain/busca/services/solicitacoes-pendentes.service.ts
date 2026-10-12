import { Injectable } from '@angular/core';

import { SolicitarBusca } from '../dtos/solicitar-busca.dto';

@Injectable({ providedIn: 'root' })
export class SolicitacoesPendentesService {
  private readonly pedidos = new Map<string, SolicitarBusca>();

  obter(consulta: string): SolicitarBusca {
    const existente = this.pedidos.get(consulta) ?? this.ler(consulta);

    if (existente) return existente;

    const pedido = { consulta, chaveIdempotencia: crypto.randomUUID() };

    this.pedidos.set(consulta, pedido);

    try {
      sessionStorage.setItem(this.chave(consulta), JSON.stringify(pedido));
    } catch {
      // Se o armazenamento estiver bloqueado, preserva a chave durante a navegação da aplicação.
    }

    return pedido;
  }

  concluir(consulta: string): void {
    this.pedidos.delete(consulta);

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
        this.pedidos.set(consulta, pedido as SolicitarBusca);

        return pedido as SolicitarBusca;
      }
    } catch {
      // Conteúdo inválido não impede uma nova solicitação.
    }

    return null;
  }

  private chave(consulta: string): string {
    return `verum.busca.pendente:${consulta}`;
  }
}
