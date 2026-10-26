export const ROTULOS_ESTADO_RADAR: Readonly<Record<string, string>> = {
  Aguardando: 'Aguardando primeira checagem',
  Monitorando: 'Monitorando',
  MetaAtingida: 'Meta atingida',
  Pausado: 'Pausado por você',
  SuspensoPorAcesso: 'Acompanhamento suspenso por acesso',
  VerificacaoIndisponivel: 'Verificação indisponível',
};

export const ROTULOS_ENTREGA_RADAR: Readonly<Record<string, string>> = {
  Pendente: 'Aguardando envio',
  Processando: 'Enviando',
  Entregue: 'Enviado ao servidor de e-mail',
  AguardandoNovaTentativa: 'Aguardando nova tentativa',
  FalhouDefinitivamente: 'Não foi possível enviar; aviso disponível aqui',
  Cancelada: 'Não enviado por preferência ou oferta vencida',
};
