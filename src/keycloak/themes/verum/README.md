# Tema Verum

Protótipo visual do acesso ao ecossistema Verum, herdando os formulários e comportamentos nativos de `keycloak.v2` (26.8.0).

A identidade fica em `login/resources/css/verum.css`: fundo claro, painel central responsivo, azul nas ações e detalhe verde. O logotipo em `resources/img` reutiliza o arquivo do frontend. As fontes usam a pilha do sistema quando Inter e Plus Jakarta Sans não estão disponíveis; não há dependências externas. A apresentação é clara independentemente do tema do dispositivo.

`theme.properties` carrega primeiro o CSS nativo e depois a personalização. Não há alterações de campos, endpoints, validações, mensagens, credenciais ou fluxo de autenticação nesta personalização. O Compose monta o tema local e o Dockerfile já o inclui na imagem independente do Keycloak.

## Correção funcional preexistente

`login/register.ftl` é baseado no template oficial 26.8.0 (Apache 2.0; licença incluída). Com confirmação de e-mail, o fluxo nativo define a senha após a verificação. O template dessa versão inclui o script de validação mesmo sem o campo, causando `Cannot read properties of null (reading 'addEventListener')`.

A chamada `validator.script` permanece protegida por `passwordRequired??`, condição também usada pelo template para renderizar os campos. Não há JavaScript personalizado nem redução da validação de senha no servidor. Essa correção antecede o protótipo visual.

Ao atualizar Keycloak, compare o template com a versão oficial e remova a sobrescrita quando houver correção na origem. Valide login, cadastro, recuperação e definição de senha, mensagens de erro e navegação por teclado, inclusive em 320 px. A versão do template acompanha a imagem fixada no Compose/Dockerfile.
