# Verum Web

Interface web do ecossistema **Verum**, criada para sustentar as experiências do **Verum Busca** e do **Verum Radar** em navegadores desktop e mobile.

O produto tem como objetivo oferecer transparência na comparação de preços, indicar ofertas confiáveis e ajudar o consumidor a identificar o melhor momento para comprar.

> Projeto em desenvolvimento. A arquitetura prioriza entregas incrementais, baixo acoplamento e reaproveitamento da mesma base responsiva em uma futura experiência mobile empacotada.

## Sumário

- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Primeiros passos](#primeiros-passos)
- [Comandos disponíveis](#comandos-disponíveis)
- [Organização da aplicação](#organização-da-aplicação)
- [Regras de dependência](#regras-de-dependência)
- [Padrões de implementação](#padrões-de-implementação)
- [Responsividade e experiência mobile](#responsividade-e-experiência-mobile)
- [Estilos e identidade visual](#estilos-e-identidade-visual)
- [Autenticação](#autenticação)
- [Testes e qualidade](#testes-e-qualidade)
- [Integração contínua](#integração-contínua)
- [Convenções Git](#convenções-git)
- [Decisões técnicas](#decisões-técnicas)
- [Referências](#referências)

## Tecnologias

| Área                   | Tecnologia                                           |
| ---------------------- | ---------------------------------------------------- |
| Framework              | Angular 22 com componentes standalone e modo estrito |
| Linguagem              | TypeScript 6                                         |
| Componentes acessíveis | Angular Material e CDK                               |
| Estilização            | Tailwind CSS 4 e SCSS                                |
| Ícones                 | Lucide Angular                                       |
| Autenticação           | Keycloak JavaScript Adapter                          |
| Programação reativa    | RxJS e Signals do Angular                            |
| Testes unitários       | Vitest                                               |
| Qualidade              | ESLint e Prettier                                    |
| Integração contínua    | GitLab CI/CD                                         |

As versões exatas e reproduzíveis estão registradas em `package-lock.json`.

## Pré-requisitos

- Node.js `22.22.3+`, `24.15.0+` ou `26+`;
- npm compatível com a versão do Node.js utilizada;
- Git.

As faixas aceitas pelo projeto devem permanecer declaradas em `package.json#engines`. O repositório adota Node.js 24 como versão padrão.

Com NVM:

```bash
nvm use
```

Caso a versão ainda não esteja instalada:

```bash
nvm install
nvm use
```

## Primeiros passos

Instale exatamente as dependências registradas no lockfile:

```bash
npm ci
```

Inicie o servidor de desenvolvimento:

```bash
npm start
```

A aplicação ficará disponível em [http://localhost:4200](http://localhost:4200).

`npm ci` é preferível a `npm install` para preparar uma cópia limpa do projeto. Use `npm install <pacote>` somente quando a intenção for adicionar ou atualizar uma dependência e versionar a alteração correspondente no `package-lock.json`.

## Comandos disponíveis

| Comando                    | Finalidade                                                  |
| -------------------------- | ----------------------------------------------------------- |
| `npm start`                | Inicia o servidor local de desenvolvimento                  |
| `npm run build`            | Gera o build da aplicação                                   |
| `npm run build:production` | Gera explicitamente o build com configuração de produção    |
| `npm run watch`            | Mantém o build em modo de observação para desenvolvimento   |
| `npm test`                 | Executa os testes unitários em modo interativo              |
| `npm run test:ci`          | Executa os testes uma única vez e gera cobertura            |
| `npm run lint`             | Analisa TypeScript e templates Angular estaticamente        |
| `npm run lint:fix`         | Corrige automaticamente os problemas suportados pelo ESLint |
| `npm run format`           | Formata os arquivos suportados pelo Prettier                |
| `npm run format:check`     | Verifica a formatação sem alterar arquivos                  |
| `npm run check`            | Executa a verificação completa antes de um Merge Request    |

O script `check` deve representar a mesma barreira de qualidade aplicada pelo pipeline: formatação, lint, testes e build de produção.

## Organização da aplicação

Dentro de `src/app`, a aplicação possui somente três agrupamentos principais:

```text
src/app/
├── core/
├── domain/
└── shared/
```

Arquivos do shell da aplicação, como `app.ts`, `app.config.ts` e `app.routes.ts`, permanecem diretamente em `src/app`.

### `core`

Reúne infraestrutura e configurações com ciclo de vida global:

- inicialização e configuração da autenticação;
- guards de autenticação e autorização;
- interceptadores HTTP;
- configuração da API e feature flags;
- tratamento global de erros;
- serviços verdadeiramente globais.

`core` não conhece regras, páginas nem componentes internos dos domínios. Serviços globais devem preferir `providedIn: 'root'`. Configurações que não representam serviços devem ser expostas por funções providers ou `InjectionToken` devidamente tipados.

### `domain`

Contém funcionalidades e regras específicas de cada produto ou contexto da interface.

Cada domínio é organizado primeiro por **fluxo funcional**, mantendo próximos o componente, template, estilo, teste, modelo de apresentação e serviço específico daquele fluxo:

```text
domain/
└── busca/
    ├── inicio/
    ├── processamento/
    ├── resultados/
    ├── validators/
    └── busca.routes.ts
```

Funcionalidades com rotas devem expor um arquivo `<dominio>.routes.ts` e ser carregadas sob demanda por `app.routes.ts`.

Um domínio não acessa detalhes internos de outro. A comunicação deve ocorrer por contratos públicos pequenos, navegação por rotas ou serviços de orquestração claramente definidos.

### `shared`

Reúne elementos reutilizáveis sem regra de negócio específica:

- cabeçalho, rodapé e navegação;
- modal de confirmação;
- feedback de carregamento, erro e estado vazio;
- componentes de formulário genéricos;
- diretivas e pipes reutilizáveis;
- tipos e enumerações realmente compartilhados.

`shared` não importa `core` nem `domain`. Se um componente conhece conceitos específicos de Busca ou Radar, ele pertence ao respectivo domínio.

## Regras de dependência

| Origem             | Pode importar                               | Não pode importar                  |
| ------------------ | ------------------------------------------- | ---------------------------------- |
| Shell da aplicação | `core`, `domain`, `shared`                  | Detalhes privados de uma feature   |
| `core`             | Angular e bibliotecas de infraestrutura     | `domain` e `shared`                |
| `domain`           | APIs públicas de `core` e `shared`          | Detalhes internos de outro domínio |
| `shared`           | Angular, Material/CDK e utilitários neutros | `core` e `domain`                  |

As restrições estruturais mais importantes devem ser verificadas pelo ESLint. Os aliases disponíveis são:

- `@core/*`;
- `@domain/*`;
- `@shared/*`.

## Padrões de implementação

### Componentes

- componentes novos devem ser standalone;
- utilizar `ChangeDetectionStrategy.OnPush`;
- manter arquivos relacionados e seus testes no mesmo fluxo funcional;
- usar o prefixo `verum-` nos seletores próprios;
- manter templates sem regras de negócio complexas;
- fornecer estados de carregamento, vazio, erro e sucesso quando aplicável;
- preservar navegação por teclado, foco visível, rótulos e atributos ARIA relevantes.

### TypeScript e estado

- Signals atendem ao estado local e à maior parte do estado de uma funcionalidade;
- `computed` representa valores derivados, sem duplicação de estado;
- RxJS permanece para HTTP, eventos assíncronos e composição de streams;
- subscriptions manuais devem possuir encerramento explícito, preferencialmente com `takeUntilDestroyed`;
- formulários devem ser tipados, não anuláveis quando possível e validados de forma reutilizável;
- métodos devem expressar intenção e possuir uma responsabilidade clara;
- evitar `any`, efeitos colaterais ocultos e abstrações criadas antes de uma necessidade real.

Uma biblioteca global de estado só será adicionada quando houver coordenação complexa e recorrente entre domínios que Signals e serviços locais não resolvam com clareza.

### Nomenclatura

- nomes de arquivos usam kebab-case;
- classes, interfaces e tipos usam PascalCase;
- variáveis, Signals e métodos usam camelCase;
- textos apresentados ao usuário e nomes funcionais do produto permanecem em português;
- nomes técnicos consolidados pelo ecossistema Angular não devem ser traduzidos artificialmente.

Exemplo de geração:

```bash
npx ng generate component domain/busca/resultados/lista-ofertas \
  --change-detection OnPush \
  --standalone
```

## Responsividade e experiência mobile

A mesma base Angular atende desktop e navegadores mobile. Cada fluxo deve ser validado nas duas experiências antes de ser considerado concluído.

Diretrizes:

- adotar abordagem mobile-first quando ela simplificar o componente;
- reaproveitar regras, modelos e comportamento entre os layouts;
- permitir diferenças de composição quando desktop e mobile exigirem hierarquias visuais distintas;
- evitar lógica TypeScript baseada apenas em largura de tela quando CSS responsivo resolver o problema;
- considerar áreas seguras, navegação inferior e alvos de toque adequados em telas pequenas;
- não depender de hover para ações essenciais.

PWA, Capacitor ou um contêiner WebView ainda não fazem parte da fundação. Eles serão avaliados quando os principais fluxos responsivos estiverem maduros, sem duplicar o domínio da aplicação.

## Estilos e identidade visual

A divisão de responsabilidades é:

- **Tailwind CSS:** primeira opção para layout, responsividade, espaçamento, tipografia e estados visuais;
- **SCSS do componente:** pseudo-elementos, animações, gradientes complexos e mecanismos visuais que ficariam menos legíveis como utilities;
- **SCSS global:** tema do Angular Material, tokens da marca, resets e estilos realmente globais;
- **Angular Material/CDK:** comportamento, acessibilidade e primitivas interativas;
- **Lucide Angular:** conjunto único de ícones vetoriais carregados com a aplicação.

Evite duplicar em SCSS uma composição que possa ser expressa claramente com Tailwind. Também evite aplicar seletores sobre elementos internos privados do Angular Material; customizações devem utilizar o sistema de tema ou APIs públicas.

### Paleta principal

| Papel        | Token       | Valor     |
| ------------ | ----------- | --------- |
| Confiança    | Navy        | `#0B1D3A` |
| Inteligência | Azul        | `#1D4ED8` |
| Economia     | Verde       | `#10B981` |
| Oportunidade | Verde-claro | `#A7F3D0` |
| Superfície   | Cinza-claro | `#F1F5F9` |
| Texto        | Grafite     | `#1F2937` |

Os tokens devem ser consumidos por variáveis CSS e pelas utilities configuradas no Tailwind, evitando cores repetidas sem significado semântico.

Os arquivos de marca ficam em `public/images/marca`. Prefira SVG para logotipos e ícones da identidade e preserve fundos transparentes. PNG deve ser reservado para casos em que o formato raster seja realmente necessário.

## Autenticação

O `keycloak-js` será responsável pela integração do frontend com o Keycloak, que também fará o broker do login com Google.

A configuração depende de URL, realm e client ID de cada ambiente. Quando implementada, deverá respeitar as seguintes regras:

- o cliente Angular é público e não possui client secret;
- o login utiliza Authorization Code Flow com PKCE;
- redirect URIs e web origins são restritos por ambiente;
- tokens não são persistidos em `localStorage`;
- autenticação no frontend não substitui autorização na API;
- nenhuma variável entregue ao navegador é tratada como segredo.

## Testes e qualidade

- testes unitários utilizam Vitest;
- validators, transformações e regras de apresentação devem possuir testes diretos;
- componentes devem ser testados pelo comportamento observável, não por detalhes internos;
- correções de regressão devem incluir um teste capaz de reproduzir o problema;
- o relatório de cobertura orienta a análise, mas não substitui testes relevantes.

Antes de abrir um Merge Request:

```bash
npm run check
```

## Integração contínua

O `.gitlab-ci.yml` executa as verificações em uma imagem Node.js compatível com Angular 22 e utiliza `package-lock.json` como chave do cache npm.

O pipeline possui barreiras independentes para:

1. formatação e análise estática;
2. testes unitários e cobertura;
3. auditoria de vulnerabilidades das dependências de produção;
4. build com configuração de produção.

Pipelines de branch são evitados quando já existe um Merge Request aberto para o mesmo trabalho. Jobs obsoletos podem ser interrompidos por commits mais recentes, e o build gerado é armazenado como artefato temporário.

Publicação de imagem e deploy serão adicionados quando hospedagem, registro de imagens e ambientes estiverem definidos. O artefato validado deverá ser promovido entre ambientes sem recompilação.

## Convenções Git

- versionar `package-lock.json`;
- não versionar `node_modules`, builds, cobertura, relatórios, caches ou arquivos `.env`;
- nunca incluir credenciais, tokens ou client secrets no repositório;
- manter commits pequenos, coesos e revisáveis;
- utilizar Conventional Commits.

Exemplos:

```text
feat(busca): adiciona formulário inicial
fix(resultados): corrige expansão simultânea dos cards
refactor(shared): simplifica navegação mobile
test(busca): cobre validação do termo pesquisado
docs(readme): atualiza convenções do projeto
```

## Decisões técnicas

- **Sem NgRx inicialmente:** Signals e serviços por domínio atendem ao estado atual;
- **Sem biblioteca adicional de componentes:** Material/CDK fornecem a fundação necessária;
- **Lucide como biblioteca única de ícones:** evita dependência de fontes externas e mantém consistência visual;
- **Sem paginação inicial nos resultados:** o backend entregará uma seleção curta, filtrada e ranqueada;
- **Sem PWA ou Capacitor neste momento:** a prioridade é validar os fluxos responsivos no navegador;
- **Sem segredos no frontend:** qualquer valor enviado ao browser deve ser considerado público;
- **Sem abstrações especulativas:** novas camadas e bibliotecas exigem uma necessidade concreta do produto.

## Referências

- [Angular Style Guide](https://angular.dev/style-guide)
- [Compatibilidade de versões do Angular](https://angular.dev/reference/versions)
- [Tailwind CSS com Angular](https://angular.dev/guide/tailwind)
- [Angular Material](https://material.angular.dev/guide/getting-started)
- [Keycloak JavaScript Adapter](https://www.keycloak.org/securing-apps/javascript-adapter)
- [Conventional Commits](https://www.conventionalcommits.org/pt-br/v1.0.0/)
- [GitLab CI/CD](https://docs.gitlab.com/ci/)
