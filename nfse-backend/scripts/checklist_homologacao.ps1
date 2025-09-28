# Checklist de Homologação NFe - Execução Automatizada
# Baseado no Manual de Integração do Contribuinte (MOC)

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$CnpjTeste = "12345678000199"
)

Write-Host "=== CHECKLIST DE HOMOLOGAÇÃO NFe ===" -ForegroundColor Green
Write-Host "URL Base: $BaseUrl" -ForegroundColor Cyan
Write-Host "CNPJ Teste: $CnpjTeste" -ForegroundColor Cyan

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

$resultados = @()

function Test-Endpoint {
    param($Nome, $Method, $Url, $Body = $null)
    
    try {
        Write-Host "`n--- $Nome ---" -ForegroundColor Yellow
        
        $params = @{
            Uri = "$BaseUrl$Url"
            Method = $Method
            Headers = $headers
        }
        
        if ($Body) {
            $params.Body = $Body | ConvertTo-Json -Depth 10
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "✅ SUCESSO: $Nome" -ForegroundColor Green
        
        $script:resultados += @{
            Teste = $Nome
            Status = "SUCESSO"
            Detalhes = $response
        }
        
        return $response
    }
    catch {
        Write-Host "❌ ERRO: $Nome - $($_.Exception.Message)" -ForegroundColor Red
        
        $script:resultados += @{
            Teste = $Nome
            Status = "ERRO"
            Erro = $_.Exception.Message
        }
        
        return $null
    }
}

# 1. Verificar status do serviço
Write-Host "`n=== 1. VERIFICAÇÕES BÁSICAS ===" -ForegroundColor Magenta
Test-Endpoint "Status do Certificado" "GET" "/api/nfe/certificado/status?cnpj=$CnpjTeste"
Test-Endpoint "Ambiente Atual" "GET" "/api/nfe/ambiente"
Test-Endpoint "Status de Homologação" "GET" "/api/nfe/homolog/status"

# 2. Emissão de NFe (Caso de Sucesso)
Write-Host "`n=== 2. EMISSÃO DE NFe - CASO DE SUCESSO ===" -ForegroundColor Magenta

$nfeValida = @{
    ide = @{
        natOp = "VENDA DE MERCADORIA - HOMOLOGACAO"
        mod = 55
        serie = 1
        nNF = 1
        dhEmi = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        tpNF = 1
        idDest = 1
        cMunFG = 3304557
        tpImp = 1
        tpEmis = 1
        tpAmb = 2
        finNFe = 1
        indFinal = 1
        indPres = 1
        procEmi = 0
        verProc = "HOMOLOG_1.0"
    }
    emit = @{
        CNPJ = $CnpjTeste
        xNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
        xFant = "EMPRESA TESTE HOMOLOG"
        enderEmit = @{
            xLgr = "RUA TESTE HOMOLOGACAO"
            nro = "123"
            xBairro = "CENTRO"
            cMun = 3304557
            xMun = "Rio de Janeiro"
            UF = "RJ"
            CEP = "20000000"
            cPais = 1058
            xPais = "BRASIL"
        }
        IE = "123456789"
        CRT = 3
    }
    dest = @{
        CNPJ = "98765432000188"
        xNome = "CLIENTE TESTE HOMOLOGACAO"
        enderDest = @{
            xLgr = "AVENIDA TESTE"
            nro = "456"
            xBairro = "TESTE"
            cMun = 3304557
            xMun = "Rio de Janeiro"
            UF = "RJ"
            CEP = "22000000"
            cPais = 1058
            xPais = "BRASIL"
        }
        indIEDest = 1
        IE = "987654321"
    }
    det = @(
        @{
            nItem = 1
            prod = @{
                cProd = "HOMOLOG001"
                cEAN = "SEM GTIN"
                xProd = "PRODUTO TESTE HOMOLOGACAO NFe"
                NCM = "61091000"
                CFOP = 5102
                uCom = "UN"
                qCom = 1.0000
                vUnCom = 100.0000
                vProd = 100.00
                cEANTrib = "SEM GTIN"
                uTrib = "UN"
                qTrib = 1.0000
                vUnTrib = 100.0000
                indTot = 1
            }
            imposto = @{
                ICMS = @{
                    ICMS00 = @{
                        orig = "0"
                        CST = "00"
                        modBC = 3
                        vBC = 100.00
                        pICMS = 20.00
                        vICMS = 20.00
                        pFCP = 2.00
                        vFCP = 2.00
                    }
                }
            }
        }
    )
    total = @{
        ICMSTot = @{
            vBC = 100.00
            vICMS = 20.00
            vFCP = 2.00
            vProd = 100.00
            vNF = 100.00
        }
    }
    transp = @{
        modFrete = 9
    }
    pag = @{
        detPag = @(
            @{
                indPag = 0
                tPag = 1
                vPag = 100.00
            }
        )
    }
    infAdic = @{
        infCpl = "NFe emitida em ambiente de homologacao para testes - SEM VALOR FISCAL"
    }
}

$emissaoResult = Test-Endpoint "Emissão NFe Válida" "POST" "/api/nfe/emitir" $nfeValida

if ($emissaoResult -and $emissaoResult.protocolo) {
    $protocolo = $emissaoResult.protocolo
    
    # 3. Gerar DANFE
    Write-Host "`n=== 3. GERAÇÃO DE DANFE ===" -ForegroundColor Magenta
    
    try {
        $danfeUrl = "$BaseUrl/api/nfe/danfe/$protocolo"
        Write-Host "Baixando DANFE de: $danfeUrl" -ForegroundColor Cyan
        
        $danfeResponse = Invoke-WebRequest -Uri $danfeUrl -Method GET
        if ($danfeResponse.StatusCode -eq 200) {
            $danfePath = ".\DANFE_HOMOLOG_$protocolo.pdf"
            [System.IO.File]::WriteAllBytes($danfePath, $danfeResponse.Content)
            Write-Host "✅ DANFE gerado: $danfePath" -ForegroundColor Green
            
            $script:resultados += @{
                Teste = "Geração DANFE"
                Status = "SUCESSO"
                Arquivo = $danfePath
            }
        }
    }
    catch {
        Write-Host "❌ ERRO na geração do DANFE: $($_.Exception.Message)" -ForegroundColor Red
        
        $script:resultados += @{
            Teste = "Geração DANFE"
            Status = "ERRO"
            Erro = $_.Exception.Message
        }
    }
    
    # 4. Consultar NFe
    Write-Host "`n=== 4. CONSULTA DE NFe ===" -ForegroundColor Magenta
    if ($emissaoResult.chaveAcesso) {
        Test-Endpoint "Consulta por Chave" "GET" "/api/nfe/consulta/$($emissaoResult.chaveAcesso)"
    }
}

# 5. Testes de Rejeição (dados inválidos)
Write-Host "`n=== 5. TESTES DE REJEIÇÃO ===" -ForegroundColor Magenta

$nfeInvalida = @{
    ide = @{
        natOp = "" # Natureza vazia deve causar rejeição
        mod = 55
        serie = 1
    }
    emit = @{
        CNPJ = "00000000000000" # CNPJ inválido
        xNome = ""
    }
}

Test-Endpoint "NFe com Dados Inválidos (deve rejeitar)" "POST" "/api/nfe/emitir" $nfeInvalida

# 6. Carta de Correção
Write-Host "`n=== 6. CARTA DE CORREÇÃO ===" -ForegroundColor Magenta

if ($emissaoResult -and $emissaoResult.chaveAcesso) {
    $cartaCorrecao = @{
        chaveAcesso = $emissaoResult.chaveAcesso
        cnpjEmitente = $CnpjTeste
        textoCorrecao = "Teste de carta de correcao em ambiente de homologacao"
        sequenciaEvento = 1
    }
    
    Test-Endpoint "Carta de Correção" "POST" "/api/nfe/carta-correcao" $cartaCorrecao
}

# 7. Inutilização
Write-Host "`n=== 7. INUTILIZAÇÃO ===" -ForegroundColor Magenta

$inutilizacao = @{
    cnpjEmitente = $CnpjTeste
    uf = "RJ"
    ano = (Get-Date).Year
    serie = 1
    numeroInicial = 999
    numeroFinal = 999
    justificativa = "Teste de inutilizacao em ambiente de homologacao"
}

Test-Endpoint "Inutilização de Numeração" "POST" "/api/nfe/inutilizar" $inutilizacao

# 8. Listar NFe
Write-Host "`n=== 8. LISTAGEM ===" -ForegroundColor Magenta
Test-Endpoint "Listar NFe" "GET" "/api/nfe/lista?cnpj=$CnpjTeste"

# Relatório Final
Write-Host "`n=== RELATÓRIO FINAL ===" -ForegroundColor Green

$sucessos = ($resultados | Where-Object { $_.Status -eq "SUCESSO" }).Count
$erros = ($resultados | Where-Object { $_.Status -eq "ERRO" }).Count
$total = $resultados.Count

Write-Host "Total de Testes: $total" -ForegroundColor Cyan
Write-Host "Sucessos: $sucessos" -ForegroundColor Green
Write-Host "Erros: $erros" -ForegroundColor Red

if ($erros -eq 0) {
    Write-Host "`n🎉 TODOS OS TESTES PASSARAM! Sistema pronto para homologação." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Alguns testes falharam. Verifique os erros acima." -ForegroundColor Yellow
}

# Salvar relatório
$relatorio = @{
    DataExecucao = Get-Date
    BaseUrl = $BaseUrl
    CnpjTeste = $CnpjTeste
    Resultados = $resultados
    Resumo = @{
        Total = $total
        Sucessos = $sucessos
        Erros = $erros
    }
}

$relatorioPath = ".\relatorio_homologacao_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
$relatorio | ConvertTo-Json -Depth 10 | Set-Content -Path $relatorioPath -Encoding UTF8

Write-Host "`nRelatório salvo em: $relatorioPath" -ForegroundColor Cyan
