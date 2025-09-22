$ErrorActionPreference = 'Stop'
$base = Join-Path $PSScriptRoot '..' | Resolve-Path
$nfeDir = Join-Path $base 'Schemas/NFe/v4.00'
New-Item -ItemType Directory -Force -Path $nfeDir | Out-Null
Write-Host 'Baixe manualmente o pacote de esquemas da NF-e (PL_009_V4) no portal e extraia aqui:' -ForegroundColor Yellow
Write-Host $nfeDir -ForegroundColor Cyan

