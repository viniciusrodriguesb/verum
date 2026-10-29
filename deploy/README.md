# Deploy independente do Verum

O destino inicial é uma VPS Linux com Docker Engine e Compose, arquitetura x86_64. A hospedagem compartilhada tradicional não atende a API e workers. Não há servidor, domínio ou credenciais configurados ainda: os arquivos estão preparados; nenhuma implantação pública foi realizada.

## Unidades de implantação

| Unidade       | Imagem GHCR       | Build                              | Deploy no Actions                                      |
| ------------- | ----------------- | ---------------------------------- | ------------------------------------------------------ |
| Frontend      | web               | Frontend CI, após testes/auditoria | Deploy Frontend                                        |
| API           | api               | Backend CI                         | Deploy API                                             |
| Descoberta    | worker-descoberta | Backend CI                         | Deploy Worker Descoberta                               |
| Radar         | worker-radar      | Backend CI                         | Deploy Worker Radar                                    |
| Keycloak      | keycloak          | Keycloak CI                        | Deploy Keycloak                                        |
| Migrations EF | migrations        | Backend CI                         | Deploy Migrations ou etapa anterior ao serviço backend |

Cada aplicação possui manifesto próprio. Uma implantação usa compose up --no-deps somente no serviço escolhido; não derruba os demais projetos. Não há container_name fixo, imagem latest da aplicação, segredos no build ou migrations na inicialização das réplicas. Bibliotecas internas .NET continuam distribuídas com os executáveis, sem deploy individual.

Mudanças comuns do backend validam a solução e geram imagens separadas dos três executáveis e das migrations. Os targets publicam cada executável individualmente, compartilhando cache e runtime quando possível. O frontend e o Keycloak possuem builds próprios. Uma falha em um target backend não cancela os demais targets, mas seu próprio deploy depende de uma imagem válida.

## Como funciona o GitHub Actions

PRs validam código e construção das imagens, sem publicar ou acessar a VPS. Em main, depois dos testes, o CI publica no GitHub Container Registry usando GITHUB_TOKEN. O GHCR não exige criar um registry separado: os pacotes aparecem no GitHub. O primeiro build cria cada pacote. Permita que o repositório escreva seus próprios packages; pacotes existentes precisam conceder acesso a este repositório.

A referência fica no resumo do job e em ghcr.io/OWNER/REPOSITORY/SERVICE:sha-COMMIT. Para implantar, use o digest sha256 indicado no resumo, não a tag. O deploy é manual em Actions, separado por aplicação e protegido pelo GitHub Environment. Isso permite publicar imagens sem promover automaticamente mudanças de banco ou de identidade.

Configure o Environment production e, se disponível no seu plano GitHub, proteção da branch main e revisores para produção. O deploy rejeita execução fora de main. No Environment, configure:

| Secret          | Valor                                                                            |
| --------------- | -------------------------------------------------------------------------------- |
| VPS_HOST        | IP ou hostname da VPS                                                            |
| VPS_USER        | Usuário Linux dedicado ao deploy                                                 |
| VPS_SSH_KEY     | Chave SSH privada desse usuário                                                  |
| VPS_KNOWN_HOSTS | Chave de host SSH verificada fora do pipeline, incluindo porta quando não for 22 |

Variáveis opcionais: VPS_PORT (22) e VPS_ROOT (/opt/verum). Não use ssh-keyscan durante o deploy para confiar automaticamente na primeira resposta. Confirme o fingerprint pelo painel/console confiável da VPS antes de cadastrar known_hosts.

A chave do deploy deve ser exclusiva deste repositório. O usuário precisa escrever em VPS_ROOT e executar Docker; acesso ao daemon Docker equivale a privilégios administrativos da máquina. Não use essa chave em jobs de PR. O token efêmero do GitHub é enviado pelo SSH para autenticar o pull no GHCR e o arquivo Docker de autenticação é removido ao final; não é necessário guardar um PAT permanente na VPS.

Os arquivos de ambiente ficam somente na VPS. O workflow não envia credenciais de banco nem SMTP, e não substitui os arquivos de configuração durante a implantação.

## Preparar a primeira VPS

1. Instale Docker Engine, o plugin Docker Compose, Bash, flock e tar. Crie o usuário de deploy, configure sua chave e dê a ele um diretório próprio /opt/verum.
2. Copie este repositório ou seus arquivos deploy para a VPS. Execute bash deploy/scripts/prepare-vps.sh /opt/verum com o usuário de deploy. O script cria diretórios e a rede verum (172.30.0.0/24), sem instalar pacotes ou remover recursos existentes.
3. Copie os exemplos de deploy/config para /opt/verum/config, removendo apenas o sufixo .example. Preencha todas as senhas/domínios e revise os arquivos; CHANGE_ME é apenas um marcador. Mantenha arquivos .env com permissão 600 e diretório config com 700. Defina também `export VERUM_CONFIG_DIR=/opt/verum/config` na sessão usada pelos comandos Compose. O arquivo público web.configuracao.json precisa de permissão 644 para ser lido pelo Nginx sem root.
4. Aponte os registros DNS A/AAAA dos hosts do frontend e do SSO para a VPS. Abra somente SSH restrito, HTTP 80 e HTTPS 443; banco, Redis, RabbitMQ, API e Keycloak não são publicados diretamente.
5. Inicie as dependências com docker compose --env-file /opt/verum/config/infra.env -p verum-infra -f deploy/infra/compose.yaml up -d --wait. Use senhas fortes diferentes por serviço. A identidade possui PostgreSQL e volume próprios.
6. Execute os CIs em main para publicar as imagens e obtenha seus digests.
7. Implante Keycloak com sua ação própria. O deploy garante a criação inicial de seu PostgreSQL, sem recriá-lo nos upgrades. Configure SMTP real; a imagem de produção não usa Mailpit, não contém senha e exige HTTPS. O import inicial cria o realm apenas quando ele ainda não existe.
8. Implante API e workers, informando também o digest da imagem migrations da mesma release. Implante o frontend. Os pacotes pagos e a IA continuam desabilitados até sua configuração específica.
9. Inicie a entrada HTTPS com docker compose --env-file /opt/verum/config/edge.env -p verum-edge -f deploy/edge/compose.yaml up -d. O Caddy gerencia certificados públicos e envia /api ao backend, o restante ao frontend, e o host do SSO ao Keycloak.

O mesmo endereço público do frontend é usado pela API, preservando cookies e chamadas relativas /api sem CORS adicional. O Angular lê a configuração pública montada em runtime; trocar a URL do Keycloak não exige recompilar o frontend. Mantenha HEALTHCHECK_HOST igual ao host permitido da API. API e Keycloak confiam apenas na rede privada do proxy. Se mudar o subnet, ajuste também ReverseProxy__KnownNetworks e KC_PROXY_TRUSTED_ADDRESSES.

A API persiste as chaves de Data Protection em volume próprio, preservando o cookie de visitante entre atualizações. Não remova volumes durante o deploy. Em várias VPS, as chaves precisam de armazenamento compartilhado e protegido; um volume local não atende a vários hosts.

## Migrations e compatibilidade

As migrations são bundles EF Core em imagem própria, sem instalar SDK no servidor. Cada deploy de API/worker exige o digest das migrations da mesma release. O executor usa uma trava de arquivo comum na VPS; se esse digest já foi aplicado com sucesso, não reaplica. O comando de migração também pode ser executado pela ação Deploy Migrations, antes de qualquer aplicação.

Cada contexto mantém seu schema e histórico; não existe uma transação única entre todos os schemas durante a atualização. Em falha, o serviço escolhido não é atualizado, o marcador não é gravado e a execução seguinte retoma as migrations faltantes. Não são executadas migrations Down automaticamente.

Mudanças de banco e contratos de mensagens devem seguir expandir/migrar/remover: versões antiga e nova podem coexistir durante o deploy independente. Uma remoção de coluna ou mudança incompatível exige plano específico; as travas não tornam uma mudança destrutiva compatível.

## Operação e limites

Os manifests usam imagens por digest, volumes de dados, recursos limitados e logs com rotação. Frontend, API e workers usam usuário sem root, filesystem somente leitura e diretório temporário separado. Keycloak usa sua imagem otimizada oficial, sem permissões Linux adicionais. Seu runtime não é marcado read-only porque o import inicial e arquivos de execução do produto necessitam escrita.

Os workers contêm somente Chromium e suas dependências Linux na versão correspondente ao NuGet; o SDK não vai para o runtime. O health check do worker confirma o processo, não a conexão do consumidor, o atraso da fila ou o sucesso de um scraper. Não confunda isso com observabilidade completa. Workers precisam de espaço em memória compartilhada e o sandbox Chromium não foi habilitado automaticamente; os perfis de isolamento devem ser avaliados conforme o kernel da VPS, sem adicionar SYS_ADMIN ou privileged para contornar falhas.

A primeira versão usa uma VPS e recria o container escolhido, podendo haver breve indisponibilidade desse serviço. Deploy independente não significa zero downtime ou alta disponibilidade. API e workers podem ter réplicas quando necessário; a capacidade depende de memória, CPU, conexões PostgreSQL, broker e provedores. Os limites são tetos, não reservas; meça a carga antes de ajustar ou escolher o plano da VPS.

O exemplo de Descoberta habilita KaBuM, Amazon, Kalunga e Decathlon. Carrefour respondeu com bloqueio na carga local de 29/10/2026 e ficou fora da lista de produção. Seu perfil permanece disponível para revalidação explícita; não contorne CAPTCHA nem trate bloqueio como ausência de produto. Revalide todas as fontes no IP da VPS antes da abertura pública.

O estado da última implantação fica em /opt/verum/state/SERVICE.current e a anterior em SERVICE.previous. Para retornar uma aplicação, execute sua ação com o digest anterior e as migrations correspondentes. Verifique compatibilidade antes de voltar binários. Keycloak pode alterar seu banco interno durante upgrades: faça backup e siga suas notas de versão; não tente rollback automático sobre schema atualizado. Backups, restauração, alertas e políticas de retenção de imagens precisam ser operados na VPS.

Não há deploy automático do banco, do proxy ou de outra aplicação como efeito de atualizar um serviço. A única preparação automática é a migração coordenada do backend ou a criação inicial do banco próprio do Keycloak.

## Validar e ampliar

Para gerar dumps privados do banco da aplicação e do Keycloak, execute `VERUM_CONFIG_DIR=/opt/verum/config bash deploy/scripts/backup.sh /opt/verum/backups`. O script usa os projetos Compose existentes, não interrompe serviços, gera checksums e só publica o diretório quando os dois dumps terminam. Não remove backups antigos nem inclui credenciais, chaves de Data Protection ou definições do RabbitMQ. Proteja e copie esses itens separadamente; mantenha cópia externa criptografada e uma política de retenção. Os dumps dos dois bancos não representam um snapshot distribuído simultâneo.

Antes da primeira liberação, restaure cada dump em banco descartável com `pg_restore --exit-on-error --no-owner --no-acl`, valide migrations, contagens e login/fluxos. Nunca ensaie a restauração sobre o banco em uso. Configure a execução periódica no servidor apenas quando a infraestrutura existir; um arquivo de backup sem ensaio de restauração não comprova recuperação.

Execute bash deploy/scripts/validate.sh para validar todos os manifests com valores fictícios, sem subir containers. O CI verifica também os scripts. Builds locais usam o contexto da raiz: docker build -f src/backend/Dockerfile --target api -t verum-api:local .; os demais targets são worker-descoberta, worker-radar e migrations. Frontend e Keycloak têm Dockerfiles próprios.

Ao criar nova aplicação: inclua target/Dockerfile, manifesto, exemplos de configuração, validação CI e ação de deploy exclusiva, reaproveitando container-image.yml e deploy-vps.yml. Atualize as listas permitidas do deploy-service.sh, do workflow reutilizável, do validador e esta tabela. Essa decisão está registrada em AGENTS.md.

### Radar e SMTP

A API e o Worker Radar compartilham a política de Radar (`Radar__*`) e a habilitação do canal de e-mail. O worker recebe as credenciais SMTP em runtime; não usa o SMTP do Keycloak. Ative `Playwright__Enabled=true` no Worker Radar para renovar observações com os providers já existentes. A imagem do worker já inclui o runtime de navegador e segue com deploy independente.

Os exemplos começam com Radar e e-mail desabilitados. Antes de liberar Verum Plus: configure preços, confirme a capacidade do worker e habilite a política nos dois executáveis. Migrations de Radar/Notificações são aplicadas pelo processo de migrations existente, nunca ao subir réplicas. Veja [decisões do Radar](../docs/arquitetura.md#radar-e-notificações).
