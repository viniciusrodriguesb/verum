# Backend Verum

O Verum usa um monólito modular em .NET 10, com API e workers separados. Cada módulo mantém domínio, aplicação, contratos e infraestrutura próprios. A organização preserva limites de negócio sem exigir uma interface para cada operação.

O primeiro fluxo de busca para visitante e a integração de identidade com Keycloak estão implementados. Contas são vinculadas à identidade real; planos, concessões de acesso, busca por conta e processamento do Radar permanecem para as próximas entregas.

Os controllers herdam de `AppController`, que define `api/v1/[controller]`, proteção por padrão e métodos protected para obter sujeito (`sub`), e-mail, username, nome e confirmação de e-mail. Rotas geradas usam minúsculas. `BuscasController` permite visitante explicitamente. CPF não faz parte do token atual.

`POST /api/v1/contas/me` exige JWT com issuer, assinatura, audience e validade corretos, além de e-mail confirmado. Não recebe dados de perfil no corpo: usa as claims validadas, cria uma conta no primeiro acesso e atualiza o perfil nos seguintes. O índice único de `SUJEITO_IDENTIDADE` impede duplicação entre abas. Contas bloqueadas ou excluídas são rejeitadas. Não concede planos nem transforma uma busca de visitante em busca autenticada.

## Organização

| Local | Responsabilidade |
| --- | --- |
| `Apps/Verum.Api` | Controllers, entrada HTTP, identificação do visitante e respostas |
| `Apps/Verum.Worker.Descoberta` | Processamento assíncrono das buscas |
| `Apps/Verum.Worker.Radar` | Host e consumidor preparados para monitoramento futuro |
| `Modules` | Busca, Catálogo, Ofertas, Radar, Contas, Acesso, Assinaturas e Notificações |
| `Verum.CrossCutting` | Configuração compartilhada, com um arquivo por pipeline |
| `Verum.BuildingBlocks` | Contratos mínimos de erros e IA compartilhados |
| `Tests` | Validações da infraestrutura e dos fluxos |

Os hosts usam CrossCutting para montar a aplicação. Módulos não dependem de CrossCutting; comunicam-se por contratos e mensagens. Relacionamentos EF não atravessam schemas. Pastas vazias com finalidade futura conhecida permanecem reservadas.

Cada tipo possui seu próprio arquivo. DTOs públicos ficam em `Contratos/DTOs` ou na pasta `DTOs` da funcionalidade correspondente dentro de Contratos; DTOs internos da aplicação ficam em `Aplicacao/<funcionalidade>/DTOs`. Interfaces permanecem em Contratos, separadas dos objetos de entrada e saída. Options e classes de configuração ficam em `Configuracoes` dentro do componente responsável. Serviços permanecem em Aplicacao, controllers na API e consumidores nos workers. DTOs e opções não são definidos dentro de serviços.

Cada módulo possui DbContext, schema e histórico de migrations próprios. Os mappings são classes separadas de `IEntityTypeConfiguration<T>`, descobertas por `ApplyConfigurationsFromAssembly`. Entidades têm setters privados, construtores de criação e validadores. Navegações permanecem no mesmo arquivo, separadas visualmente; enums ficam em `Dominio/Enums`.

Tabelas e colunas usam `UPPER_SNAKE_CASE`; schemas usam minúsculas. SQL manual exige aspas duplas nos identificadores maiúsculos, por exemplo `busca."BUSCA_ETAPA"`. O código mantém uma linha em branco entre instruções, propriedades e métodos, e uma chamada por linha nos mappings; arquivos gerados por ferramentas seguem sua formatação original.

O `ServicesPipeline` registra automaticamente classes com sufixo `Service` no namespace `Aplicacao` dos módulos selecionados. O ciclo é scoped. O contrato correspondente `I...Service`, quando existente, resolve a mesma instância. Classes internas precisam de construtor público para a injeção.

## Fluxo da busca

1. `POST /api/v1/buscas` recebe `consulta` e `chaveIdempotencia`. Valida entrada, identifica o visitante por cookie protegido e verifica repetição e cota. Retorna `202` com ID e endereço para acompanhamento.
2. Busca e comando são gravados na mesma transação PostgreSQL. A Outbox entrega a mensagem à fila RabbitMQ, mesmo que o worker ainda não tenha iniciado.
3. O worker reutiliza primeiro o catálogo e suas ofertas válidas. Havendo resultados suficientes, evita consultas externas.
4. Quando necessário, consulta as lojas configuradas em paralelo. KaBuM e Amazon possuem perfis Playwright. Se ainda faltarem ofertas, tenta a IA quando habilitada e configurada.
5. Valida e normaliza candidatos, preservando resultados válidos quando outra fonte falha ou demora. Catálogo e Ofertas persistem seus dados e diagnósticos em transações curtas próprias.
6. Busca grava o snapshot ordenado, finaliza o estado e invalida seu cache. `GET /api/v1/buscas/{id}` e `GET /api/v1/buscas/{id}/resultado` retornam andamento e resultado ao visitante proprietário.

Estados: `Recebida`, `Processando`, `Concluida`, `ConcluidaSemResultado`, `Falhou` e `Expirada`. Uma fonte operacional sem produtos difere de uma falha técnica de todas as fontes. O prazo global inclui a espera na fila. Resultados válidos já obtidos podem acompanhar uma busca expirada; `parcial` indica limitação ou indisponibilidade de fontes. Consultas genéricas sem especificação suficiente recebem motivo explícito para refinamento.

A identificação de produtos é conservadora: os termos da consulta precisam aparecer no título, e cada URL mantém uma identidade provisória. Não unificamos automaticamente produtos de lojas diferentes sem evidência suficiente. Marca e categoria desconhecidas ficam explícitas. O ranking prioriza relevância textual (70%) e combina preço dentro da mesma variante (9%), confiança inicial da fonte/loja (12%) e atualização (9%). Reconhece PS5 como PlayStation 5 e televisão/televisor como TV. Reduz a prioridade de acessórios quando não solicitados, inclusive quando a marca aparece antes do tipo de acessório. É uma regra conservadora, não uma classificação universal de categorias. Uma nota máxima de preço com apenas uma oferta não comprova desconto. As ofertas são alternativas; o fluxo não afirma equivalência apenas por semelhança de nome.

O cookie `verum.visitante` é protegido, HttpOnly e válido por 30 dias. A cota padrão é uma busca por dia UTC, configurável. Repetir a mesma chave e consulta retorna o mesmo ID sem consumir outra cota; usar a mesma chave com outra consulta gera conflito. A identificação é operacional: apagar cookies permite uma nova identidade. Preserve e compartilhe as chaves de ASP.NET Core Data Protection entre reinicializações e réplicas da API. Clientes web precisam enviar o cookie; CORS aceita credenciais somente nas origens explicitamente configuradas.

| Configuração `Busca` | Padrão |
| --- | --- |
| `FilaSolicitacoes` | `verum-descoberta-busca-solicitada` |
| `Lojas` | `kabum`, `amazon` |
| `MinimoOfertas` / `MaximoResultados` | 3 / 12 |
| `PrazoSegundos` | 180, incluindo a espera na fila |
| `TimeoutFonteSegundos` / `TimeoutIaSegundos` | 45 / 20 |
| `LimiteVisitanteDiario` | 1 |

Mantenha fila e políticas de busca alinhadas na API e no worker. Ofertas observadas neste fluxo têm validade de 15 minutos.

A interface prepara o cookie de visitante com `GET /api/v1/buscas/visitante` antes de enviar o POST. A rota retorna 204, não cria busca nem consome cota e impede cache da resposta. Isso permite recuperar uma solicitação com a mesma chave mesmo se a resposta do primeiro POST se perder. O snapshot histórico não é alterado quando as ofertas vencem; a interface sinaliza a validade usando `ValidaAte`.

## Mensageria e concorrência

MassTransit administra conexões, envio, consumo, recuperação e retries no RabbitMQ. `ConcurrentMessageLimit` controla mensagens simultâneas por endpoint e instância; `PrefetchCount` controla mensagens antecipadas. Os padrões são 8 e 16; réplicas multiplicam a concorrência. Não é necessário outro semáforo no consumidor. Playwright possui seu próprio limite de consultas simultâneas.

| Worker | Mensagem | Fila padrão |
| --- | --- | --- |
| Descoberta | `BuscaSolicitada` | `verum-descoberta-busca-solicitada` |
| Radar | `VerificacaoRadarSolicitada` | `verum-radar-verificacao-radar-solicitada` |

A API utiliza Bus Outbox persistida no `BuscaDbContext`: solicitação e comando são atômicos e a entrega pendente sobrevive à reinicialização. As três tabelas técnicas do MassTransit possuem migrations. O consumidor usa advisory lock PostgreSQL por ID e ignora buscas finalizadas, evitando duplicação entre workers. Não mantém transação aberta durante consultas externas. As gravações entre módulos são independentes; uma reentrega reutiliza dados já persistidos.

Falhas elegíveis recebem retries curtos. Depois das tentativas, MassTransit envia a mensagem para `_error` e publica `Fault<T>`. `BuscaFalhouConsumer` finaliza a busca com código seguro. Falhas de providers são tratadas no próprio fluxo para preservar resultados válidos. O buffer em memória dos consumidores protege publicações de uma tentativa, mas não substitui a Outbox durável da API. O Inbox EF não envolve as consultas externas em uma transação longa.

Redelivery adiado está desabilitado. Habilitar `RabbitMQ:UseDelayedRedelivery` exige o plugin `rabbitmq_delayed_message_exchange`. O processamento do Radar ainda lança `ProcessamentoNaoConfiguradoException` até sua implementação.

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

Development habilita Playwright no worker Descoberta. Instale o Chromium correspondente depois do build, em `src/backend`:

```powershell
pwsh Apps/Verum.Worker.Descoberta/bin/Debug/net10.0/playwright.ps1 install chromium --no-remove
```

Em containers Linux, prepare as dependências com `install --with-deps chromium` durante a construção da imagem e execute sem privilégios. Hosts permitidos, downloads desabilitados e contextos isolados não substituem isolamento de rede da implantação.

A integração IA possui contratos independentes de provider e implementação OpenAI Responses com busca web. Contexto e schema ficam separados no módulo Busca para edição. Ela permanece desabilitada e sem chave no desenvolvimento inicial; configure sua seção `InteligenciaArtificial` para habilitar. O fluxo ignora IA não configurada e preserva ofertas quando ocorrer falha ou timeout.

Entrada e interpretação aceitam até 500 caracteres. Estrutura inválida é falha da resposta; candidatos inválidos são descartados individualmente com motivo. São exigidos URL/evidência, correspondência com a consulta e preço válido. A consulta direta de IA retorna descartes detalhados; o fluxo grava diagnósticos agregados de execução da fonte. A chamada real OpenAI ainda exige validação com uma chave da aplicação.

## Bibliotecas

Versões ficam centralizadas em `Directory.Packages.props`.

| Biblioteca | Função |
| --- | --- |
| ASP.NET Core / Microsoft.Extensions.Hosting | API, workers, configuração e DI |
| Microsoft.AspNetCore.OpenApi | OpenAPI em Development |
| Microsoft.AspNetCore.Authentication.JwtBearer | Validação JWT do Keycloak nos endpoints protegidos |
| Entity Framework Core / Npgsql | Mappings, migrations e PostgreSQL |
| Microsoft.Playwright | Navegação e extração de lojas |
| MassTransit / RabbitMQ / EntityFrameworkCore | Mensageria, retries e Outbox persistida |
| StackExchange.Redis / DistributedCache | Cache e operações atômicas |
| Microsoft.Extensions.Http.Resilience / Polly | Resiliência HTTP |
| OpenTelemetry | Logs, métricas e traces; exportação OTLP opcional |
| xUnit / Microsoft.AspNetCore.Mvc.Testing | Testes e hospedagem da API para integração |

## Ambiente local e execução

Use o SDK de `global.json`: .NET 10.0.401, com atualização de patch permitida. O backend não fornece Docker Compose; disponibilize os serviços abaixo.

| Serviço | Configuração Development |
| --- | --- |
| PostgreSQL | `localhost:5432`, banco `verum`, usuário/senha `postgres` |
| RabbitMQ | `localhost:5672`, usuário `admin`, senha `admin123`, vhost `/` |
| Redis 6.2+ | `localhost:6379`, sem senha ou TLS |
| Keycloak | `http://localhost:8081/realms/verum`, audience `verum-api`; ambiente próprio em `src/keycloak`, necessário no endpoint protegido de conta |
| Collector OTLP | Opcional, quando exportação estiver habilitada |

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

## Validação

`dotnet test Verum.Backend.slnx` executa a suíte; integrações externas são opcionais e exigem variáveis:

| Variável | Habilitação |
| --- | --- |
| `VERUM_TEST_POSTGRES` | Conexão PostgreSQL de desenvolvimento |
| `VERUM_TEST_REDIS` | Conexão Redis de desenvolvimento |
| `VERUM_TEST_PLAYWRIGHT=1` | Chromium real com páginas locais simuladas |
| `VERUM_TEST_NOVAS_LOJAS=1` | Três consultas reais na KaBuM e Amazon, com normalização e seleção de até 12 ofertas |
| `VERUM_TEST_RABBITMQ=1` | Broker local e management em 15672, junto de PostgreSQL |
| `VERUM_TEST_KABUM=1` | Verificação opcional do site real |
| `VERUM_TEST_PRODUTOS_ESPECIFICOS=1` | Livros, cabo SCSI, SSD com modelo e consulta inexistente nas duas lojas reais |

Sem habilitação, os testes correspondentes são ignorados; serviços habilitados indisponíveis causam falha. Os testes do fluxo criam bancos temporários próprios, aplicam migrations e os removem; precisam de permissão para criar bancos. Outros testes usam schemas transacionais. Redis e RabbitMQ usam recursos exclusivos dos testes, removidos ao terminar. O teste RabbitMQ confirma entrega durável antes da inicialização do worker.

A validação cobre API/cookie/cota, idempotência, persistência, reutilização de ofertas, mensagens, concorrência, fontes parciais, IA simulada, timeout e erros. O teste de Contas usa PostgreSQL temporário para verificar sincronização concorrente, atualização de perfil e bloqueio. A verificação KaBuM real consulta produtos, paginação e ausência de resultados sem gravar ofertas. O teste JWT real é optativo e exige tokens de acesso e identidade obtidos com PKCE nas variáveis `VERUM_TEST_KEYCLOAK_ACCESS_TOKEN` e `VERUM_TEST_KEYCLOAK_ID_TOKEN`; tokens não devem ser salvos no repositório. IA real não é comprovada pelas simulações.

Validação local: 171 testes aprovados com serviços locais e um teste JWT optativo ignorado na execução geral. O teste adicional pelo navegador validou login PKCE do Angular, JWT real na API, sincronização de conta, restauração SSO, logout e proteção da rota Radar. Buscas reais de livros e itens específicos foram verificadas como visitante, com login e após logout, incluindo idempotência, isolamento entre navegadores e cota. Códigos de modelo permanecem inteiros na comparação; capacidades como `1TB` e `1 TB` são equivalentes. Ausência de resultados usa o marcador explícito da loja; layout desconhecido continua sendo falha técnica. Não foi necessária nova migration.
