import { AssinaturaConsulta } from '@domain/assinaturas/dtos/assinatura-consulta.dto';

export function nomePlano(codigo: string): string {
  return (
    (
      { FREE: 'Verum Free', BUSCA_PLUS: 'Busca Plus', VERUM_PLUS: 'Verum Plus' } as Record<
        string,
        string
      >
    )[codigo] ?? codigo
  );
}

export function nomeStatus(assinatura: AssinaturaConsulta): string {
  if (assinatura.cancelamentoSolicitado && assinatura.status !== 'Cancelada')
    return 'Cancelamento em processamento';

  return {
    Pendente: 'Aguardando confirmação',
    Ativa: 'Ativa',
    Inadimplente: 'Pagamento pendente',
    Cancelada: 'Cancelada',
    Expirada: 'Expirada',
  }[assinatura.status];
}

export function tempoRestante(data: string | null, agora: number): string {
  if (!data) return 'Sem data de expiração';

  const diferenca = new Date(data).getTime() - agora;

  if (!Number.isFinite(diferenca)) return 'Data indisponível';

  if (diferenca <= 0) return 'Período encerrado';

  const horas = Math.ceil(diferenca / 3600000);

  if (horas < 24) return horas === 1 ? 'Menos de 1 hora' : `Cerca de ${horas} horas`;

  const dias = Math.ceil(diferenca / 86400000);

  return dias === 1 ? 'Falta 1 dia' : `Faltam ${dias} dias`;
}

export function checkoutSeguro(url: string | null): string | null {
  if (!url) return null;

  try {
    const destino = new URL(url);

    return destino.protocol === 'https:' &&
      destino.hostname === 'www.mercadopago.com.br' &&
      !destino.username &&
      !destino.password
      ? destino.href
      : null;
  } catch {
    return null;
  }
}
