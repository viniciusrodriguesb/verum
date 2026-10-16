# Contas, acesso e ranking — contratos para avaliação do frontend

Esta entrega não altera templates, estilos, telas ou navegação. O frontend mantém os avisos de erro existentes e passa a separar pedidos pendentes entre visitante e cada identidade autenticada. IA e Radar permanecem fora desta entrega.

## Contextos e regras implementadas

| Contexto  | Identificação                                          | Limite inicial diário | Propriedade das buscas              |
| --------- | ------------------------------------------------------ | --------------------: | ----------------------------------- |
| Visitante | Cookie protegido e HttpOnly                            |                     1 | Somente aquele visitante            |
| FREE      | Conta interna resolvida pelo sub do JWT                |                     3 | Conta, inclusive em outro navegador |
| Pago      | Conta com concessão válida de BUSCA_PLUS ou VERUM_PLUS |                    30 | Conta, inclusive em outro navegador |

As janelas são dias UTC. `reiniciaEm` informa o instante exato; a tela futura deve convertê-lo para o fuso do usuário. Uma busca aceita consome uma unidade mesmo que finalize vazia, com falha ou expirada. Consultar resultados e repetir o mesmo pedido idempotente não consome outra unidade. A qualidade do processamento e do ranking é a mesma para todos.

O primeiro acesso autenticado provisiona FREE uma única vez. Login não copia buscas, consumo ou pedidos pendentes do visitante. Logout retorna ao visitante daquela sessão. Uma concessão paga futura, expirada, revogada ou com pacote inativo não concede benefício. Sem benefício pago válido, a conta retorna ao FREE se sua concessão continuar ativa. Revogar FREE não provoca uma nova concessão automática no próximo login. Pacotes sobrepostos não somam limites: utiliza-se o maior benefício válido. Upgrade e downgrade não zeram o consumo do dia.

Conta bloqueada ou excluída é recusada pelo backend em solicitação, resultado, histórico e consulta de acesso. Token inválido ou expirado não é convertido em visitante. Não existe endpoint público para o usuário atribuir plano, desbloquear conta ou criar concessão paga. Pagamentos e administração visual de concessões não foram implementados; o backend está preparado para receber concessões do módulo responsável futuramente.

## Dados já disponíveis

`GET /api/v1/buscas/acesso`, disponível também para visitante, prepara a identidade quando necessário e não consome busca. O retorno é tipado por `AcessoBusca`: `contexto`, `pacote`, `limite`, `utilizado`, `restante`, `reiniciaEm` e `concessaoValidaAte`. Use `BuscasService.obterAcesso()` para consultar. Não inferir plano por e-mail, username ou roles do Keycloak.

`GET /api/v1/buscas/historico` exige conta autenticada e retorna suas últimas 50 buscas, ordenadas da mais recente para a mais antiga. `BuscaHistorico` fornece `id`, `consulta`, `status`, `iniciadaEm` e `concluidaEm`. Use `BuscasService.obterHistorico()`. O resultado continua disponível em `GET /api/v1/buscas/{id}`; acessar ID de outro proprietário retorna 404. Visitante não recebe uma lista de histórico.

`POST /api/v1/buscas` continua recebendo apenas consulta e chave de idempotência. Contexto, ContaId e plano são resolvidos no servidor. O backend registra consumo, busca e Outbox na mesma transação. A verificação de cota na tela nunca deve substituir a validação do servidor nem impedir a recuperação de um pedido já aceito quando a resposta se perdeu.

## Estados que precisam de decisão visual

| Situação                           | Resposta do backend                                 | O que decidir no layout                                                 |
| ---------------------------------- | --------------------------------------------------- | ----------------------------------------------------------------------- |
| Sessão inválida                    | 401, SESSAO_INVALIDA ou desafio de autenticação     | Orientação para entrar novamente, preservando o pedido pendente         |
| E-mail não confirmado              | 403, EMAIL_NAO_CONFIRMADO                           | Orientação para confirmar pelo fluxo nativo                             |
| Conta bloqueada                    | 403, CONTA_BLOQUEADA                                | Mensagem própria e canal de atendimento                                 |
| Conta excluída                     | 403, CONTA_EXCLUIDA                                 | Orientação distinta de falha temporária                                 |
| Recurso ou concessão indisponível  | 403, BUSCA_NAO_PERMITIDA                            | Estado sem permissão para novas buscas                                  |
| Cota esgotada                      | 429, LIMITE_BUSCAS                                  | Limite, consumo, horário de renovação e eventual apresentação de planos |
| Pedido repetido com outra consulta | 409, IDEMPOTENCIA_CONFLITO                          | Orientação para iniciar um novo pedido                                  |
| Histórico de conta nova            | Lista vazia                                         | Estado vazio sem trazer resultados do visitante                         |
| Concessão paga encerrada           | Contexto FREE ou 403, conforme concessões restantes | Informação de retorno ao gratuito, mantendo histórico e consumo         |

Até a aprovação do layout, permanecem apenas os avisos genéricos existentes. Não foram adicionados contador, badge de plano, botão de upgrade, página de histórico ou página de bloqueio.

## Ranking e dados para explicação

O cálculo usa `ranking-v1.0`: preço 45%, confiança 35% e atualização 20%. Correspondência é elegibilidade, não peso comercial. Acessórios reconhecidos e não solicitados ficam fora dessa seleção. Preço compara somente a mesma variante e condição de pagamento; não combina PIX com preço anunciado de outra condição.

O histórico utiliza a janela de 30 dias e uma observação diária representativa por loja e variante, evitando que mais coletas da mesma loja fabriquem confiança. Com pelo menos 10 observações e 3 lojas, há histórico suficiente; quando também há 3 lojas no mercado atual, combina histórico 60% e mercado 40%. Sem histórico suficiente, utiliza mercado quando houver 3 lojas; caso contrário, preço recebe nota neutra provisória, e não nota máxima. Pequenas diferenças de até 0,5% são tratadas como empate. Atualização tem decaimento exponencial com meia-vida de 60 minutos. Confiança combina a pontuação inicial da loja (70%) e da fonte (30%).

As notas existentes da API permanecem em 0–100 para compatibilidade; o frontend já divide por 10 para apresentar 0–10. Não aplicar outra divisão aos pesos, percentis ou medianas monetárias.

Cada oferta possui `fundamentos`, com versão, pesos, confiança do cálculo, flag de preço provisório, quantidade de lojas, observações, dias, medianas, percentil histórico, idade e parâmetros. Esses dados ficam congelados no snapshot. Resultados antigos podem não ter fundamentos; o campo é opcional no DTO do Angular.

Falta decidir como apresentar preço provisório, confiança Baixa/Media/Alta, tamanho da amostra e justificativas numéricas no tooltip existente. As mensagens atuais que prometem média de mercado, reputação verificada ou preço justo devem ser revistas junto desse layout. Não é necessário criar novas telas para todas as informações.

## Limites dos dados atuais

O catálogo ainda usa identidade provisória por URL, portanto não reconhece automaticamente que anúncios de lojas diferentes são a mesma variante. A pontuação de confiança inicial não é reputação externa medida e não confirma o vendedor do marketplace. Os providers não comprovam todos os dados de estoque, condição e vendedor; não se deve apresentá-los como verificados. Validação adicional de outliers, vendedor e identidade entre lojas continua necessária para completar toda a elegibilidade estatística descrita no ADR. O motor não inventa esse conhecimento nem usa a IA para definir notas.

## Contratos de assinatura disponíveis, sem alteração de layout

O backend de pagamento mensal automático e o serviço Angular AssinaturasService estão implementados. Pagamentos ficam desabilitados até a configuração de credenciais e preços. Não foram adicionados botões, páginas, rotas ou mensagens visuais de assinatura.

| Operação autenticada    | Rota                                       | Uso futuro no frontend                                      |
| ----------------------- | ------------------------------------------ | ----------------------------------------------------------- |
| Planos contratáveis     | GET /api/v1/assinaturas/planos             | Renderizar somente preços e planos retornados pelo servidor |
| Criar contratação       | POST /api/v1/assinaturas                   | Enviar plano e chaveIdempotencia; receber URL de checkout   |
| Contratações da conta   | GET /api/v1/assinaturas                    | Recuperar as vinte mais recentes, inclusive após timeout    |
| Situação da contratação | GET /api/v1/assinaturas/{id}               | Consultar status, período e intenção de cancelamento        |
| Solicitar cancelamento  | POST /api/v1/assinaturas/{id}/cancelamento | HTTP 202 indica recebimento, não confirmação do gateway     |

AssinaturaConsulta contém id, plano, status, valor, moeda, urlCheckout, periodoTerminaEm e cancelamentoSolicitado. Os DTOs ficam em domain/assinaturas/dtos e o serviço em domain/assinaturas/services. O interceptor de autenticação existente envia o access token à API; nenhum segredo Mercado Pago vai ao navegador.

Na avaliação do layout, precisamos decidir: seleção de plano, confirmação explícita da recorrência mensal e preço, abertura do checkout hospedado, estado de confirmação pendente após o retorno, exibição do período pago e solicitação/andamento do cancelamento. O cancelamento confirmado preserva o período já pago. Não existe rateio ou troca imediata de plano nesta entrega.

Preserve uma chave de idempotência por tentativa de contratação e conta, inclusive em recarregamento e nova tentativa após erro. Em timeout, consulte a lista antes de iniciar outra contratação. Não crie uma nova chave automaticamente para contornar ASSINATURA_EXISTENTE ou ASSINATURA_EM_CONFIRMACAO. O backend não repete uma criação externa com resposta incerta. O serviço Angular não faz retry automático nem navegação por conta própria.

Após o retorno do checkout, consulte a assinatura e GET /api/v1/buscas/acesso para atualizar o contexto da conta. Parâmetros da URL, status de redirecionamento e assinatura apenas autorizada não comprovam pagamento. Aguarde a confirmação do backend, com polling limitado e opção de consultar novamente; evite indicador infinito. A URL de checkout serve somente para uma contratação ainda pendente; não reutilize links de contratos cancelados.

PAGAMENTOS_DESABILITADOS e GATEWAY_INDISPONIVEL indicam indisponibilidade; PLANO_INDISPONIVEL indica plano/preço não contratável; ASSINATURA_EXISTENTE e ASSINATURA_EM_CONFIRMACAO exigem recuperar o estado existente; IDEMPOTENCIA_CONFLITO indica reutilização incorreta da chave. Erros de sessão, e-mail e conta seguem os tratamentos já existentes. A interface não deve prometer que cancelamento ou bloqueio da conta eliminam imediatamente uma cobrança em andamento.

Os guias de configuração do Mercado Pago, variáveis de ambiente, tópicos de webhook e homologação ficam no README principal do backend, seção Assinaturas e pagamentos. A IA e o processamento do Radar continuam fora desta entrega.

## Próxima avaliação visual

Prioridade sugerida: identificação do contexto e cota, estados de bloqueio/sessão, histórico da conta, explicação honesta do ranking e fluxo visual da assinatura. Google e Radar continuam para entregas próprias. Nenhuma decisão de paleta, componente, posição ou navegação foi imposta por esta implementação.
