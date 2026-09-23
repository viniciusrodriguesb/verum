# Backend Verum

Base técnica do monólito modular em .NET 10, com oito módulos, 32 entidades,
mappings EF Core, DbContexts próprios, cache Redis, hosts e pipelines.
Casos de uso, scrapers, integrações de pagamento/notificação, migrations e
endpoints de negócio ainda não foram implementados.

## Estrutura e composição

- `Apps/Verum.Api`: ASP.NET Core; chama `AddVerumApi` e `UseVerumApi`.
- `Apps/Verum.Worker.Descoberta`: Generic Host; chama `AddVerumWorkerDescoberta`.
- `Apps/Verum.Worker.Radar`: Generic Host; chama `AddVerumWorkerRadar`.
- `Modules`: Contas, Acesso, Busca, Catálogo, Ofertas, Radar, Assinaturas e
  Notificações, cada um com domínio, mappings, DbContext e registro de persistência.
- `Verum.CrossCutting/Pipelines`: um arquivo por pipeline.
- `Verum.CrossCutting/Mensageria`: opções e componentes genéricos.
- `Tests/Verum.Tests.Integracao`: testes da configuração com infraestrutura em
  memória; demais categorias continuam reservadas.

CrossCutting compõe os módulos; módulos nunca referenciam CrossCutting. A API
registra os oito módulos. Descoberta registra Busca, Catálogo e Ofertas; o worker
Radar registra Radar, Catálogo, Ofertas e Notificações.
Consumidores são selecionados explicitamente por host, evitando processar a mesma
mensagem em executáveis diferentes por uma varredura indiscriminada.

## Pacotes

Versões fixadas em `Directory.Packages.props`; cada projeto declara seus pacotes.

| Base | Versão |
| --- | --- |
| ASP.NET OpenAPI, JWT, Hosting, Redis e EF Core | 10.0.12 |
| Microsoft.Extensions.Http.Resilience | 10.10.0 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 |
| MassTransit.RabbitMQ e EntityFrameworkCore | 8.5.10 |
| OpenTelemetry Hosting, ASP.NET Core e OTLP | 1.19.0 |
| OpenTelemetry HTTP | 1.18.0 |

MassTransit 8.5.10 fornece assets para `net10.0` e licença Apache-2.0, conforme
seus manifests NuGet. A versão foi fixada explicitamente; a linha 9 tem
licenciamento comercial. O transporte usa RabbitMQ.Client transitivamente;
não há conexão ou canal RabbitMQ manual paralelo ao MassTransit.

EF/Npgsql estão no CrossCutting porque o registro genérico de persistência os
utiliza. Quando um módulo implementar um DbContext ou consumidor, deverá declarar
também seus próprios PackageReferences para as APIs que usa. SDKs de provedores
(OpenAI, pagamento, e-mail, Playwright) serão adicionados com seus adaptadores;
nenhum adaptador de negócio foi criado nesta etapa.

## Pipelines

| Arquivo | Responsabilidade |
| --- | --- |
| ApplicationPipeline | Middleware HTTP, autenticação, autorização, CORS, Problem Details, OpenAPI e health endpoints |
| WebPipeline | Registro de controllers, OpenAPI, Problem Details e CORS por origens explícitas |
| AutenticacaoPipeline | JWT do realm Keycloak, validação de issuer, audience e validade |
| ServicesPipeline | DI por convenção de nome, com lifetime scoped |
| ModulosPipeline | Assemblies de aplicação selecionados para cada host |
| HttpClientsPipeline | IHttpClientFactory e resiliência padrão para clientes nomeados/tipados |
| MensageriaPipeline | Bus RabbitMQ, credenciais, TLS opcional, prefixos de filas e ciclo de vida |
| PublishersPipeline | PublicadorMensagem<T> scoped sobre IPublishEndpoint |
| ConsumersPipeline | Concorrência, retry, redelivery opcional e buffer de publicações |
| PersistenciaPipeline | Registro PostgreSQL por DbContext/schema e preparação de Outbox transacional |
| CachePipeline | Cache genérico, contratos de cache por módulo, coordenação atômica e IDistributedCache |
| ObservabilidadePipeline | Logs, tracing, métricas, exportação OTLP opcional e health checks |

## DI automática de services

Classes concretas fechadas com sufixo `Service` em um segmento de namespace
`Aplicacao` dos assemblies selecionados são registradas automaticamente:

```csharp
namespace Verum.Modules.Busca.Aplicacao.IniciarBusca;

internal sealed class IniciarBuscaService
{
  // Futuro caso de uso.
}
```

Não é necessário adicionar uma linha de DI para cada nova classe. Classes
internas são suportadas, desde que tenham construtores públicos para o container.
Quando existe `IIniciarBuscaService`, contrato e classe resolvem a mesma instância
dentro do escopo. A interface é opcional. Classes abstratas, genéricos abertos e
classes fora de Aplicacao não são registradas. Contratos ambíguos geram erro.
Lifetime padrão é scoped; não existe promoção automática para singleton.
Novos módulos devem entrar na seleção de assemblies uma única vez.

## HTTP

Todos os clientes criados por `IHttpClientFactory` recebem a resiliência padrão:
limite de concorrência, timeout total, retry exponencial com jitter, circuit
breaker e timeout por tentativa. Cada cliente nomeado/tipado isola seu pipeline;
crie um cliente por provedor. Configuração comum em `Http:Resilience`.

```csharp
services.AddHttpClient<ClienteDaFonte>(client =>
  client.BaseAddress = new Uri("https://fonte.example"));
```

POST, PATCH, PUT, DELETE e CONNECT não recebem retry automático, evitando
duplicação de efeitos. Não empilhe outro handler padrão sobre o já configurado.
Timeout e cancelamento propagam ao chamador. O fallback entre fontes pertence
ao futuro caso de uso: esse pipeline não converte falhas em resultados vazios.

## RabbitMQ, publishers e consumers

`RabbitMQ:Enabled=false` permite executar a base sem broker. Nesse modo não há
bus nem publicador registrado: não existe transporte falso ou descarte silencioso.
Para habilitar, configure em cada executável, por variáveis de ambiente ou secrets:

```text
RabbitMQ__Enabled=true
RabbitMQ__Host=seu-host
RabbitMQ__Port=5672
RabbitMQ__VirtualHost=/
RabbitMQ__Username=usuario
RabbitMQ__Password=senha
```

Para TLS, use `RabbitMQ__UseSsl=true` e a porta TLS do ambiente (normalmente 5671).
Certificados são validados normalmente. Não há credenciais reais versionadas.

O host aguarda a conexão por até 30 segundos. Os endpoints usam prefixos
`verum-api`, `verum-descoberta` ou `verum-radar`, com nomes kebab-case,
prefetch e concorrência configuráveis. MassTransit gerencia publisher confirms,
acknowledgments, conexões e recuperação do transporte.

Os workers já registram seus consumidores, cada um em seu próprio host:

| Worker | Contrato | Fila |
| --- | --- | --- |
| Descoberta | `BuscaSolicitada(BuscaId)` | `verum-descoberta-busca-solicitada` |
| Radar | `VerificacaoRadarSolicitada(MonitoramentoId)` | `verum-radar-verificacao-radar-solicitada` |

Os contratos pertencem aos módulos Busca e Radar e carregam somente o ID do
registro persistido. Nenhum agendamento ou produtor de negócio foi criado.
Os registros ficam em `Configuracoes/ConsumidoresConfiguration.cs` de cada
worker. A API não registra esses consumidores.

Em cada consumidor, a região `#region Processamento...` indica onde injetar e
chamar a fachada/serviço real. Até essa implementação, IDs vazios geram
ArgumentException e mensagens válidas geram ProcessamentoNaoConfiguradoException.
Ambas seguem para `_error` e emitem `Fault<T>` sem retries/redelivery.
Isso evita confirmar como concluído um trabalho ainda não implementado.
Após implementar o serviço, remova o throw e aguarde sua Task com o token do
contexto. As mensagens na fila de erro poderão ser reenviadas para processamento.

Para adicionar outro consumidor futuro no host correspondente:

```csharp
builder.Services.AddVerumWorkerDescoberta(builder.Configuration,
  bus => bus.AddConsumer<MinhaMensagemConsumer>());
```

O consumidor pode implementar `IConsumer<T>` diretamente ou, nos hosts, herdar
`ConsumidorMensagem<T>`. A base adiciona escopo de logs com IDs e delega a
`ProcessarAsync(ConsumeContext<T>)`, preservando cancellation e falhas.
Módulos usam as interfaces nativas do MassTransit sem depender de CrossCutting.

O `PublicadorMensagem<T>.PublicarAsync` aceita CancellationToken e usa o
`IPublishEndpoint` scoped, preservando contexto e Outbox. Possui deadline
configurável para publicação e não adiciona um segundo loop de retry que
possa duplicar publicações. Se falhar ou expirar, a falha é devolvida ao chamador.
O publicador não substitui a persistência durável de eventos.

Há até três retries curtos por padrão. ArgumentException,
ProcessamentoNaoConfiguradoException e OperationCanceledException não são repetidas.
Esgotadas as tentativas, o
MassTransit mantém seu fluxo padrão de fila `_error` e `Fault<T>`; mensagens
sem consumidor seguem para `_skipped`. Consumidores precisam ser idempotentes:
entrega e confirmações não eliminam a possibilidade de duplicação.

`UseDelayedRedelivery=true` habilita intervalos de 30, 120 e 600 segundos,
configuráveis. Essa opção exige o plugin `rabbitmq_delayed_message_exchange`
instalado no broker. Ela permanece desabilitada até essa preparação.
Retry curto mantém a mensagem em processamento; redelivery libera o consumidor
durante esperas longas. Não há scheduler de negócio nesta etapa.

### Paralelismo de consumo

Não há SemaphoreSlim adicional. `RabbitMQ:ConcurrentMessageLimit=8` limita o
processamento simultâneo por endpoint e instância; `PrefetchCount=16` limita as
mensagens antecipadas pelo broker. Cada worker possui sua própria configuração.
O MassTransit cria escopos de DI para as mensagens e aguarda suas Tasks.
Não use Task.Run ou fire-and-forget para disparar o processamento.

Com duas réplicas do mesmo worker, o limite agregado pode chegar a 16 mensagens
simultâneas na mesma fila. Esse limite não garante ordenação nem exclusão por
BuscaId/MonitoramentoId; idempotência e proteção de alterações concorrentes serão
implementadas com o processamento. Um semáforo local tampouco coordenaria réplicas.
Limites por provedor externo e paralelismo dentro de uma busca são preocupações
separadas. Os valores iniciais são configuráveis, não resultado de benchmark.

## Outbox e PostgreSQL

O buffer em memória dos consumidores impede publicar os eventos de uma tentativa
que terminou com erro. Ele **não é durável**, não garante deduplicação persistente
nem atomicidade com mudanças no banco.

A base fornece `AddVerumPostgreSql<TContext>(configuration, schema)`,
`AddVerumOutbox<TContext>()` e `UseVerumOutbox<TContext>(context)`.
Quando existir o contexto e suas migrations:

1. Registre o DbContext com a conexão `ConnectionStrings:PostgreSQL` e schema
   proprietário. O schema informado separa a tabela de histórico de migrations;
   o próprio contexto deve definir `HasDefaultSchema` para suas entidades.
2. No modelo do módulo, usando as extensões nativas de MassTransit, adicione
   `AddInboxStateEntity`, `AddOutboxMessageEntity` e `AddOutboxStateEntity`.
3. Registre `bus.AddVerumOutbox<TContext>()` no callback de composição.
4. No ConsumerDefinition correspondente, aplique
   `endpoint.UseEntityFrameworkOutbox<TContext>(context)` (ou a extensão
   `UseVerumOutbox` se a definição estiver no host).
5. Desabilite `RabbitMQ:UseInMemoryOutbox` no host que adotar o Outbox
   transacional, para não empilhar os dois mecanismos.
6. Publique pelo endpoint scoped e persista com `SaveChangesAsync` na mesma
   unidade de trabalho das alterações. Crie/aplique migrations antes de habilitar.

O helper inicial de Bus Outbox deve ser usado com **um contexto proprietário
por bus/host**. A composição de vários Bus Outboxes na mesma instância exige
seleção explícita do contexto e será definida com os fluxos modulares; não se
deve registrar vários e supor que IPublishEndpoint escolherá sozinho.
Os oito DbContexts e seus mappings já existem. Migrations e tabelas técnicas
de Outbox/Inbox ainda não foram criadas; o Outbox durável não está ativo.
Retry de transação do EF não foi ligado globalmente para não competir com
as transações gerenciadas pelo consumidor.

## Configuração e execução

Os três hosts possuem `appsettings.Development.json` com as conexões do Docker
local: RabbitMQ em localhost:5672, Redis em localhost:6379 e PostgreSQL em
localhost:5432. O banco escolhido para a aplicação é `verum`. RabbitMQ e Redis
estão habilitados nesse ambiente. Os perfis locais dos workers selecionam
Development, assim como os perfis da API. São endereços para executar o .NET
na máquina host; em containers, localhost apontaria para o próprio container.

Os schemas já estão definidos nos DbContexts com `HasDefaultSchema` e serão
criados ao aplicar as migrations do EF Core. Registrar a conexão ou iniciar o
host não cria banco, schemas ou tabelas nesta etapa. Não há migrations ainda.

O cache Redis possui operações genéricas, métodos por módulo, TTLs configuráveis,
reservas de atualização e limites operacionais atômicos. Veja os
[métodos, exemplos e garantias](Verum.CrossCutting/Cache/README.md).
Com `Redis__Enabled=false`, leitura vira miss e gravação retorna false; reservas
e limites não são concedidos. Para conectar, habilite e preencha
`ConnectionStrings__Redis`. Não há substituição por cache em memória.

A API exige `Authentication__Authority` (URL do realm Keycloak) e
`Authentication__Audience`. Em Development há apenas valores locais de exemplo.
Em outros ambientes o issuer deve ser configurado, com metadados HTTPS por
padrão. O realm e o client precisam emitir o audience esperado. As políticas
de autorização e o mapeamento de papéis de negócio serão definidos depois.

`Cors__Origins__0` define a primeira origem permitida. A configuração de
Development permite `http://localhost:4200`. OpenAPI é exposto apenas em
Development. `/health/live` verifica o processo; `/health/ready` agrega os checks
registrados, incluindo MassTransit quando habilitado. Não verifica Redis,
PostgreSQL ou Keycloak nesta etapa e não atesta disponibilidade do ecossistema.

Para exportar telemetria, habilite `Observability__OtlpEnabled=true` e configure
as variáveis OTEL usuais, como `OTEL_EXPORTER_OTLP_ENDPOINT`. Sem habilitação,
não há exportação para um collector externo.

Execute em `src/backend`, com o SDK do `global.json`:

```powershell
dotnet build Verum.Backend.slnx
dotnet test Verum.Backend.slnx
dotnet run --project Apps/Verum.Api --launch-profile http
```

Os testes cobrem DI por escopo, validação de opções, inicialização dos dois hosts,
middleware da API, retries HTTP seguros, retry de consumo, Outbox em memória e
Fault de mensagem inválida, os consumidores reais dos workers e paralelismo
respeitando o limite do endpoint. Transporte em memória e HTTP simulado permitem
validar configuração sem serviços externos. Não substituem testes com RabbitMQ,
PostgreSQL, Redis ou Keycloak reais.

## Padrão de código e revisão da arquitetura

- Tabelas e colunas de negócio usam `UPPER_SNAKE_CASE`: `BUSCA_ETAPA`, `INICIADA_EM`.
  Schemas continuam em minúsculas por módulo e a tabela técnica de histórico do
  EF mantém seu nome padrão. Propriedades C# continuam em PascalCase.
  No SQL manual, use aspas duplas: `SELECT "ID", "INICIADA_EM" FROM busca."BUSCA_ETAPA";`.
  O EF/Npgsql inclui as aspas no SQL gerado. Expressões SQL de checks e filtros
  também usam identificadores explicitamente delimitados. Esse comportamento é
  [nativo do PostgreSQL](https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-IDENTIFIERS).
  Sem aspas, o PostgreSQL converte identificadores para minúsculas.
- Entidades ficam diretamente em `Dominio`, com seus métodos privados de validação
  no final do mesmo arquivo. `Validacao.cs` mantém os auxiliares locais do módulo.
- Enums ficam em `Dominio/Enums`; o namespace de domínio é preservado.
- Testes são agrupados em `Configuracao`, `Persistencia`, `Cache`, `Mensageria`,
  `Http` e `Fixtures`, no projeto de testes existente.
- Uma linha em branco entre declarações, propriedades, instruções e métodos.
- Duas linhas em branco antes das navegações, no próprio arquivo da entidade.
- Cada cadeia de mapping mantém uma chamada por linha, com os pontos alinhados;
  uma linha em branco separa a configuração de uma propriedade da seguinte.
- Um DbContext interno por módulo, schema e histórico de migrations próprios.
- Mappings separados com `IEntityTypeConfiguration<T>`, descobertos por
  `ApplyConfigurationsFromAssembly`; nenhuma configuração individual no contexto.
- Entidades com setters privados, coleções somente leitura e validação de criação.
- Nenhum relacionamento EF cruza schemas; integrações usam contratos/eventos.
- Redis é descartável; consumo e efeitos definitivos pertencem ao PostgreSQL.

Os testes verificam os oito modelos, 32 mappings, chaves, tipos de coluna,
relacionamentos, encapsulamento e separação do histórico de migrations.
Com `VERUM_TEST_POSTGRES` configurada para uma conexão de desenvolvimento, validam
também a criação das 32 tabelas e suas constraints/índices no PostgreSQL real,
em schemas aleatórios dentro de transações revertidas ao final. O usuário da
conexão precisa poder criar schemas; os schemas da aplicação não são alterados.
Nenhuma migration foi criada nesta etapa. Se existir um banco criado manualmente
com nomes antigos, a alteração dos mappings não o renomeia automaticamente.
A revisão cobre a base implementada; o ADR ainda prevê migrations, Outbox/Inbox
duráveis, idempotência e os casos de uso que serão desenvolvidos nas próximas etapas.

### Pastas reservadas para o desenvolvimento

Pastas vazias com responsabilidade prevista permanecem na estrutura:

| Local | Uso previsto |
| --- | --- |
| API: Controllers, Configuracoes, Filtros e Middlewares | Endpoints, configuração e comportamento HTTP |
| Workers: Processadores | Orquestração do processamento, delegando regras aos módulos |
| Worker Radar: Agendamentos | Disparo das verificações periódicas |
| Módulos: Aplicacao | Casos de uso, organizados por funcionalidade quando implementados |
| CrossCutting: Autenticacao e Web | Componentes de autenticação e HTTP usados pelos pipelines |
| CrossCutting: Observabilidade e Resiliencia | Instrumentação e políticas técnicas de integração |
| CrossCutting: Persistencia | Componentes técnicos de persistência e Outbox, sem centralizar DbContexts |
| Tests: Unitarios, Arquitetura e Funcionais | Separação futura das suítes quando necessária; ainda sem novos projetos |

`CrossCutting/Modulos` foi removida por redundância: a composição permanece em
`Pipelines/ModulosPipeline.cs`, e os módulos de negócio em `Modules`.
Os três `ModuleMarker.cs` sem referências foram removidos; a seleção de assemblies
continua utilizando `DependencyInjection` de cada módulo.

## Referências

- [Resiliência HTTP — Microsoft](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Retry, redelivery e falhas — MassTransit](https://masstransit.io/documentation/concepts/exceptions)
- [RabbitMQ — configuração MassTransit](https://masstransit.io/documentation/configuration/transports/rabbitmq)
- [Outbox — MassTransit](https://masstransit.io/documentation/configuration/middleware/outbox)
- [Confiabilidade e duplicações — RabbitMQ](https://www.rabbitmq.com/docs/reliability)
- [Provider EF Core — Npgsql](https://www.npgsql.org/efcore/)

A documentação atual do MassTransit também descreve recursos da versão 9.
O código desta base foi compilado e testado contra a versão 8.5.10 fixada.
