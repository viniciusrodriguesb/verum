# Verum

Plataforma brasileira de inteligência de compra, criada para oferecer transparência de preços, comparação de ofertas confiáveis e monitoramento de oportunidades.

O ecossistema é composto inicialmente pelo **Verum Busca** e pelo **Verum Radar**.

## Estrutura do repositório

| Caminho             | Responsabilidade                                  |
| ------------------- | ------------------------------------------------- |
| `src/frontend/web`  | Aplicação web Angular                             |
| `src/backend`       | API, módulos e workers .NET                       |
| `tests`             | Testes de integração e arquitetura                |
| `deploy`            | Docker, Kubernetes e configurações de implantação |
| `docs`              | Documentação e decisões arquiteturais             |
| `.github/workflows` | Pipelines de integração contínua                  |

## Aplicações previstas

| Aplicação        | Tecnologia             | Situação           |
| ---------------- | ---------------------- | ------------------ |
| Verum Web        | Angular 22             | Em desenvolvimento |
| Verum API        | ASP.NET Core / .NET 10 | Planejada          |
| Discovery Worker | .NET Worker Service    | Planejado          |
| Radar Worker     | .NET Worker Service    | Planejado          |

## Executando o frontend

```bash
cd src/frontend/web
npm ci
npm start
```
