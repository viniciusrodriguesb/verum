# Arquitetura e decisões do Verum

Este documento reúne as decisões que devem orientar novas entregas. Configuração, comandos e operação ficam nos READMEs de [backend](../src/backend/README.md), [frontend](../src/frontend/web/README.md), [Keycloak](../src/keycloak/README.md) e [deploy](../deploy/README.md). As regras permanentes de desenvolvimento ficam em [AGENTS.md](../AGENTS.md).

## Organização

O Verum é um monólito modular .NET, com API e workers separados. Os módulos representam contextos de negócio e mantêm domínio, aplicação, contratos e infraestrutura próprios. Cada módulo possui schema e DbContext; os mappings ficam em classes `IEntityTypeConfiguration`, aplicadas automaticamente por assembly.

CrossCutting configura DI, mensageria, cache, clientes HTTP e proteções técnicas. BuildingBlocks contém abstrações técnicas compartilhadas; regras de negócio permanecem nos módulos. Evitar interfaces sem finalidade, camadas adicionais para CRUD e duplicação de serviços já compartilhados. DTOs, opções, serviços e controllers têm lugares próprios.

Módulos se comunicam por contratos, sem consultar diretamente o DbContext de outro contexto. Chamadas externas não mantêm transações de banco abertas. Quando uma operação precisa de atomicidade entre módulos do monólito, o uso da transação compartilhada deve ser explícito, como na aceitação de busca e seu consumo de acesso.

Frontend Angular organiza infraestrutura de aplicação em Core, funcionalidades em Domain e componentes/utilitários independentes em Shared. A identidade visual, a responsividade e as regras existentes não devem mudar como efeito colateral de ajustes técnicos.

## Busca e fontes

Fluxo: identificar visitante/conta → validar acesso e cota → persistir solicitação e Outbox → RabbitMQ → Worker Descoberta → reutilizar catálogo/ofertas válidas → consultar fontes necessárias → normalizar, validar e deduplicar → classificar → persistir snapshot → acompanhar pelo mesmo ID.

Reenvio com a mesma chave recupera a solicitação sem consumir outra cota. Nenhum POST é repetido automaticamente para contornar resposta incerta. Resultado, histórico e cache respeitam o proprietário da busca; token inválido não é convertido em visitante.

O MVP seleciona até 12 ofertas elegíveis, sem paginação e sem frete. Uma fonte falhar não interrompe as demais. Pode haver menos de 12 resultados, nenhum resultado ou cobertura parcial; não completar a lista com produtos incompatíveis nem apresentar falha técnica como ausência de estoque.

KaBuM, Amazon, Kalunga e Decathlon são as fontes ativas e reutilizam o mesmo executor Playwright, navegador por processo, contextos isolados, concorrência limitada, cancelamento, hosts permitidos e diagnósticos. O perfil Carrefour permanece disponível, mas foi retirado da lista padrão após bloqueios na carga local de 29/10/2026. Não contornar CAPTCHA/bloqueios. Mercado Livre, Magalu, Casas Bahia, Netshoes e Leroy Merlin ficaram fora após bloqueios nos testes locais; acesso e seletores precisam ser revalidados quando o ambiente ou site mudar. Estante Virtual não foi incorporada porque a mistura de novos/usados exige tratamento adicional.

Rede comercial conhecida não confirma a reputação de cada vendedor de marketplace. A identidade do catálogo ainda é provisória por anúncio/URL: não pressupor equivalência entre lojas, ISBN, edições, formatos ou kits. Busca textual pode retornar obras derivadas com o título original; melhorar essa distinção depende de evolução da identificação dos produtos.

Erros pequenos de digitação podem ser tolerados em uma única palavra alfabética longa, mas números, modelos e capacidades permanecem exatos. Se a loja retornar apenas sugestões incompatíveis, descartá-las. IA é fallback para fontes insuficientes, com contratos independentes de provedor e implementação inicial OpenAI Web Search preparada. Deve permanecer desativada sem credenciais e créditos; aproveitar candidatos válidos sem aceitar resultados inventados ou sem evidência.

## Ranking

Elegibilidade vem antes da pontuação: correspondência, condição e variante compatíveis, preço identificável e oferta utilizável. Acessório não solicitado não deve competir com o produto principal nos casos cobertos. Essas regras são heurísticas, com testes; não representam interpretação semântica universal.

O ranking é determinístico e versionado. Os critérios ficam em `CriteriosRanking`, no módulo Busca. A nota combina preço (45%), confiança (35%) e atualização (20%); confiança combina qualidade inicial da loja/fonte e do dado. Atualização usa meia-vida de 60 minutos. Preço considera ofertas comparáveis e histórico com amostra suficiente; sem dados suficientes, a nota é provisória e não afirma preço justo comprovado.

Cada resultado preserva snapshot, notas parciais, explicação, fundamentos, tamanho da amostra e versão. Pesos e critérios não são parâmetros operacionais para mudanças livres em runtime. Ranking orgânico não pode ser comprado; eventual patrocínio deve ser identificado. Frete não participa do MVP.

A API mantém notas de 0–100 e o frontend as apresenta de 0–10. Não dividir novamente pesos, percentis ou valores monetários. Snapshots antigos podem não ter fundamentos; esse campo é opcional no contrato do frontend.

## Identidade, acesso e pagamentos

Keycloak é responsável por login, senha, confirmação de e-mail e recuperação de acesso. O backend valida access token e extrai claims pelo AppController. A conta comercial é sincronizada separadamente da identidade; não usar token ou redirecionamento de checkout como prova de pagamento.

Visitante possui identidade por cookie protegido e limite próprio. Conta nova recebe FREE e começa do zero, sem migrar o histórico do visitante. Limites iniciais de busca são configuráveis: visitante 1/dia, FREE 3/dia, BUSCA_PLUS e VERUM_PLUS 30/dia. A reposição segue a janela UTC informada pela API e aparece no horário local no frontend.

Pacotes pagos dependem de concessão vigente. Busca e Radar consultam Acesso, sem depender de uma consulta de cobrança a cada operação. Radar exige VERUM_PLUS; FREE e BUSCA_PLUS não incluem monitoramento. Concessões administrativas/promocionais de teste não representam pagamento confirmado.

Assinaturas são mensais com renovação automática, preços configuráveis e Mercado Pago como primeiro gateway. Contratos permitem outros provedores, mas cobrança incerta não deve mudar automaticamente de gateway: isso pode duplicá-la. Webhooks assinados são recebidos de forma durável e processados com idempotência; a confirmação validada no backend libera o acesso. Cancelamento preserva o período confirmado; não há rateio ou troca imediata de plano nesta entrega.

## Radar e notificações

Radar acompanha o anúncio, variante e condição confirmados, com a regra `preço observado <= preço-alvo`, sem frete. Disponibilidade operacional, acesso da conta e disponibilidade de venda do plano são estados independentes. Não anunciar canal, preço comercial ou capacidade que o ambiente não oferece.

A intenção temporária de criação expira em 30 minutos e não contém token. Se a oferta vencer durante login/contratação, é preciso obter e confirmar observação válida. Criação é idempotente, isolada por conta e protegida contra concorrência. Pausar ocupa vaga; encerrar libera a vaga. Metas permanecem salvas após suspensão por acesso.

O Worker Radar agenda verificações persistidas e publica na fila. Verificações do mesmo anúncio são agrupadas e protegidas por lock de sessão. Observações recentes da Busca podem atender o Radar; o executor compartilhado reconsulta somente quando necessário. Não trocar silenciosamente anúncio, capacidade, condição ou variante.

Avaliação e oportunidade são gravadas atomicamente no schema Radar antes da notificação. O encaminhamento permanece pendente até Notificações registrar o aviso idempotente pelo mesmo ID. Esse mecanismo é durável e não exige outra Outbox ou uma fila por usuário.

Aviso interno e entrega externa são registros separados. Criar Radar não ativa preferências globais de e-mail. Opt-in, fuso e horário silencioso são configurados separadamente e valem para os radares da conta. Falha de SMTP preserva a oportunidade e registra retry; oferta vencida ou que deixou de atender à condição não deve produzir aviso externo enganoso.

SMTP pode reenviar em uma queda entre aceitação e confirmação no banco; Message-ID determinístico não garante entrega exatamente uma vez. Mailpit captura mensagens locais, sem comprovar entrega a provedores reais. A aplicação usa SMTP próprio, separado do SMTP do Keycloak.

## Operação e pendências

Cada executável tem imagem e deploy independentes: frontend, API, cada worker e Keycloak. Bibliotecas não recebem deploy próprio. Migrations têm execução serializada própria, nunca no startup das réplicas. Segredos ficam em runtime; não implantar em infraestrutura inferida ou sem credenciais. Novos executáveis devem seguir o padrão do guia de deploy e AGENTS.md.

Antes da liberação comercial, configurar/homologar preços e Mercado Pago, webhook público, SMTP de produção, domínio e VPS. Contato, Termos e Privacidade atuais são demonstrações autorizadas, não documentos oficiais vigentes. Google login, validação real de IA com créditos, identificação canônica de produtos e histórico detalhado permanecem evoluções próprias.

Observabilidade foi adiada pelo usuário. A direção é OpenTelemetry, correlação entre API/fila/workers/fontes e métricas de negócio. Medição futura deve distinguir convite, intenção, checkout, pagamento confirmado no backend, monitoramento ativo, checagem, oportunidade e entrega. Evitar PII desnecessária e nunca contar retorno de checkout como pagamento confirmado. Pesquisa com compradores reais, aparelhos físicos e leitor de tela não deve ser presumida a partir de testes automatizados.
