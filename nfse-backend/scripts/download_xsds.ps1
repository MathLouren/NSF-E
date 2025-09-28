# Script para download automático dos XSDs oficiais da NFe
# Executa: powershell -ExecutionPolicy Bypass -File download_xsds.ps1

param(
    [string]$TargetPath = ".\Schemas",
    [switch]$Force = $false
)

Write-Host "=== DOWNLOAD DE XSDs OFICIAIS DA NFe ===" -ForegroundColor Green

$urls = @{
    "NFe_v4.00" = "https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=QWjIzJXRDOo%3D"
    "NFSe_Nacional" = "https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/leiaute-e-esquemas-atuais/xsd_pl_nfse_1-00-producao.zip/view"
}

# Criar diretórios
$nfeDir = Join-Path $TargetPath "NFe\v4.00"
$nfseDir = Join-Path $TargetPath "NFSe"

if (-not (Test-Path $nfeDir)) {
    New-Item -ItemType Directory -Path $nfeDir -Force | Out-Null
    Write-Host "Diretório criado: $nfeDir" -ForegroundColor Yellow
}

if (-not (Test-Path $nfseDir)) {
    New-Item -ItemType Directory -Path $nfseDir -Force | Out-Null
    Write-Host "Diretório criado: $nfseDir" -ForegroundColor Yellow
}

# Função para download com retry
function Download-WithRetry {
    param($Url, $OutputPath, $MaxRetries = 3)
    
    for ($i = 1; $i -le $MaxRetries; $i++) {
        try {
            Write-Host "Tentativa $i de download: $Url" -ForegroundColor Cyan
            Invoke-WebRequest -Uri $Url -OutFile $OutputPath -UseBasicParsing
            Write-Host "Download concluído: $OutputPath" -ForegroundColor Green
            return $true
        }
        catch {
            Write-Host "Erro na tentativa $i : $($_.Exception.Message)" -ForegroundColor Red
            if ($i -eq $MaxRetries) {
                Write-Host "Falha definitiva no download de $Url" -ForegroundColor Red
                return $false
            }
            Start-Sleep -Seconds (2 * $i)
        }
    }
    return $false
}

# Download NFe XSDs
Write-Host "`nBaixando XSDs da NFe v4.00..." -ForegroundColor Cyan

# Lista de XSDs principais da NFe v4.00 (URLs diretas quando disponíveis)
$nfeXsds = @(
    @{ Name = "leiauteNFe_v4.00.xsd"; Url = "https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=QWjIzJXRDOo%3D" }
    @{ Name = "xmldsig-core-schema_v1.01.xsd"; Url = "https://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd" }
)

foreach ($xsd in $nfeXsds) {
    $outputPath = Join-Path $nfeDir $xsd.Name
    if ((Test-Path $outputPath) -and -not $Force) {
        Write-Host "Arquivo já existe: $($xsd.Name) (use -Force para sobrescrever)" -ForegroundColor Yellow
        continue
    }
    
    $success = Download-WithRetry -Url $xsd.Url -OutputPath $outputPath
    if (-not $success) {
        Write-Host "AVISO: Falha no download de $($xsd.Name). Baixe manualmente do Portal da NFe." -ForegroundColor Yellow
    }
}

# Criar XSD básico se download falhar
$leiauteBasico = Join-Path $nfeDir "leiauteNFe_v4.00.xsd"
if (-not (Test-Path $leiauteBasico)) {
    Write-Host "Criando XSD básico para desenvolvimento..." -ForegroundColor Yellow
    
    $xsdContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
           targetNamespace="http://www.portalfiscal.inf.br/nfe"
           xmlns="http://www.portalfiscal.inf.br/nfe"
           elementFormDefault="qualified">
    
    <!-- XSD básico para desenvolvimento -->
    <!-- BAIXE O XSD OFICIAL DO PORTAL DA NFE PARA PRODUÇÃO -->
    
    <xs:element name="NFe">
        <xs:complexType>
            <xs:sequence>
                <xs:element name="infNFe" type="xs:anyType"/>
                <xs:element name="Signature" type="xs:anyType" minOccurs="0"/>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
    
</xs:schema>
"@
    
    Set-Content -Path $leiauteBasico -Value $xsdContent -Encoding UTF8
    Write-Host "XSD básico criado. ATENÇÃO: Baixe o XSD oficial para produção!" -ForegroundColor Red
}

# Instruções finais
Write-Host "`n=== INSTRUÇÕES IMPORTANTES ===" -ForegroundColor Yellow
Write-Host "1. Para PRODUÇÃO, baixe os XSDs oficiais do Portal Nacional da NFe:"
Write-Host "   https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D"
Write-Host "2. Descompacte os arquivos no diretório: $nfeDir"
Write-Host "3. Verifique se todos os XSDs estão presentes antes de ir para produção"
Write-Host "4. Para NFSe, baixe os XSDs do seu município ou use o padrão nacional"

Write-Host "`n=== DOWNLOAD CONCLUÍDO ===" -ForegroundColor Green
Write-Host "Diretório de schemas: $TargetPath"