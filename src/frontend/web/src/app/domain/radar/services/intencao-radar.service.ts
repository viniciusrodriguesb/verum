import { Injectable } from '@angular/core';

import { IntencaoRadar } from '../dtos/intencao-radar.dto';

@Injectable({ providedIn: 'root' })
export class IntencaoRadarService {
  obter(): IntencaoRadar | null {
    try {
      const item = JSON.parse(
        sessionStorage.getItem('verum.radar.intencao') ?? 'null',
      ) as IntencaoRadar | null;

      if (
        item &&
        item.expiraEm > Date.now() &&
        /^[0-9a-f-]{36}$/i.test(item.ofertaId) &&
        Number.isFinite(item.precoAlvo) &&
        typeof item.chave === 'string'
      )
        return item;
      this.limpar();
    } catch {
      /* Armazenamento indisponível não impede o formulário. */
    }
    return null;
  }

  salvar(ofertaId: string, precoAlvo: number, chave: string): void {
    try {
      sessionStorage.setItem(
        'verum.radar.intencao',
        JSON.stringify({ ofertaId, precoAlvo, chave, expiraEm: Date.now() + 30 * 60000 }),
      );
    } catch {
      /* Continua sem persistência local. */
    }
  }

  limpar(): void {
    try {
      sessionStorage.removeItem('verum.radar.intencao');
    } catch {
      /* Sem armazenamento local. */
    }
  }
}
