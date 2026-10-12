[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot

$environmentFile = Join-Path $projectRoot '.env'

if (Test-Path -LiteralPath $environmentFile) {
  Write-Host '.env existente preservado.'

  return
}

function New-LocalSecret {
  $bytes = New-Object byte[] 24

  $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()

  try {
    $random.GetBytes($bytes)

    return [BitConverter]::ToString($bytes).Replace('-', '').ToLowerInvariant()
  }
  finally {
    $random.Dispose()
  }
}

$template = Get-Content -LiteralPath (Join-Path $projectRoot '.env.example') -Raw

$template = $template.Replace('KEYCLOAK_ADMIN_PASSWORD=', 'KEYCLOAK_ADMIN_PASSWORD=' + (New-LocalSecret))

$template = $template.Replace('POSTGRES_PASSWORD=', 'POSTGRES_PASSWORD=' + (New-LocalSecret))

[System.IO.File]::WriteAllText($environmentFile, $template)

Write-Host '.env local criado com senhas aleatórias. Consulte-o para acessar o console administrativo.'
