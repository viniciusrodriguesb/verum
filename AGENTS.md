# Padrões permanentes do Verum

## Organização e comportamento
- Preserve o monólito modular .NET e as responsabilidades de cada módulo.
- Não altere regras de negócio ou o visual como consequência de ajustes de infraestrutura.
- No backend, mantenha uma linha vazia entre declarações, chamadas e métodos. Navegações continuam separadas visualmente das propriedades.
- Remova usings/imports sem uso; respeite EditorConfig, ESLint e Prettier.
- Opções, contratos/DTOs, serviços, controllers e processadores devem permanecer nos locais próprios.

## Deploy independente — decisão do usuário
- Toda aplicação executável possui imagem e deploy independentes: cada frontend, API, worker e Keycloak.
- Ao criar um novo frontend ou worker, inclua seu Dockerfile/target, manifesto de deploy, configuração de exemplo, pipeline com validação e documentação na mesma entrega.
- Siga o padrão de deploy/README.md e dos workflows existentes. Bibliotecas e módulos internos não recebem deploy próprio.
- Mudanças compartilhadas no backend devem validar todos os consumidores afetados. Cada aplicação deve poder ser atualizada sem reiniciar as demais.
- Use imagens imutáveis por digest, runtime sem root quando suportado, segredos somente em runtime e logs/recursos limitados.
- Migrations têm execução própria, serializada. Não aplique migrations na inicialização da API ou de cada réplica.
- Não publicar nem implantar em destino inferido. Sem infraestrutura/credenciais, prepare os arquivos e informe a etapa externa pendente.
