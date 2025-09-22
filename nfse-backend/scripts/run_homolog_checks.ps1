$ErrorActionPreference = 'Stop'
$api = 'http://localhost:5000/api/nfe/homolog/status'
Write-Host "Verificando checklist de homologacao em $api"
try {
  $resp = Invoke-RestMethod -Method Get -Uri $api
  $resp | ConvertTo-Json -Depth 5
} catch {
  Write-Error $_
}

