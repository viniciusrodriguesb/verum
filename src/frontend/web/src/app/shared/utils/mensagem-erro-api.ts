import { HttpErrorResponse } from '@angular/common/http';

export function mensagemErroApi(erro: unknown): string {
  if (erro instanceof HttpErrorResponse) {
    const busca = /\/api\/v1\/buscas(?:\/|$)/.test(erro.url ?? '');

    switch (erro.status) {
      case 400:
        return 'Confira os dados informados e tente novamente.';
      case 401:
        return 'Sua sessão expirou. Entre novamente para continuar.';
      case 403:
        return 'Você não tem acesso a esta operação.';
      case 404:
        return 'O conteúdo solicitado não está disponível para este navegador.';
      case 409:
        return 'Esta solicitação já foi utilizada. Inicie uma nova tentativa.';
      case 429:
        return busca
          ? 'Você atingiu o limite diário de buscas. Tente novamente amanhã.'
          : 'Muitas solicitações em pouco tempo. Aguarde antes de tentar novamente.';
    }
  }

  return 'Não foi possível concluir a solicitação. Tente novamente em instantes.';
}
