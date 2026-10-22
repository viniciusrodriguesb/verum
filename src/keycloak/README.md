# Keycloak do Verum

Ambiente local independente para identidade do Verum: Keycloak oficial, PostgreSQL próprio e Mailpit para capturar e-mails de confirmação e recuperação de senha. Não depende do Keycloak do PE+ e não modifica seus containers, banco, SPI ou temas.

## Executar

Em `src/keycloak`, execute:

```powershell
./scripts/Initialize-Local.ps1
docker compose up -d --wait
./scripts/Test-Configuration.ps1
```

O script cria `.env` com senhas aleatórias e preserva um arquivo já existente. O arquivo não é versionado. O usuário administrativo padrão é `verum-admin`; sua senha fica no `.env`.

| Recurso | Endereço |
| --- | --- |
| Console administrativo | http://localhost:8081/admin/ |
| Conta do usuário | http://localhost:8081/realms/verum/account/ |
| OpenID Connect | http://localhost:8081/realms/verum/.well-known/openid-configuration |
| E-mails locais | http://localhost:8025/ |
| SMTP local para aplicações | localhost:1025 (somente loopback) |

Use `localhost` consistentemente. As portas publicadas são vinculadas somente à máquina local. Banco, SMTP e porta de gerenciamento do Keycloak não são publicados. O projeto Compose, a rede e os volumes são exclusivos do Verum.

## Configuração e responsabilidades

`realms/verum-realm.json` define o realm inicial. Cadastro usa e-mail como identificador de login e senha de pelo menos 12 caracteres. O fluxo nativo desta versão confirma o e-mail antes de pedir a definição da senha. Login, cadastro, recuperação e vinculação de identidade usam os fluxos e temas padrão. Google será adicionado posteriormente pelo broker nativo.

`verum-web` é um cliente público para Angular, sem segredo, com Authorization Code e PKCE S256. Password grant e implicit flow estão desabilitados. Redirecionamentos e origens ficam restritos ao endereço de `VERUM_WEB_URL`. O escopo nativo `basic` inclui o `sub` no token de acesso; o mapper de audiência inclui `verum-api`. O cliente da API apenas representa o recurso protegido.

Contas e permissões comerciais continuam no backend. O vínculo usa o `sub` estável do Keycloak, nunca e-mail, CPF ou uma identidade por empresa. Não há federação de usuários, SPI, GOV.BR ou fluxo personalizado neste ambiente. O banco interno do Keycloak é gerenciado pelo próprio servidor, com seus nomes e migrations nativos; não é gerenciado por EF Core.

O Angular usa `verum-web` para login, cadastro, logout e gerenciamento da identidade. Configure URL, realm e client em `src/frontend/web/public/configuracao.json`; o arquivo é carregado antes da aplicação. A API local aponta para `http://localhost:8081/realms/verum`, com audiência `verum-api`. Ao iniciar uma sessão autenticada, o frontend chama `POST /api/v1/contas/me`, que sincroniza a conta e provisiona FREE. Buscas autenticadas pertencem à conta; buscas anônimas pertencem ao visitante. Planos, concessões e cotas ficam no módulo Acesso, sem roles comerciais no Keycloak. Login não migra buscas anônimas, e bloqueio da conta interna é verificado mesmo quando o JWT ainda está válido.

## Persistência e alterações

O volume PostgreSQL mantém usuários, vínculos e configurações. `docker compose down` para somente este projeto e preserva os volumes. Não remova volumes para aplicar alterações de configuração.

O Keycloak importa o JSON na criação inicial e ignora a importação quando o realm já existe. Alterar o JSON ou `.env` não sobrescreve um realm existente. Ajuste a instância pela administração e mantenha o arquivo inicial alinhado quando mudar a configuração. Senhas de bootstrap só criam o administrador inicial; editar `.env` não troca a senha existente.

Versões de imagens são fixadas no Compose. Atualizações exigem leitura das notas de versão e backup do banco. O Mailpit mantém somente mensagens locais; não existe SMTP externo ou encaminhamento de e-mails reais.

O runtime usa init para encaminhar sinais e recolher processos, prazo de 30 segundos para encerrar, logs locais limitados a três arquivos de 10 MB e bloqueio de elevação de privilégios. O Keycloak usa o usuário não root da imagem oficial e não recebe capabilities adicionais. Os limites locais de memória são 2 GB para Keycloak, 512 MB para PostgreSQL e 256 MB para Mailpit; revise-os conforme a carga medida.

A implantação de produção usa imagens e pipelines independentes para frontend, API, workers e Keycloak. Consulte o [guia de implantação](../../deploy/README.md). Este Compose permanece exclusivo para desenvolvimento local.

Este Compose usa `start-dev` e HTTP para desenvolvimento. Produção exige hostname público HTTPS, configuração de proxy conforme a implantação, SMTP real e gestão de segredos. Não reutilize o `.env` local em produção. Observabilidade completa fica para a entrega correspondente; o health check aqui apenas valida a prontidão do container.

## Idioma e tema

O realm usa pt-BR e o tema verum, herdado de keycloak.v2. Apenas o template de cadastro possui uma correção pontual para a versão 26.8.0: a validação de senha só inicia quando o campo existe. A confirmação de e-mail e a senha após verificação continuam nativas. Consulte [a origem e manutenção do tema](themes/verum/README.md).

Em um realm local já criado, execute `./scripts/Set-LocalPresentation.ps1` e depois `./scripts/Test-Configuration.ps1`. O script aplica somente idioma e tema, sem reimportar usuários ou fluxos. O Compose monta o tema somente para leitura; a imagem de produção o incorpora. Alterar arquivos de import não atualiza um realm já existente.
