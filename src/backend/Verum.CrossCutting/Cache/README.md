# Cache Redis do Verum

Implementação das decisões do chat de arquitetura: Redis contém dados descartáveis,
limites operacionais e reservas temporárias; PostgreSQL continua sendo a fonte de
preços, snapshots, permissões e consumo oficial. RabbitMQ continua responsável pelas
mensagens. Nenhum endpoint, fluxo de cobrança ou processamento de negócio foi criado.

## Organização

| Componente | Uso |
| --- | --- |
| `CacheRedis` | Operações genéricas de cache de DTOs JSON |
| `CoordenacaoRedis` | Inclusão condicional, reserva com token e limite atômico |
| `RedisConexao` | Uma conexão assíncrona multiplexada por processo |
| `Modulos/*Cache` | Implementações dos contratos de cada módulo |
| `CachePipeline` | DI, opções e integração com `IDistributedCache` |

Os contratos estão em `Modules/Verum.Modules.*/Contratos/Cache`. Os módulos dependem
desses contratos, sem referenciar CrossCutting ou conhecer Redis. API e workers já
registram tudo pelo pipeline existente. A ligação dos contratos aos futuros casos
de uso ocorrerá na implementação desses fluxos.

## Operações genéricas

| Método | Comportamento |
| --- | --- |
| `GetAsync<T>` | Obtém um DTO; retorna `null` em miss ou indisponibilidade |
| `SetAsync<T>` | Cria ou substitui um DTO, com TTL obrigatório entre 1 ms e 30 dias |
| `SetAteAsync<T>` | Cria ou substitui até um instante absoluto de validade |
| `RemoveAsync` | Remove uma chave específica |
| `ExistsAsync` | Consulta existência; não serve como verificação de exclusão mútua |
| `GetOrCreateAsync<T>` | Consulta cache, carrega a origem em miss e preenche o cache |
| `TryAddAsync` em `CoordenacaoRedis` | Grava uma string somente se ausente, com TTL atômico |

`SetAsync` é a operação de gravação equivalente ao “post” solicitado: Redis utiliza
`SET`, que também substitui valores existentes. Os genéricos recebem tipos de
referência: use DTOs/records, listas ou strings; não serialize entidades EF e suas
navegações. `null` não é armazenado. Um DTO com lista vazia pode representar um
resultado concluído sem ofertas.

Exemplo de uso na camada de composição/host:

```csharp
var resultado = await cache.GetOrCreateAsync<MeuDto>(
  "meu-modulo:projecao:v1:123",
  ct => carregarDaOrigemAsync(ct),
  TimeSpan.FromMinutes(1),
  cancellationToken);
```

Nos módulos, prefira o contrato específico abaixo. Use um único tipo de DTO por
chave e versione o sufixo ao mudar sua representação. Dados incompatíveis com o
JSON esperado geram log e miss; erros de serialização na escrita propagam.

`GetOrCreateAsync` contém concorrência **dentro do processo**, com 256 semáforos
reutilizáveis e nova leitura após adquirir o semáforo. Não mantém um dicionário
ilimitado de chaves. Processos distintos podem executar a mesma carga; para consultas
externas caras compartilhadas por Busca e Radar, use a reserva de atualização de
Ofertas. Falhas e cancelamento da função de origem propagam. Em indisponibilidade do
cache, a origem continua sendo consultada, sem substituir Redis por memória local.

## Métodos do produto

| Contrato | Operações e chave lógica | TTL padrão máximo |
| --- | --- | --- |
| `IBuscaCache` | Obter/armazenar status e resultado por `BuscaId`; invalidar ambos | status: 5 s; resultado: 15 min |
| `ICatalogoCache` | Obter/armazenar/invalidar variante e resolução de consulta | variante: 6 h; resolução: 1 h |
| `IOfertasCache` | Obter/armazenar/invalidar ofertas por variante | 2 min |
| `IOfertasCache` | Reservar/renovar/liberar atualização por variante + fonte | duração obrigatória do chamador |
| `IAcessoCache` | Obter/armazenar/invalidar permissões por conta | 30 s |
| `IAcessoCache` | Consumir limite operacional por conta ou visitante + recurso + fim da janela | até o fim da janela |

Os valores são iniciais e configuráveis, não parâmetros comerciais definitivos.
Status é uma projeção temporária para polling. Autorize o acesso ao `BuscaId` **antes**
de consultar seu cache; conhecer um ID não concede acesso ao resultado.

Consultas do Catálogo são identificadas pelo hash SHA-256 da consulta normalizada
e da versão do interpretador. A normalização pertence ao módulo: o cache não altera
caixa de URLs ou retira informações que possam distinguir duas consultas.

Para ofertas, resultados e permissões, informe `validasAte`/`validoAte`. O cache usa
o menor prazo entre essa validade e o TTL configurado. Em uma lista de ofertas,
informe a menor validade entre seus itens. Para permissões, considere a próxima
expiração de uma concessão. Uma validade já vencida remove a entrada anterior.
Não há renovação do prazo ao ler. Jitter reduz a validade em até 10%, evitando
expirações simultâneas sem prolongar dados vencidos. A escrita usa `SET PXAT`.

```csharp
// Dentro do módulo Ofertas, após consultar sua própria persistência:
await ofertasCache.ArmazenarAtuaisAsync(varianteId, dto, menorValidadeDasOfertas, ct);

// Após persistir uma mudança de permissão no módulo Acesso:
await acessoCache.InvalidarPermissoesAsync(contaId, ct);
```

A invalidação deve ocorrer depois do commit e ser propagada pelos eventos do módulo
quando houver réplicas. Uma invalidação que falhe retorna `false` e o TTL limita a
defasagem. Cache-aside não garante consistência forte entre uma leitura concorrente
e uma alteração; operações que exigem autorização ou preço rigorosamente atuais
devem confirmar na fonte proprietária.

## Reservas compartilhadas entre Busca e Radar

`TentarReservarAtualizacaoAsync` retorna token quando adquiriu a reserva, `null`
quando já há outra reserva e lança exceção se Redis está indisponível. Não trate
essas três situações como equivalentes.

```csharp
var token = await ofertasCache.TentarReservarAtualizacaoAsync(
  varianteId, fonteId, TimeSpan.FromMinutes(1), ct);

if (token is null)
{
  // Reutilizar dados ainda válidos ou reagendar a consulta pelo fluxo de negócio.
  return;
}

try
{
  // Consultar a fonte, validar e persistir. Usar um timeout inferior à reserva.
}
finally
{
  // Usar um token de cancelamento próprio para a liberação, mesmo se ct foi cancelado.
  await ofertasCache.LiberarReservaAsync(varianteId, fonteId, token, CancellationToken.None);
}
```

A reserva usa `SET NX PX`; renovação e liberação comparam o token no Redis por Lua
antes de alterar a chave. Um dono antigo não remove uma reserva adquirida depois
dele. Para trabalhos longos, renove antes do vencimento e interrompa/reagende se a
renovação falhar. Trate também falhas de liberação sem ocultar a falha original do
processamento; a reserva expira automaticamente.

Essa reserva reduz trabalho externo duplicado. Expiração, pausas e failover podem
permitir sobreposição; ela não substitui idempotência, restrições/transações do
PostgreSQL nem Outbox/Inbox para garantir efeitos de negócio.

## Limites operacionais

```csharp
var decisao = await acessoCache.TentarConsumirLimiteVisitanteAsync(
  visitanteId, "BUSCA_EXECUTADA", limiteConfigurado, fimDaJanelaUtc, ct);

// decisao.Permitido, Utilizado, Restante e TentarNovamenteEm.
```

O módulo Acesso define o limite e calcula uma janela de calendário estável. Todas
as chamadas dessa janela devem enviar o **mesmo fim**, não `UtcNow.AddDays(1)` a cada
requisição. Conta e visitante possuem chaves distintas, mesmo se os GUIDs coincidirem.
O ID do visitante deve vir de cookie assinado/validado, conforme a arquitetura.

Um script Lua verifica o limite, incrementa apenas tentativas permitidas e define
a expiração atomicamente. Tentativas negadas não incrementam nem prolongam o prazo.
Não há retry automático de mutações: após timeout/cancelamento do cliente, o comando
pode já ter sido executado no servidor. Uma nova tentativa pode consumir outra unidade.

Isso limita admissões, não registra faturamento nem consumo concluído. O consumo
definitivo permanece em `acesso.registro_uso`, com sua idempotência. Perda/evicção de
chaves Redis pode reiniciar um contador; cotas comerciais estritas precisam ser
validadas/reconciliadas com PostgreSQL pelo módulo Acesso. A infraestrutura não
implementa essa regra de negócio nem libera acesso automaticamente em falhas.

## Configuração e operação

Redis 6.2+; validado localmente com o container Redis 7. Os três hosts possuem a
mesma seção `Redis` em `appsettings.json`. Os arquivos Development já habilitam
a conexão local. Em produção, configure:

```text
Redis__Enabled=true
Redis__Prefixo=verum:producao:
ConnectionStrings__Redis=host:6379,...
```

Use um prefixo por ambiente e o **mesmo prefixo entre API e workers**. Os namespaces
`v1:cache`, `v1:coord` e `v1:distributed` impedem colisão entre DTOs, coordenação e o
formato interno de `IDistributedCache`. Não há comandos globais de limpeza/varredura
na implementação do produto.

Conexão assíncrona singleton, reconexão do cliente, timeout padrão de 1 s e backlog
fail-fast evitam recriar conexões e acumular operações enquanto Redis está fora.
O cliente `IDistributedCache` compartilha a conexão. O `StackExchange.Redis` foi
explicitado na mesma versão já resolvida pelo provider (2.7.27), sem atualizar
outras dependências.

Com `Enabled=false`, os contratos permanecem disponíveis: leitura retorna miss,
escrita/invalidação retorna `false`, e coordenação lança exceção. Com Redis habilitado
mas indisponível, o comportamento é equivalente para cache; coordenação propaga o
erro Redis. Não use `ExistsAsync` nem `GetAsync` para conceder uma reserva/cota.

## Testes

```powershell
dotnet build src/backend/Verum.Backend.slnx
$env:VERUM_TEST_REDIS = 'localhost:6379'
dotnet test src/backend/Verum.Backend.slnx --no-build --no-restore
```

Sem essa variável, somente os testes externos são marcados como ignorados. Com ela,
Redis indisponível faz o teste falhar. Testes usam duas instâncias, prefixos aleatórios
`verum:tests:<guid>:` e removem apenas suas próprias chaves. Cobrem expiração, DTOs,
cache-aside concorrente, token obsoleto, limite atômico, isolamento e indisponibilidade.

## Referências

- [Conexão multiplexada — StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/Basics.html)
- [SET, NX e PXAT — Redis](https://redis.io/docs/latest/commands/set/)
- [Reservas e limitações de locks — Redis](https://redis.io/docs/latest/develop/clients/patterns/distributed-locks/)
- [Contadores e atomicidade com Lua — Redis](https://redis.io/docs/latest/commands/incr/)
