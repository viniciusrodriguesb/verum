# Backend Verum

O Verum usa um **monólito modular em .NET 10**, com uma API e dois workers executados separadamente. O código é organizado por contexto de negócio, com uma Clean Architecture enxuta dentro de cada módulo. O objetivo é permitir evolução e escala sem exigir interfaces ou projetos extras para cada operação.

A base técnica já contém entidades, mappings, persistência por módulo, cache, mensageria, autenticação JWT, proteção HTTP e tratamento de erros. Endpoints e casos de uso de negócio, processamento das mensagens, agendamentos, migrations e integrações com provedores ainda serão implementados.

## Organização

| Local | Responsabilidade |
| --- | --- |
| `Apps/Verum.Api` | Entrada HTTP, controllers, autenticação e respostas da API |
| `Apps/Verum.Worker.Descoberta` | Consumo das solicitações de busca e futuro processamento de descoberta |
| `Apps/Verum.Worker.Radar` | Consumo das verificações de monitoramento e futuros agendamentos |
| `Modules` | Busca, Catálogo, Ofertas, Radar, Contas, Acesso, Assinaturas e Notificações |
| `Verum.CrossCutting` | Composição de dependências e infraestrutura compartilhada, com um arquivo por pipeline |
| `Verum.BuildingBlocks` | Contrato mínimo de erros esperados, compartilhado entre módulos e API, sem dependências externas |
| `Tests` | Testes da base e pastas reservadas para as demais categorias |

Dentro de cada módulo, `Dominio` mantém entidades, validações e enums; `Aplicacao` recebe os casos de uso; `Contratos` define a comunicação com outros componentes; `Infraestrutura` implementa a persistência. Pastas vazias com responsabilidade futura conhecida permanecem reservadas.

Os hosts usam CrossCutting para montar a aplicação. **Módulos não dependem de CrossCutting**: definem seus contratos e preservam suas regras. Integrações entre módulos usam contratos e eventos; relacionamentos EF não atravessam seus schemas. BuildingBlocks contém somente os erros compartilhados, sem bases genéricas de entidades ou serviços.

A API registra os oito módulos. Descoberta registra Busca, Catálogo e Ofertas. Radar registra Radar, Catálogo, Ofertas e Notificações. Cada worker registra explicitamente seus consumidores.

## Fluxo do início ao fim

O fluxo previsto para uma funcionalidade é:

1. A requisição entra na API, passa pelos middlewares, CORS, autenticação e autorização e chega ao controller.
2. O controller delega ao serviço de aplicação do módulo, que coordena regras de domínio, persistência, cache e integrações.
3. PostgreSQL mantém os dados definitivos. Redis pode fornecer uma projeção temporária; em uma ausência de cache, o caso de uso consulta a origem e atualiza o cache.
4. Trabalho assíncrono é publicado pelo MassTransit no RabbitMQ. O worker responsável recebe a mensagem e delega o processamento aos módulos.
5. O processamento consulta provedores por clientes HTTP protegidos, persiste o resultado e atualiza ou invalida as projeções. A API poderá expor andamento e resultado ao cliente.
6. Falhas HTTP recebem respostas padronizadas; falhas de consumo seguem o tratamento do MassTransit. Logs e rastreamento permitem acompanhar a execução.

Essa sequência descreve o desenho do produto. Atualmente, os consumidores recebem e validam os IDs, mas lançam `ProcessamentoNaoConfiguradoException` enquanto o processamento real não existir. A mensagem vai para a fila de erro, evitando confirmar um trabalho que não foi realizado.

## Padrões da implementação

### Domínio, persistência e injeção de dependências

Cada módulo possui seu próprio DbContext, schema PostgreSQL e histórico de migrations. Os mappings ficam em classes `IEntityTypeConfiguration<T>` separadas e são descobertos por `ApplyConfigurationsFromAssembly`, mantendo o contexto limpo.

Entidades possuem setters privados, construtores de criação e validações. Navegações permanecem no mesmo arquivo, separadas visualmente das propriedades. Enums ficam em `Dominio/Enums`. O código usa uma linha em branco entre propriedades, instruções e métodos, duas antes das navegações e uma chamada por linha nas cadeias dos mappings.

Tabelas e colunas usam `UPPER_SNAKE_CASE`; schemas usam minúsculas. O EF gera os identificadores corretamente. Em SQL manual, os nomes maiúsculos exigem aspas duplas, como `busca."BUSCA_ETAPA"`.

O `ServicesPipeline` registra automaticamente classes concretas com sufixo `Service` no segmento de namespace `Aplicacao` dos módulos selecionados. O ciclo de vida é scoped. Interfaces são opcionais; quando existe o contrato correspondente `I...Service`, ele resolve a mesma instância. Classes internas precisam de construtores públicos para a injeção. Um novo módulo entra na composição uma vez, sem registros individuais para cada serviço.

### Mensageria e concorrência

MassTransit administra conexões, publicação, confirmações, consumo e recuperação do transporte RabbitMQ. O publicador genérico utiliza o contexto scoped e um prazo configurável de publicação, sem um segundo ciclo de retry.

| Worker | Mensagem | Fila |
| --- | --- | --- |
| Descoberta | `BuscaSolicitada`, com `BuscaId` | `verum-descoberta-busca-solicitada` |
| Radar | `VerificacaoRadarSolicitada`, com `MonitoramentoId` | `verum-radar-verificacao-radar-solicitada` |

`ConcurrentMessageLimit` controla mensagens simultâneas por endpoint e instância; `PrefetchCount` controla mensagens antecipadas do broker. Os padrões são 8 e 16. Réplicas aumentam a concorrência total. Não há semáforo adicional nem garantia de ordenação ou exclusão por ID.

Falhas elegíveis recebem retries curtos. Erros esperados de aplicação, argumentos inválidos, cancelamento e processamento ainda não configurado não recebem retry/redelivery. Ao esgotar as tentativas, o MassTransit encaminha para `_error` e publica `Fault<T>`. Consumidores precisam ser idempotentes, pois mensagens podem ser entregues novamente.

O Outbox em memória está habilitado: retém publicações de uma tentativa até o sucesso, mas não é durável nem torna alterações no banco atômicas com mensagens. Existem helpers para Outbox/Inbox com EF Core, porém ativação, tabelas técnicas, migrations e composição transacional estão pendentes. Sua adoção exige definir o contexto proprietário e substituir o buffer em memória.

### Cache Redis

O padrão é **cache-aside**: PostgreSQL continua sendo a fonte definitiva e Redis guarda dados descartáveis. Os módulos acessam contratos próprios; CrossCutting implementa a integração com Redis.

Há operações genéricas de leitura, gravação com validade, remoção, existência e obtenção com carregamento da origem. O cache armazena DTOs, não entidades EF com navegações, e compartilha uma conexão multiplexada por processo.

| Uso no produto | Conteúdo |
| --- | --- |
| Busca | Status e resultado temporários |
| Catálogo | Variantes e resolução de consultas |
| Ofertas | Ofertas atuais e reserva de atualização por variante/fonte |
| Acesso | Permissões temporárias e limites operacionais por conta ou visitante |

TTLs são configuráveis. A validade de ofertas, resultados e permissões limita o prazo do cache; leituras não o renovam. Alterações devem invalidar as projeções depois do commit. Autorização e dados que exigem consistência estrita devem ser confirmados no módulo proprietário.

Reservas com token e limites usam operações atômicas no Redis. Reservas reduzem consultas externas duplicadas, mas não substituem transações ou idempotência. Limites operacionais não representam cobrança ou consumo definitivo. A contenção de carregamento do cache genérico é local ao processo; a reserva de Ofertas coordena processos distintos.

Com Redis desabilitado ou indisponível, leituras de cache retornam ausência e gravações retornam `false`; coordenação falha explicitamente, sem conceder reservas ou cotas. Não há cache em memória substituto. API e workers compartilham o prefixo Redis do ambiente, com prefixos distintos entre ambientes.

### Chamadas externas e erros da API

Clientes criados por `IHttpClientFactory` recebem timeout total e por tentativa, retry exponencial com jitter, circuit breaker e controle de concorrência de saída. Cada cliente mantém seu próprio circuito. Use um nome estável por integração e não empilhe outro handler de resiliência.

`Http:Resilience` define o padrão; `Http:Clients:<nome>:Resilience` permite ajustes por cliente. `RetryEnabled` e `CircuitBreakerEnabled` podem ser definidos globalmente ou por cliente. Reinicie o host após alterar essas políticas. POST, PUT, PATCH, DELETE e CONNECT não recebem retry automático. A proteção se aplica somente a chamadas externas; não foi adicionado limitador de entrada na API.

A API responde erros em Problem Details, com status, descrição, código e `traceId`; validações podem incluir erros por campo. `ErroAplicacaoException` representa validação, ausência, conflito ou acesso negado, sem acoplar o domínio a códigos HTTP. Mensagens esperadas devem ser seguras para exposição. Falhas técnicas retornam descrições genéricas, com detalhes internos apenas nos logs. Nos workers, falhas seguem o fluxo de mensageria.

## Bibliotecas utilizadas

As versões ficam centralizadas em `Directory.Packages.props`; cada projeto declara os pacotes que utiliza.

| Biblioteca | Papel |
| --- | --- |
| ASP.NET Core e Microsoft.Extensions.Hosting | API, workers, configuração e injeção de dependências |
| Microsoft.AspNetCore.OpenApi | Documento OpenAPI da API em Development |
| Microsoft.AspNetCore.Authentication.JwtBearer | Validação de tokens JWT emitidos pelo Keycloak |
| Entity Framework Core + Npgsql | Mapeamento das entidades e acesso ao PostgreSQL |
| MassTransit + RabbitMQ | Publicação e consumo de mensagens, retries e tratamento de falhas |
| MassTransit.EntityFrameworkCore | Suporte ao futuro Outbox/Inbox persistido no PostgreSQL |
| StackExchange.Redis + Microsoft.Extensions.Caching.StackExchangeRedis | Cache, coordenação atômica e integração com `IDistributedCache` |
| Microsoft.Extensions.Http.Resilience + Polly | Proteção configurável das chamadas HTTP externas |
| OpenTelemetry | Logs, métricas e rastreamento; exportação OTLP opcional |
| xUnit, Microsoft.NET.Test.Sdk e Microsoft.AspNetCore.Mvc.Testing | Execução dos testes e hospedagem da API em testes |

## Dependências e configuração local

Instale o SDK indicado em `global.json` (10.0.401, com atualização de patch permitida). Docker pode hospedar as dependências abaixo; serviços instalados diretamente também atendem. O backend ainda não fornece Docker Compose para provisionar esse ambiente.

| Serviço | Configuração local esperada | Preparação |
| --- | --- | --- |
| PostgreSQL | `localhost:5432`, banco `verum`, usuário/senha `postgres` | Disponibilizar o banco e acesso para o usuário da aplicação |
| RabbitMQ | `localhost:5672`, usuário `admin`, senha `admin123`, virtual host `/` | Criar o usuário e conceder permissões no virtual host; MassTransit declara sua topologia |
| Redis 6.2+ | `localhost:6379`, sem senha ou TLS localmente | Disponibilizar o serviço; não exige criação prévia de chaves |
| Keycloak | `http://localhost:8080/realms/verum`, audience `verum-api` | Configurar realm e client para emitir tokens com o audience esperado |
| Collector OTLP, opcional | Endpoint escolhido para telemetria | Necessário somente quando a exportação estiver habilitada |

Em containers, publique essas portas para executar o .NET na máquina local. Se a aplicação também estiver em container, substitua `localhost` pelos nomes dos serviços na rede Docker. Use volumes para os dados persistentes de PostgreSQL e RabbitMQ. A interface administrativa do RabbitMQ é opcional e não substitui a porta AMQP 5672.

Os três hosts possuem `appsettings.json` e `appsettings.Development.json`. Development já aponta para as conexões locais acima e habilita RabbitMQ e Redis. As credenciais são exclusivas de desenvolvimento. Os valores do Keycloak são referências de configuração: o projeto não provisiona realm ou client.

| Seção | O que configurar |
| --- | --- |
| `ConnectionStrings` | Conexões `PostgreSQL` e `Redis` |
| `RabbitMQ` | Habilitação, host, porta, credenciais, TLS, concorrência, retries e publicação |
| `Redis` | Habilitação, prefixo do ambiente, timeout e TTLs |
| `Authentication` | Authority, audience e exigência de metadados HTTPS da API |
| `Cors:Origins` | Origens permitidas; Development inclui `http://localhost:4200` |
| `Http` | Políticas padrão e sobrescritas por cliente externo |
| `Observability` | Habilitação da exportação OTLP |

Variáveis de ambiente sobrescrevem os arquivos e usam `__` entre níveis, por exemplo `ConnectionStrings__PostgreSQL` e `RabbitMQ__Password`. Configure segredos pelo mecanismo de secrets da implantação. Fora de Development, configure explicitamente conexões e autoridade de autenticação; metadados HTTPS são exigidos por padrão.

`RabbitMQ:Enabled=false` permite iniciar sem broker, mas não registra bus nem publicador. `Redis:Enabled=false` mantém os contratos com o comportamento de indisponibilidade descrito acima. A autenticação de endpoints protegidos depende de um emissor JWT configurado corretamente.

Redelivery adiado permanece desabilitado. Para habilitar `RabbitMQ:UseDelayedRedelivery`, o broker precisa do plugin `rabbitmq_delayed_message_exchange`. A exportação de telemetria exige `Observability:OtlpEnabled=true` e as variáveis OTEL, como `OTEL_EXPORTER_OTLP_ENDPOINT`.

### Banco de dados

Iniciar os hosts não cria banco, schemas ou tabelas. Os schemas já estão definidos nos DbContexts e serão criados pelas migrations do EF Core quando elas forem implementadas e aplicadas. Não é necessário desenhar os schemas manualmente; nesta etapa, disponibilize o banco `verum`. Ainda não há migrations para executar, e alterações nos mappings não atualizam um banco existente automaticamente.

## Como executar e validar

Com as dependências locais preparadas, execute em `src/backend`:

```powershell
dotnet build Verum.Backend.slnx
dotnet test Verum.Backend.slnx
dotnet run --project Apps/Verum.Api --launch-profile http
```

Inicie cada worker em um terminal separado:

```powershell
dotnet run --project Apps/Verum.Worker.Descoberta --launch-profile Verum.Worker.Descoberta
dotnet run --project Apps/Verum.Worker.Radar --launch-profile Verum.Worker.Radar
```

Esses perfis selecionam Development. A API HTTP fica em `http://localhost:5099`; o perfil alternativo `https` usa `https://localhost:7269` e requer certificado local confiável. O documento OpenAPI fica em `/openapi/v1.json` somente em Development.

`/health/live` verifica o processo. `/health/ready` agrega os checks registrados, incluindo MassTransit quando habilitado; ainda não verifica PostgreSQL, Redis ou Keycloak e não comprova a disponibilidade de todo o ecossistema.

Os testes cobrem composição, modelos EF, cache, mensageria, concorrência, proteção HTTP e erros da API. A suíte usa transporte em memória e HTTP simulado para os testes que dispensam serviços externos. Para incluir testes reais, configure `VERUM_TEST_REDIS` com a conexão Redis e `VERUM_TEST_POSTGRES` com uma conexão PostgreSQL de desenvolvimento antes de executar `dotnet test`.

Sem essas variáveis, os respectivos testes externos são ignorados; quando configuradas, indisponibilidade causa falha. Os testes Redis usam chaves próprias. Os testes PostgreSQL criam schemas aleatórios em transações revertidas e precisam de permissão para criar schemas. Esses testes não aplicam migrations da aplicação nem validam integrações reais com RabbitMQ ou Keycloak.
