# Verum

Plataforma brasileira de inteligência de compra, criada para oferecer transparência de preços, comparação de ofertas confiáveis e monitoramento de oportunidades.

O ecossistema é composto inicialmente pelo **Verum Busca** e pelo **Verum Radar**.

## Estrutura do repositório

| Caminho             | Responsabilidade                                  |
| ------------------- | ------------------------------------------------- |
| `src/frontend/web`  | Aplicação web Angular                             |
| `src/backend`       | API, módulos e workers .NET                       |
| `src/keycloak`      | Identidade local isolada: Keycloak, PostgreSQL e Mailpit |
| `tests`             | Testes de integração e arquitetura                |
| `deploy`            | Docker Compose e configurações de implantação |
| `docs`              | Documentação e decisões arquiteturais             |
| `.github/workflows` | Pipelines de integração contínua                  |

## Aplicações previstas

| Aplicação        | Tecnologia             | Situação           |
| ---------------- | ---------------------- | ------------------ |
| Verum Web        | Angular 22             | Em desenvolvimento |
| Verum API        | ASP.NET Core / .NET 10 | Implementada       |
| Discovery Worker | .NET Worker Service    | Base implementada  |
| Radar Worker     | .NET Worker Service    | Base implementada  |

## Implantação

Frontend, API, Worker Descoberta, Worker Radar e Keycloak possuem imagens e deploys independentes para VPS com Docker. O CI publica imagens no GHCR; a promoção de cada serviço é feita por sua ação de deploy no GitHub. Migrations usam executor separado e trava comum.

Veja [o guia de implantação](deploy/README.md) para preparar a VPS, configurar secrets, domínio e executar os deploys. A infraestrutura remota ainda não foi contratada ou configurada. Novas aplicações devem seguir o padrão registrado em [AGENTS.md](AGENTS.md).

## Executando o frontend

```bash
cd src/frontend/web
npm ci
npm start
```
