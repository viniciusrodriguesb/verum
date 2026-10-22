[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot

$settings = @{}

foreach ($line in Get-Content -LiteralPath (Join-Path $projectRoot '.env')) {
  if ($line -match '^([A-Z_]+)=(.*)$') {
    $settings[$Matches[1]] = $Matches[2]
  }
}

$address = 'http://localhost:' + $settings['KEYCLOAK_PORT']

$metadata = Invoke-RestMethod -Uri ($address + '/realms/verum/.well-known/openid-configuration')

if ($metadata.issuer -ne ($address + '/realms/verum')) {
  throw 'Issuer diferente do hostname local esperado.'
}

$token = Invoke-RestMethod -Method Post -Uri ($address + '/realms/master/protocol/openid-connect/token') -Body @{
  client_id = 'admin-cli'
  grant_type = 'password'
  username = $settings['KEYCLOAK_ADMIN_USERNAME']
  password = $settings['KEYCLOAK_ADMIN_PASSWORD']
}

$headers = @{ Authorization = 'Bearer ' + $token.access_token }

$realm = Invoke-RestMethod -Headers $headers -Uri ($address + '/admin/realms/verum')

$clients = Invoke-RestMethod -Headers $headers -Uri ($address + '/admin/realms/verum/clients?clientId=verum-web')

$client = $clients[0]

if (!$realm.registrationAllowed -or !$realm.verifyEmail -or !$realm.resetPasswordAllowed -or
  !$realm.registrationEmailAsUsername -or $realm.duplicateEmailsAllowed) {
  throw 'Cadastro e verificação de e-mail divergentes do padrão do Verum.'
}

if ($realm.browserFlow -ne 'browser' -or $realm.registrationFlow -ne 'registration') {
  throw 'O Verum deve utilizar os fluxos nativos de browser e cadastro.'
}

$registrationExecutions = Invoke-RestMethod -Headers $headers -Uri ($address + '/admin/realms/verum/authentication/flows/registration/executions')

if (!($registrationExecutions | Where-Object { $_.providerId -eq 'registration-password-action' -and $_.requirement -eq 'REQUIRED' })) {
  throw 'A validação nativa de senha deve permanecer obrigatória no cadastro.'
}

if (!$realm.internationalizationEnabled -or $realm.defaultLocale -ne 'pt-BR' -or $realm.supportedLocales -notcontains 'pt-BR') {
  throw 'O login, cadastro e recuperação devem usar português do Brasil.'
}

if ($realm.loginTheme -ne 'verum') {
  throw 'Use o tema verum, herdado do tema oficial, para o cadastro nativo nesta versão.'
}

if (!$client.publicClient -or !$client.standardFlowEnabled -or $client.directAccessGrantsEnabled -or
  $client.implicitFlowEnabled -or $client.serviceAccountsEnabled -or
  $client.attributes.'pkce.code.challenge.method' -ne 'S256') {
  throw 'O cliente Angular deve usar Authorization Code com PKCE S256 e sem segredo.'
}

if ($client.redirectUris.Count -ne 1 -or $client.redirectUris[0] -ne ($settings['VERUM_WEB_URL'] + '/*') -or
  $client.webOrigins.Count -ne 1 -or $client.webOrigins[0] -ne $settings['VERUM_WEB_URL']) {
  throw 'Origens e redirecionamentos diferentes do frontend configurado.'
}

if (!($client.protocolMappers | Where-Object {
  $_.protocolMapper -eq 'oidc-audience-mapper' -and $_.config.'included.client.audience' -eq 'verum-api'
})) {
  throw 'Tokens de acesso devem incluir a audiência verum-api.'
}

$scopes = Invoke-RestMethod -Headers $headers -Uri ($address + '/admin/realms/verum/clients/' + $client.id + '/default-client-scopes')

if ($scopes.name -notcontains 'basic') {
  throw 'O escopo nativo basic deve emitir o sub estável no token de acesso.'
}

$providers = Invoke-RestMethod -Headers $headers -Uri ($address + '/admin/realms/verum/components?type=org.keycloak.storage.UserStorageProvider')

if ($providers.Count -gt 0) {
  throw 'Foi encontrada federação de usuários; o ambiente deve usar usuários nativos.'
}

$null = Invoke-RestMethod -Uri ('http://localhost:' + $settings['MAILPIT_PORT'] + '/api/v1/messages')

Write-Host 'OK: issuer, realm, cadastro, verificação, reset, PKCE, audiência, origens e Mailpit.'

$headers.Clear()

$settings.Clear()

$token = $null
