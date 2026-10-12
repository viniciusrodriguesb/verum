# Validação de buscas específicas — 12/10/2026

A causa da busca original parada foi a ausência do worker Descoberta: RabbitMQ tinha uma mensagem pronta e zero consumidores. A solicitação havia excedido o prazo de três minutos antes da inicialização do worker e foi finalizada como Expirada. API, Angular e worker ficaram ativos ao final da validação.

## Correções

- Amazon: reconhecimento explícito da mensagem de ausência de resultados, tanto em título quanto em span. Layout desconhecido continua sendo erro técnico, e não lista vazia.
- Correspondência: códigos alfanuméricos são preservados. NV3 não corresponde a NV1 combinado com PCIe 3.0. Capacidades como 1TB e 1 TB continuam equivalentes.
- Ranking identificado como relevancia-conservadora-v3 para registrar a mudança de comparação.
- Testes de regressão para modelos diferentes, capacidades, títulos com acentos e as duas variantes do marcador vazio.

## Fluxo real pela interface

Foram usados Chromium, Angular, API, RabbitMQ, worker, Redis, PostgreSQL e Keycloak reais. Cada caso recebeu um contexto de navegador novo; o estado deslogado foi alcançado fazendo login e depois logout. Contas temporárias foram removidas. Nenhuma conta ou cookie do usuário foi utilizado ou alterado.

| Consulta | Visitante | Logado | Após logout | Ofertas nos três estados |
| --- | ---: | ---: | ---: | ---: |
| O homem mais rico da Babilônia | 13,0 s | 5,3 s | 5,3 s | 12 |
| 1984 George Orwell | 7,9 s | 4,6 s | 5,3 s | 12 |
| Cabo SCSI HD68 | 7,2 s | 7,2 s | 7,9 s | 2 |
| SSD Kingston NV3 1TB | 36,5 s | 4,6 s | 5,3 s | 9 |
| Produto inexistente | 6,7 s | 5,3 s | 4,7 s | 0 |

Os quatro primeiros casos vêm da matriz de 15 buscas. A consulta inexistente foi repetida nos três estados após a correção definitiva; antes, levava aproximadamente 32,6 segundos e registrava timeout da Amazon. Após a correção, ambas as lojas concluíram sem falha. O resultado final foi ConcluidaSemResultado, distinguindo ausência de itens de falha técnica.

Consultas seguintes puderam reutilizar o catálogo e ofertas válidas. Nessas consultas, zero fontes externas consultadas é esperado. Os tempos incluem o fluxo pela interface e não representam exclusivamente a duração das lojas. A consulta direta inexistente passou de aproximadamente 30,7 segundos para 1 segundo. Lojas e latência variam no tempo.

Nos livros, os primeiros resultados apresentam o título solicitado. Para o cabo raro, os títulos identificam SCSI e HD68. O SSD NV1 2TB indevidamente aceito pela comparação anterior foi excluído. Não foram fabricadas ofertas para completar 12; edições, kits, capacidade e formato ainda precisam ser avaliados ao comparar produtos distintos.

Em cada caso também foram validados POST aceito, repetição idempotente, acompanhamento até estado final, limite de 12, URLs sem duplicação, rejeição de outro navegador com 404 e cota diária com 429. Na repetição da consulta inexistente, o erro de cota foi testado pelo formulário: aviso visível e permanência na tela de resultados, sem abrir o loading.

## Limites da entrega

Login e logout são reais, mas a busca ainda identifica o proprietário por cookie de visitante. Concessões e cotas por conta não foram implementadas por esta revisão. A IA permaneceu desabilitada e sem chave; seus caminhos de falha e timeout são cobertos pela regressão simulada, não por uma chamada real à OpenAI.

## Evidências

- 171 testes backend aprovados, com um teste JWT optativo ignorado na execução geral.
- 22 testes frontend aprovados e build de produção aprovado.
- Teste real adicional de livros, item raro, SSD e consulta inexistente aprovado.
- Teste real anterior de televisão 4K, iPhone 17 e Samsung S25 aprovado.
- Teste real de produtos, paginação e ausência de resultados na KaBuM aprovado.
- JSONs locais: buscas-especificas-fluxo.json, buscas-especificas-vazio-corrigido.json, produtos-especificos-fontes.json, produtos-especificos-fontes-apos.json e regressao-eletronicos.json.

Para executar buscas, mantenha o worker em um terminal separado da API: `dotnet run --project Apps/Verum.Worker.Descoberta --launch-profile Verum.Worker.Descoberta`, a partir de src/backend. A saúde da API não comprova a presença do consumidor.
