[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$settings = @{}

foreach ($line in Get-Content -LiteralPath (Join-Path (Split-Path -Parent $PSScriptRoot) '.env')) {
  if ($line -match '^([A-Z_]+)=(.*)$') { $settings[$Matches[1]] = $Matches[2] }
}

$address = 'http://localhost:' + $settings['KEYCLOAK_PORT']

$token = Invoke-RestMethod -Method Post -Uri ($address + '/realms/master/protocol/openid-connect/token') -Body @{
  client_id = 'admin-cli'
  grant_type = 'password'
  username = $settings['KEYCLOAK_ADMIN_USERNAME']
  password = $settings['KEYCLOAK_ADMIN_PASSWORD']
}

$headers = @{ Authorization = 'Bearer ' + $token.access_token }

try {
  # Tema verum herda keycloak.v2 e corrige apenas a validação do campo de senha ausente.
  $body = @{ internationalizationEnabled = $true; supportedLocales = @('pt-BR'); defaultLocale = 'pt-BR'; loginTheme = 'verum' } | ConvertTo-Json

  Invoke-RestMethod -Method Put -Headers $headers -ContentType 'application/json' -Uri ($address + '/admin/realms/verum') -Body $body

  Write-Host 'Realm Verum atualizado: pt-BR e tema herdado verum. Usuários e fluxos preservados.'
}
finally {
  $headers.Clear()

  $settings.Clear()

  $token = $null
}
