# Backend Verum

O Verum usa um monólito modular em .NET 10, com API e workers separados. Cada módulo mantém domínio, aplicação, contratos e infraestrutura próprios. A organização preserva limites de negócio sem exigir uma interface para cada operação.

O fluxo de busca identifica visitante ou conta autenticada. Contas recebem FREE e podem usar concessões válidas de BUSCA_PLUS ou VERUM_PLUS, agora integradas à confirmação de pagamentos. O processamento do Radar e o layout de contratação permanecem para entregas próprias.

Os controllers herdam de `AppController`, que define `api/v1/[controller]`, proteção por padrão e métodos protected para obter sujeito (`sub`), e-mail, username, nome e confirmação de e-mail. Rotas geradas usam minúsculas. `BuscasController` permite visitante explicitamente. CPF não faz parte do token atual.

`POST /api/v1/contas/me` exige JWT com issuer, assinatura, audience e validade corretos, além de e-mail confirmado. Usa as claims validadas, cria ou atualiza a conta e provisiona FREE uma única vez. O índice único de `SUJEITO_IDENTIDADE` impede duplicação entre abas. Contas bloqueadas ou excluídas são rejeitadas em cada operação de busca, inclusive leitura. Nenhuma busca de visitante é migrada para a conta.

O módulo Acesso mantém BUSCA_EXECUTAR, pacotes, concessões e consumo. Os limites iniciais são visitante 1, FREE 3 e pago 30 por dia UTC. Configure `Acesso:LimiteVisitanteDiario`, `Acesso:LimiteFreeDiario` e `Acesso:LimitePagoDiario`; a preparação idempotente sincroniza esses limites sem reativar recursos, pacotes ou concessões bloqueados. `Busca:LimiteVisitanteDiario` permanece como compatibilidade quando a seção Acesso não define o limite.

Concessões futuras, expiradas, revogadas ou com pacote inativo não elevam o limite. Benefícios sobrepostos usam o maior limite, sem soma. Upgrade ou downgrade não zeram uso do dia. Sem benefício pago, prevalece FREE quando válido; FREE revogado não é concedido novamente no login. Não existe endpoint público para atribuir pacote ou desbloquear conta. Assinaturas poderá conceder benefícios futuramente por integração interna, sem ser consultada a cada busca.

`GET /api/v1/buscas/acesso` informa contexto, pacote, limite, uso, saldo e renovação. `GET /api/v1/buscas/historico` retorna somente as últimas 50 buscas da conta autenticada. Resultado, histórico e acesso não permitem cache HTTP compartilhado. Token inválido não vira visitante. As decisões de acesso e apresentação estão no [documento de arquitetura](../../docs/arquitetura.md).

## Organização

| Local                          | Responsabilidade                                                            |
| ------------------------------ | --------------------------------------------------------------------------- |
| `Apps/Verum.Api`               | Controllers, entrada HTTP, identificação do visitante e respostas           |
| `Apps/Verum.Worker.Descoberta` | Processamento assíncrono das buscas                                         |
| `Apps/Verum.Worker.Radar`      | Agendamento, verificação de metas e despacho de avisos                      |
| `Modules`                      | Busca, Catálogo, Ofertas, Radar, Contas, Acesso, Assinaturas e Notificações |
| `Verum.CrossCutting`           | Configuração compartilhada, com um arquivo por pipeline                     |
| `Verum.BuildingBlocks`         | Contratos mínimos de erros e IA compartilhados                              |
| `Tests`                        | Validações da infraestrutura e dos fluxos                                   |

Os hosts usam CrossCutting para montar a aplicação. Módulos não dependem de CrossCutting; comunicam-se por contratos e mensagens. Relacionamentos EF não atravessam schemas. Pastas vazias com finalidade futura conhecida permanecem reservadas.

Cada tipo possui seu próprio arquivo. DTOs públicos ficam em `Contratos/DTOs` ou na pasta `DTOs` da funcionalidade correspondente dentro de Contratos; DTOs internos da aplicação ficam em `Aplicacao/<funcionalidade>/DTOs`. Interfaces permanecem em Contratos, separadas dos objetos de entrada e saída. Options e classes de configuração ficam em `Configuracoes` dentro do componente responsável. Serviços permanecem em Aplicacao, controllers na API e consumidores nos workers. DTOs e opções não são definidos dentro de serviços.

Cada módulo possui DbContext, schema e histórico de migrations próprios. Os mappings são classes separadas de `IEntityTypeConfiguration<T>`, descobertas por `ApplyConfigurationsFromAssembly`. Entidades têm setters privados, construtores de criação e validadores. Navegações permanecem no mesmo arquivo, separadas visualmente; enums ficam em `Dominio/Enums`.

Tabelas e colunas usam `UPPER_SNAKE_CASE`; schemas usam minúsculas. SQL manual exige aspas duplas nos identificadores maiúsculos, por exemplo `busca."BUSCA_ETAPA"`. O código mantém uma linha em branco entre instruções, propriedades e métodos, e uma chamada por linha nos mappings; arquivos gerados por ferramentas seguem sua formatação original.

O `ServicesPipeline` registra automaticamente classes com sufixo `Service` no namespace `Aplicacao` dos módulos selecionados. O ciclo é scoped. O contrato correspondente `I...Service`, quando existente, resolve a mesma instância. Classes internas precisam de construtor público para a injeção.

## Fluxo da busca

1. `POST /api/v1/buscas` recebe consulta e chave de idempotência. Resolve a conta pelo sub validado ou o visitante pelo cookie. Verifica repetição, permissão e cota. Retorna 202 com ID e endereço para acompanhamento.
2. Uso em Acesso, busca e comando são gravados na mesma transação PostgreSQL. Os contextos compartilham conexão e transação, mas cada módulo acessa somente seu schema. Rollback não consome cota. A Outbox entrega a mensagem à fila RabbitMQ, mesmo que o worker ainda não tenha iniciado.
3. O worker reutiliza primeiro o catálogo e suas ofertas válidas. Havendo resultados suficientes, evita consultas externas.
4. Quando necessário, consulta as lojas configuradas em paralelo. KaBuM e Amazon possuem perfis Playwright. Se ainda faltarem ofertas, tenta a IA quando habilitada e configurada.
5. Valida e normaliza candidatos, preservando resultados válidos quando outra fonte falha ou demora. Catálogo e Ofertas persistem seus dados e diagnósticos em transações curtas próprias.
6. Busca grava o snapshot ordenado, finaliza o estado e invalida seu cache. `GET /api/v1/buscas/{id}` e `GET /api/v1/buscas/{id}/resultado` retornam andamento e resultado ao visitante proprietário.

Estados: `Recebida`, `Processando`, `Concluida`, `ConcluidaSemResultado`, `Falhou` e `Expirada`. Uma fonte operacional sem produtos difere de uma falha técnica de todas as fontes. O prazo global inclui a espera na fila. Resultados válidos já obtidos podem acompanhar uma busca expirada; `parcial` indica limitação ou indisponibilidade de fontes. Consultas genéricas sem especificação suficiente recebem motivo explícito para refinamento.

A identificação permanece conservadora e provisória por URL. `ranking-v1.4` usa correspondência como elegibilidade e combina preço 45%, confiança 35% e atualização 20%, igualmente para todos os contextos. Acessórios reconhecidos e não solicitados são excluídos desta seleção. Mercado e histórico comparam somente a mesma variante e condição de pagamento. Sem amostra suficiente, o preço recebe nota neutra provisória; uma oferta isolada não recebe nota máxima nem comprova preço justo. Há percentis com tolerância de 0,5%, mediana e consolidação diária por loja em uma janela de 30 dias. Histórico suficiente exige 10 observações e 3 lojas; com mercado suficiente, utiliza histórico 60% e mercado 40%. A atualização usa meia-vida de 60 minutos. Fundamentos, pesos e qualidade da amostra ficam no snapshot. Reputação externa, vendedor confirmado, validação adicional de outliers e identificação entre lojas ainda não estão completos e não são presumidos.

O cookie `verum.visitante` é protegido, HttpOnly e válido por 30 dias. A cota padrão é uma busca por dia UTC, configurável. Repetir a mesma chave e consulta retorna o mesmo ID sem consumir outra cota; usar a mesma chave com outra consulta gera conflito. A identificação é operacional: apagar cookies permite uma nova identidade. Preserve e compartilhe as chaves de ASP.NET Core Data Protection entre reinicializações e réplicas da API. Clientes web precisam enviar o cookie; CORS aceita credenciais somente nas origens explicitamente configuradas.

| Configuração `Busca`                         | Padrão                                    |
| -------------------------------------------- | ----------------------------------------- |
| `FilaSolicitacoes`                           | `verum-descoberta-busca-solicitada`       |
| `Lojas`                                      | `kabum`, `amazon`, `kalunga`, `decathlon` |
| `MinimoOfertas` / `MaximoResultados`         | 3 / 12                                    |
| `PrazoSegundos`                              | 180, incluindo a espera na fila           |
| `TimeoutFonteSegundos` / `TimeoutIaSegundos` | 45 / 20                                   |
| `LimiteVisitanteDiario`                      | 1                                         |

Mantenha fila e políticas de busca alinhadas na API e no worker. Ofertas observadas neste fluxo têm validade de 15 minutos.

A interface prepara o cookie de visitante com `GET /api/v1/buscas/visitante` antes de enviar o POST. A rota retorna 204, não cria busca nem consome cota e impede cache da resposta. Isso permite recuperar uma solicitação com a mesma chave mesmo se a resposta do primeiro POST se perder. O snapshot histórico não é alterado quando as ofertas vencem; a interface sinaliza a validade usando `ValidaAte`.

## Mensageria e concorrência

MassTransit administra conexões, envio, consumo, recuperação e retries no RabbitMQ. `ConcurrentMessageLimit` controla mensagens simultâneas por endpoint e instância; `PrefetchCount` controla mensagens antecipadas. Os padrões são 8 e 16; réplicas multiplicam a concorrência. Não é necessário outro semáforo no consumidor. Playwright possui seu próprio limite de consultas simultâneas.

| Worker     | Mensagem                     | Fila padrão                                |
| ---------- | ---------------------------- | ------------------------------------------ |
| Descoberta | `BuscaSolicitada`            | `verum-descoberta-busca-solicitada`        |
| Radar      | `VerificacaoRadarSolicitada` | `verum-radar-verificacao-radar-solicitada` |

A API utiliza Bus Outbox persistida no `BuscaDbContext`: solicitação e comando são atômicos e a entrega pendente sobrevive à reinicialização. As três tabelas técnicas do MassTransit possuem migrations. O consumidor usa advisory lock PostgreSQL por ID e ignora buscas finalizadas, evitando duplicação entre workers. Não mantém transação aberta durante consultas externas. As gravações entre módulos são independentes; uma reentrega reutiliza dados já persistidos.

Falhas elegíveis recebem retries curtos. Depois das tentativas, MassTransit envia a mensagem para `_error` e publica `Fault<T>`. `BuscaFalhouConsumer` finaliza a busca com código seguro. Falhas de providers são tratadas no próprio fluxo para preservar resultados válidos. O buffer em memória dos consumidores protege publicações de uma tentativa, mas não substitui a Outbox durável da API. O Inbox EF não envolve as consultas externas em uma transação longa.

Redelivery adiado está desabilitado. Habilitar `RabbitMQ:UseDelayedRedelivery` exige o plugin `rabbitmq_delayed_message_exchange`. O consumidor do Radar executa as verificações persistidas e preserva oportunidades e avisos para retomada após falhas.

## Cache, HTTP e erros

Redis usa cache-aside: PostgreSQL é a fonte definitiva e o cache contém DTOs descartáveis. Há métodos genéricos de leitura, gravação com TTL, remoção, existência e carregamento da origem, além de contratos para Busca, Catálogo, Ofertas e Acesso. Reservas com token e limites usam operações atômicas, mas não substituem transações ou cobrança definitiva.

Neste fluxo, a API confirma a propriedade da busca no banco antes de acessar o cache e armazena somente resultados terminais. O andamento permanece no banco. A validade das ofertas limita a do cache. Sem Redis, leituras retornam ausência e gravações falham de forma controlada; não há cache local substituto. Coordenação indisponível não concede reservas ou cotas. API e workers compartilham o prefixo Redis do ambiente.

Clientes de `IHttpClientFactory` recebem timeout total e por tentativa, retry com jitter, circuit breaker e limite de concorrência. `Http:Resilience` define o padrão; `Http:Clients:<nome>:Resilience` permite ajustes. POST, PUT, PATCH, DELETE e CONNECT não recebem retry automático. Essas políticas protegem chamadas externas; não interceptam o tráfego do navegador Playwright.

A API responde erros em Problem Details, com código e `traceId`. Validação pode incluir erros por campo; conflito retorna 409, cota excedida 429 e serviço indisponível 503. Falhas técnicas têm mensagem segura; detalhes ficam nos logs. Logs do processamento incluem `BuscaId`.

## Providers Playwright e IA

`IConsultaLoja` recebe loja e consulta de até 500 caracteres. `Microsoft.Playwright` usa Chromium headless, compartilhado por processo e iniciado sob demanda. Cada consulta possui contexto isolado, fechado ao terminar, falhar ou cancelar. Não há retry automático de navegação nem tentativa de contornar CAPTCHA.

A implementação está em `Modules/Verum.Modules.Ofertas/Infraestrutura/Descoberta/Playwright`. Configurações por loja definem URL/formulário, seletores de cards e campos, marcador de resultados vazios, paginação, limites e hosts permitidos. Use locators em vez de coordenadas. Cards inválidos são descartados individualmente. Formulários complexos, iframes ou rolagem infinita podem exigir adaptadores específicos, reutilizando o navegador compartilhado.

KaBuM está registrada como `kabum`, com limite padrão de duas páginas e 100 itens. Seu perfil extrai nome, preço PIX, condição e imagem; preço de parcela não é tratado como preço final. Sobrescritas ficam em `Playwright:Lojas:kabum`. [playwright.lojas.exemplo.json](playwright.lojas.exemplo.json) mostra uma loja fictícia e não é carregado automaticamente. Seletores dependem do layout público e precisam de manutenção quando ele mudar.

Amazon extrai preço total anunciado, sem presumir pagamento PIX, e normaliza URLs por ASIN para eliminar rastreamento e duplicatas. Seu perfil foi validado em buscas reais por monitor, televisão 4k, iphone 17 e samsung s25, junto da KaBuM. Não há tentativa de contornar CAPTCHA ou bloqueios. O diagnóstico FONTE_BLOQUEADA preserva ofertas de outras fontes. Cada perfil pode ser sobrescrito em Playwright:Lojas:<nome>.

Carrefour, Kalunga e Decathlon usam o mesmo executor, com até uma página e 48 cards por loja. Carrefour reconhece PIX quando explicitado; Kalunga exige ação de compra online e usa o título completo; Decathlon extrai somente o preço principal, sem parcelas, cashback ou desconto global presumido. Os hosts necessários são permitidos individualmente; rastreadores externos não são necessários. A lista explícita `Busca:Lojas` substitui os padrões, permitindo retirar uma fonte sem alterar código. O Radar consulta a mesma configuração, sem lista paralela de provedores.

Mercado Livre, Magalu, Casas Bahia, Netshoes e Leroy Merlin retornaram bloqueio no teste local de 26/10/2026 e não foram ativados. Acesso a uma loja hoje não garante acesso futuro ou a partir da VPS. A reputação institucional de uma rede não confirma a reputação de cada vendedor do marketplace. As decisões sobre fontes e os limites da identificação de produtos estão no [documento de arquitetura](../../docs/arquitetura.md).

Carrefour também respondeu com bloqueio em 29/10/2026 e foi retirado da lista ativa padrão e do exemplo de produção. Seu perfil permanece disponível para revalidação explícita. As quatro fontes ativas são KaBuM, Amazon, Kalunga e Decathlon.

A correspondência tolera um erro de inserção, remoção, substituição ou troca de letras adjacentes em uma única palavra alfabética longa. Números, modelos e capacidades continuam exatos; artigos e preposições comuns não eliminam um título. Não há correção arbitrária da consulta nem retry adicional em todas as lojas: se a fonte só devolver sugestões incompatíveis, elas são descartadas. O teste real mostrou correções recuperadas em papelaria e esportes e ausência segura de resultado em outro erro no Carrefour.

Development habilita Playwright no worker Descoberta. Instale o Chromium correspondente depois do build, em `src/backend`:

```powershell
pwsh Apps/Verum.Worker.Descoberta/bin/Debug/net10.0/playwright.ps1 install chromium --no-remove
```

Em containers Linux, prepare as dependências com `install --with-deps chromium` durante a construção da imagem e execute sem privilégios. Hosts permitidos, downloads desabilitados e contextos isolados não substituem isolamento de rede da implantação.

A integração IA possui contratos independentes de provider e implementação OpenAI Responses com busca web. Contexto e schema ficam separados no módulo Busca para edição. Ela permanece desabilitada e sem chave no desenvolvimento inicial; configure sua seção `InteligenciaArtificial` para habilitar. O fluxo ignora IA não configurada e preserva ofertas quando ocorrer falha ou timeout.

Entrada e interpretação aceitam até 500 caracteres. Estrutura inválida é falha da resposta; candidatos inválidos são descartados individualmente com motivo. São exigidos URL/evidência, correspondência com a consulta e preço válido. A consulta direta de IA retorna descartes detalhados; o fluxo grava diagnósticos agregados de execução da fonte. A chamada real OpenAI ainda exige validação com uma chave da aplicação.

Os limites operacionais ficam em Options, com os mesmos padrões anteriores: `Assinaturas:TimeoutProcessamentoSegundos` (60), `EventosPorCiclo` (20), `LoteReconciliacao` (50) e `JanelaReconciliacaoDias` (90); em `Notificacoes:Email`, `LoteEnvio` (20) e `AdiamentoSilencioMinutos` (15). Valores inválidos são rejeitados na validação da configuração. Os critérios fixos e versionados do ranking ficam em `CriteriosRanking`, compartilhados pelo cálculo e pelos fundamentos da resposta; não são parâmetros operacionais para ajuste livre em runtime.

## Bibliotecas

Versões ficam centralizadas em `Directory.Packages.props`.

| Biblioteca                                    | Função                                             |
| --------------------------------------------- | -------------------------------------------------- |
| ASP.NET Core / Microsoft.Extensions.Hosting   | API, workers, configuração e DI                    |
| Microsoft.AspNetCore.OpenApi                  | OpenAPI em Development                             |
| Microsoft.AspNetCore.Authentication.JwtBearer | Validação JWT do Keycloak nos endpoints protegidos |
| Entity Framework Core / Npgsql                | Mappings, migrations e PostgreSQL                  |
| Microsoft.Playwright                          | Navegação e extração de lojas                      |
| MassTransit / RabbitMQ / EntityFrameworkCore  | Mensageria, retries e Outbox persistida            |
| StackExchange.Redis / DistributedCache        | Cache e operações atômicas                         |
| Microsoft.Extensions.Http.Resilience / Polly  | Resiliência HTTP                                   |
| OpenTelemetry                                 | Logs, métricas e traces; exportação OTLP opcional  |
| xUnit / Microsoft.AspNetCore.Mvc.Testing      | Testes e hospedagem da API para integração         |

## Ambiente local e execução

Use o SDK de `global.json`: .NET 10.0.401, com atualização de patch permitida. O backend não fornece Docker Compose; disponibilize os serviços abaixo.

| Serviço        | Configuração Development                                                                                                                  |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| PostgreSQL     | `localhost:5432`, banco `verum`, usuário/senha `postgres`                                                                                 |
| RabbitMQ       | `localhost:5672`, usuário `admin`, senha `admin123`, vhost `/`                                                                            |
| Redis 6.2+     | `localhost:6379`, sem senha ou TLS                                                                                                        |
| Keycloak       | `http://localhost:8081/realms/verum`, audience `verum-api`; ambiente próprio em `src/keycloak`, necessário no endpoint protegido de conta |
| Collector OTLP | Opcional, quando exportação estiver habilitada                                                                                            |

Publique as portas Docker para executar .NET na máquina. Se os hosts também estiverem em containers, substitua `localhost` pelos nomes dos serviços da rede. Use volumes para PostgreSQL, RabbitMQ e chaves Data Protection. MassTransit declara a topologia; Redis não exige criação manual de chaves.

Os appsettings Development habilitam RabbitMQ e Redis com credenciais locais. Em produção, configure conexões, autoridade JWT, origens CORS, providers e segredos explicitamente. Variáveis de ambiente usam `__` entre níveis, como `ConnectionStrings__PostgreSQL`. `RabbitMQ:Enabled=false` permite iniciar o host, mas novas buscas retornam indisponibilidade. `Redis:Enabled=false` mantém a degradação descrita acima. OTLP depende de `Observability:OtlpEnabled` e variáveis OTEL.

O banco local possui oito schemas, 32 tabelas de negócio e três tabelas técnicas de mensageria, além dos históricos de migrations. EF cria schemas e tabelas pelas migrations; não é necessário criar schemas manualmente. Iniciar os hosts não aplica migrations. Em `src/backend`, execute `dotnet tool restore`, compile e aplique cada módulo no banco desejado:

```powershell
dotnet ef database update --project Modules/Verum.Modules.Busca --context BuscaDbContext --no-build
```

Repita para os demais módulos usando seus projetos e contextos. As factories usam `ConnectionStrings__PostgreSQL` e não iniciam serviços externos. EF e dotnet-ef estão alinhados em 10.0.12. Gere novas migrations para alterações; não edite migrations aplicadas. As migrations de Outbox e estados de descoberta foram aplicadas ao banco local `verum`.

Para executar o primeiro fluxo, em terminais separados:

```powershell
dotnet build Verum.Backend.slnx
dotnet run --project Apps/Verum.Api --launch-profile http
dotnet run --project Apps/Verum.Worker.Descoberta --launch-profile Verum.Worker.Descoberta
```

A API fica em `http://localhost:5099`. `/openapi/v1.json` está disponível em Development. Envie a busca e acompanhe o endereço retornado mantendo o cookie. A API apenas aceita e enfileira; mantenha também o worker Descoberta executando. Sem consumidor, a busca permanece recebida na fila e pode exceder seu prazo antes de ser processada. `/health/live` verifica o processo; `/health/ready` inclui MassTransit quando habilitado, mas não comprova a presença do worker, PostgreSQL, Redis, Keycloak ou providers.

## Assinaturas e pagamentos

O módulo Assinaturas usa renovação mensal automática e checkout hospedado do Mercado Pago, sem plano associado no provedor. O catálogo e o preço são definidos pelo servidor; o cliente envia somente o código do plano e uma chave de idempotência. O navegador nunca envia valor, conta ou prazo de benefício. Dados de cartão ficam no checkout do provedor.

A integração usa HttpClient e System.Text.Json do .NET, passando pela resiliência compartilhada. IGatewayAssinaturas é o contrato para incluir outros gateways; somente MercadoPagoGateway está implementado. A escolha está em Assinaturas:Gateway e cada assinatura guarda seu gateway de origem. Uma falha de cobrança não provoca troca automática de gateway: isso poderia gerar duas cobranças. Para incluir outro provedor, implemente o contrato, registre-o e estenda o enum e as constraints correspondentes.

### Fluxo e liberação

1. Conta autenticada, com e-mail confirmado e status permitido, solicita BUSCA_PLUS ou VERUM_PLUS.
2. O servidor persiste a contratação antes de criar a assinatura remota, com referência externa igual ao ID local.
3. O frontend recebe a URL do checkout e direciona o usuário. A autorização ou o retorno do navegador não libera recursos.
4. O webhook valida HMAC-SHA256, relacionando data.id, x-request-id e x-signature. Somente após persistir EVENTO_GATEWAY responde HTTP 200.
5. PagamentosWorker, hospedado na API, consulta os recursos na API do gateway, verifica vínculo da assinatura/fatura/pagamento, valor, moeda, recebedor e ambiente. Somente pagamento approved com data válida gera benefício.
6. PAGAMENTO, CONCESSAO_PACOTE e conclusão do evento são confirmados na mesma transação PostgreSQL, mantendo cada DbContext no próprio schema. A referência da concessão é o ID local do pagamento.
7. O módulo Acesso passa a reconhecer o pacote pago na próxima operação. A busca continua consultando Acesso; não chama o gateway nem Assinaturas para cada solicitação.

A liberação costuma ocorrer no próximo ciclo do processador, configurado inicialmente em cinco segundos, além do tempo de confirmação do provedor. Cada fatura aprovada libera um mês contado da data de débito da fatura; a renovação cria outro período, sem somar limites. A data de aprovação é o fallback quando a data de débito não estiver disponível. O período de um pagamento reentregue não é acrescentado novamente.

Cotas permanecem em 1/3/30 por dia, conforme as configurações de Acesso; upgrade, renovação, estorno e cancelamento não apagam o consumo. Pagamento pendente, recusado ou assinatura somente autorizada não cria concessão. Estorno, chargeback, mediação ou devolução parcial revogam o benefício daquele pagamento; outros períodos válidos continuam independentes. A política inicial não calcula acesso proporcional para devolução parcial.

O cancelamento registra uma intenção durável e retorna 202. O processador interrompe a recorrência no gateway e preserva o período já pago, que expira naturalmente. Consulte a assinatura para confirmar Cancelada. Não há troca de plano com rateio nesta entrega; uma nova contratação exige o encerramento da anterior. Conta bloqueada não pode contratar nem usar recursos; bloqueio administrativo não cancela a cobrança automaticamente.

### Idempotência, indisponibilidade e operação

A mesma chave por conta recupera a contratação existente; outro plano com a mesma chave retorna conflito. Há no máximo uma contratação pendente ou assinatura não encerrada por conta. Se a resposta do POST externo se perder, a recuperação usa external_reference, sem repetir o POST. Uma contratação sem resposta pode ser consultada em GET /api/v1/assinaturas. Se continuar sem correspondência remota, fica pendente de conciliação operacional; não é criada outra cobrança automaticamente. Não exclua esse registro para tentar novamente sem verificar o gateway.

Webhooks repetidos com a mesma identificação são deduplicados. Entregas diferentes sobre o mesmo recurso são serializadas por assinatura; pagamento, concessão e conclusão do evento são persistidos atomicamente. Reservas de sessão PostgreSQL coordenam os processadores sem manter transação aberta durante chamadas ao gateway; a conexão é liberada ao final. Falhas têm até dez tentativas com espera exponencial e limite de uma hora entre tentativas. Após isso, o evento permanece Falhou para investigação pelo EventoGatewayId; não existe endpoint público para forçar benefícios.

Uma conciliação periódica, inicialmente a cada cinco minutos, agenda até cinquenta assinaturas por rodada e consulta suas faturas, recuperando notificações perdidas e intenções de cancelamento. O rodízio considera a última data de agendamento para que uma assinatura com erro não impeça as demais. Canceladas são conciliadas rotineiramente por até noventa dias; webhooks continuam aceitos depois disso. As rotas do gateway são fixas: o receptor não segue URLs recebidas no payload. EVENTO_GATEWAY armazena somente o ID do recurso, sem cartão, credencial ou payload financeiro completo.

As novas migrations de Assinaturas e Acesso devem ser aplicadas antes de habilitar pagamentos. O início da API não aplica migrations. Tokens e segredo do webhook devem ficar em variáveis de ambiente ou no gerenciador de segredos; nunca no Angular ou em arquivo versionado.

### Configurar Mercado Pago Developers

Crie uma aplicação própria do Verum em [Suas integrações](https://www.mercadopago.com.br/developers/panel/app). Use Assinaturas com checkout hospedado e recorrência mensal; esta implementação não utiliza Checkout Pro de pagamento avulso nem exige cadastrar preapproval_plan no painel. A criação segue o fluxo oficial de [assinatura com pagamento pendente](https://www.mercadopago.com.br/developers/pt/docs/subscriptions/integration-configuration/subscription-no-associated-plan/pending-payments).

Configure uma URL pública HTTPS para o receptor /api/v1/webhooks/mercadopago. Localhost não recebe notificações externas; em desenvolvimento use um túnel HTTPS para a API. Configure também uma URL HTTPS de retorno do frontend. O retorno pode usar a página inicial existente enquanto não houver layout de assinatura.

Ative os tópicos payment, subscription_preapproval e subscription_authorized_payment. Utilize Webhooks assinados e gere o segredo da aplicação. A documentação de Assinaturas orienta a configuração por notification_url durante a criação; o adapter já envia esse campo. Confirme os eventos disponíveis e sua entrega no painel da aplicação. Não substitua Webhooks por IPN, cuja autenticação não segue este contrato. Referência: [notificações oficiais](https://www.mercadopago.com.br/developers/pt/docs/subscriptions/additional-content/your-integrations/notifications/webhooks).

| Configuração da API                     | Preenchimento                                                      |
| --------------------------------------- | ------------------------------------------------------------------ |
| Assinaturas__Habilitado                 | true somente após configurar e migrar                              |
| Assinaturas__Gateway                    | MercadoPago                                                        |
| Assinaturas__PrecosMensais__BUSCA_PLUS  | Valor mensal aprovado, com ponto decimal                           |
| Assinaturas__PrecosMensais__VERUM_PLUS  | Valor mensal aprovado, com ponto decimal                           |
| Assinaturas__MercadoPago__AccessToken   | Access Token privado da conta vendedora do ambiente                |
| Assinaturas__MercadoPago__WebhookSecret | Segredo Webhooks da aplicação correspondente                       |
| Assinaturas__MercadoPago__CollectorId   | User ID do vendedor que recebe a cobrança                          |
| Assinaturas__MercadoPago__Producao      | false em testes; true apenas em produção                           |
| Assinaturas__MercadoPago__UrlRetorno    | URL pública HTTPS do frontend                                      |
| Assinaturas__MercadoPago__UrlWebhook    | URL pública HTTPS da API terminada em /api/v1/webhooks/mercadopago |

Valores zero mantêm o plano fora do checkout. Os preços de testes automatizados são fictícios e não definem a política comercial. Credenciais, vendedor e comprador devem pertencer ao mesmo ambiente/país. Use comprador de teste diferente do vendedor e cartões de teste seguindo o [roteiro oficial de compra](https://www.mercadopago.com.br/developers/pt/docs/subscriptions/integration-test/test-purchase); não use cartão real para a homologação. O fluxo hospedado não precisa de Public Key no Angular.

Antes de produção, conclua uma contratação no ambiente do provedor, confirme assinatura e cobrança na API e a liberação em GET /api/v1/buscas/acesso, depois valide renovação, recusa, cancelamento e estorno. O simulador de notificações sozinho não comprova pagamento: um ID inexistente nunca deve liberar acesso. Use credenciais e segredo próprios de produção e outra URL/ambiente para a homologação.

Pagamentos permanecem desabilitados no arquivo padrão, com preços e credenciais vazios. Os testes automatizados usam HTTP simulado do gateway e PostgreSQL real; não substituem a homologação no Mercado Pago. A tela de conta utiliza os contratos de contratação e acompanha a confirmação pelo backend.

Nos testes manuais, use uma conta Verum de homologação com o e-mail do comprador de teste do Mercado Pago, pois o backend usa o e-mail confirmado da conta como payer_email. Confirme esse e-mail pelo Mailpit local e abra o checkout em uma sessão separada da conta vendedora.

A regressão de pagamentos inclui 200 entregas de webhook com quatro processadores, confirmação única de pagamento/benefício, rollback e ausência de transação durante HTTP. Instale o SDK indicado no `global.json`; os resultados atuais devem ser obtidos pelo CI e pela execução local, sem depender de contagens históricas deste documento.

## Validação

`dotnet test Verum.Backend.slnx` executa a suíte; integrações externas são opcionais e exigem variáveis:

| Variável                            | Habilitação                                                                                                                                            |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `VERUM_TEST_POSTGRES`               | Conexão PostgreSQL de desenvolvimento                                                                                                                  |
| `VERUM_TEST_REDIS`                  | Conexão Redis de desenvolvimento                                                                                                                       |
| `VERUM_TEST_PLAYWRIGHT=1`           | Chromium real com páginas locais simuladas                                                                                                             |
| `VERUM_TEST_NOVAS_LOJAS=1`          | Três consultas reais na KaBuM e Amazon, com normalização e seleção de até 12 ofertas                                                                   |
| `VERUM_TEST_RABBITMQ=1`             | Broker local e management em 15672, junto de PostgreSQL                                                                                                |
| `VERUM_TEST_KABUM=1`                | Verificação opcional do site real                                                                                                                      |
| `VERUM_TEST_PRODUTOS_ESPECIFICOS=1` | Livros, cabo SCSI, SSD com modelo e consulta inexistente nas duas lojas reais                                                                          |
| `VERUM_TEST_CARGA_REAL=1`           | 50 buscas com lojas reais, PostgreSQL/RabbitMQ, visitantes/contas e criação concorrente de alertas; exige também as variáveis de PostgreSQL e RabbitMQ |
| `VERUM_RELATORIO_CARGA`             | Caminho JSON do relatório da carga; por padrão usa o diretório temporário do sistema                                                                   |

Sem habilitação, os testes correspondentes são ignorados; serviços habilitados indisponíveis causam falha. Os testes do fluxo criam bancos temporários próprios, aplicam migrations e os removem; precisam de permissão para criar bancos. Outros testes usam schemas transacionais. Redis e RabbitMQ usam recursos exclusivos dos testes, removidos ao terminar. O teste RabbitMQ confirma entrega durável antes da inicialização do worker.

A validação cobre API/cookie/cota, idempotência, persistência, reutilização de ofertas, mensagens, concorrência, fontes parciais, IA simulada, timeout e erros. O teste de Contas usa PostgreSQL temporário para verificar sincronização concorrente, atualização de perfil e bloqueio. A verificação KaBuM real consulta produtos, paginação e ausência de resultados sem gravar ofertas. O teste JWT real é optativo e exige tokens de acesso e identidade obtidos com PKCE nas variáveis `VERUM_TEST_KEYCLOAK_ACCESS_TOKEN` e `VERUM_TEST_KEYCLOAK_ID_TOKEN`; tokens não devem ser salvos no repositório. IA real não é comprovada pelas simulações.

Para repetir a carga, execute `dotnet test Tests/Verum.Tests.Integracao --filter FullyQualifiedName~CargaLocalRealTests` com as variáveis acima. O cenário usa banco e topologia descartáveis, duas buscas simultâneas, executor Playwright limitado a duas navegações e cinco POSTs concorrentes por alerta para testar idempotência. A autenticação é substituída somente no host de teste; não comprova Keycloak real. As concessões administrativas são exclusivas do banco descartável e não representam pagamento. IA, pagamentos reais e envio externo de e-mail permanecem desativados. O relatório registra resultados vazios e falhas de fontes; não representa um benchmark estatístico nem validação semântica universal.

A migration `RemoverCamposPagamentoSemUso` remove `METODO` e `VENCIMENTO_EM`, nunca preenchidos pelo fluxo recorrente atual. Ela aborta caso encontre valores nessas colunas, para impedir descarte silencioso. Não houve remoção de tabelas/colunas destinadas às funcionalidades futuras documentadas.

A migration `AlinharLimitesNomesEConsultas` amplia nome de produto para 300 caracteres e termos do catálogo para 500, acompanhando os contratos da coleta e da busca. Os limites são preservados sem truncar dados recebidos. O ranking v1.4 amplia a exclusão de peças/acessórios não solicitados; as regras continuam heurísticas e não identificam universalmente ISBN, edições ou kits.

## Deploy

API e cada worker possuem imagens e deploys independentes para VPS. As migrations executam em imagem própria, com trava compartilhada. Consulte o [guia de implantação](../../deploy/README.md) para configurar o GHCR, a VPS e os secrets do GitHub Actions.

A gestão autenticada em /conta utiliza GET /api/v1/contas/me para leitura do perfil por identidade do token, com projeção e AsNoTracking. O GET não sincroniza nem altera a conta; POST /api/v1/contas/me mantém essa responsabilidade. Plano, uso e concessão são consultados em Busca/Acesso, e o estado da assinatura pelos contratos existentes de Assinaturas. Nenhuma migration é necessária para essa tela.

## Radar e avisos

O Radar possui criação e gestão de metas por anúncio confirmado, acesso por concessão Verum Plus, verificações agrupadas e oportunidades persistidas. O Worker Radar utiliza os mesmos providers de Ofertas da Busca e também despacha os e-mails via MailKit. Free e Busca Plus não incluem Radar. Sem configuração de SMTP, os avisos permanecem disponíveis na plataforma e as entregas pendentes.

As opções `Radar` e `Notificacoes:Email` estão nos appsettings e nos exemplos de deploy da API/Worker Radar. Development utiliza Mailpit em localhost:1025; produção começa desabilitada. Configure política comercial, preços, credenciais e webhook antes da liberação. Migrations continuam com execução independente.

Consulte as [decisões de arquitetura do Radar](../../docs/arquitetura.md#radar-e-notificações) para os limites de identidade do anúncio, encaminhamento durável e entrega de notificações.
