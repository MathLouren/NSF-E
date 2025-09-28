# Script para testar DANFE 2026 com layout corrigido
# Resolve problemas de overflow de colunas

Write-Host "🧪 Testando DANFE 2026 com layout corrigido..." -ForegroundColor Green

$baseUrl = "http://localhost:5000/api"
$endpoint = "$baseUrl/nfe2026/gerar-danfe-versao"

# Payload corrigido com estrutura adequada
$payload = @{
    Versao = "2026"
    NFe = @{
        versao = "4.00"
        chaveAcesso = "35200114200166000187550010000000015123456789"
        ide = @{
            cUF = 35
            cNF = "12345678"
            natOp = "VENDA"
            mod = 55
            serie = 1
            nNF = 1
            dhEmi = "2024-01-15T10:00:00-03:00"
            tpNF = 1
            idDest = 1
            cMunFG = 3550308
            tpImp = 1
            tpEmis = 1
            cDV = 9
            tpAmb = 2
            finNFe = 1
            indFinal = 1
            indPres = 1
            procEmi = 0
            verProc = "1.0.0"
        }
        emit = @{
            CNPJ = "14200166000187"
            xNome = "Empresa Exemplo Ltda"
            xFant = "Exemplo"
            enderEmit = @{
                xLgr = "Rua das Flores"
                nro = "123"
                xBairro = "Centro"
                cMun = 3550308
                xMun = "São Paulo"
                UF = "SP"
                CEP = "01234567"
                cPais = 1058
                xPais = "Brasil"
            }
            IE = "123456789"
            CRT = 1
        }
        dest = @{
            CNPJ = "12345678000195"
            xNome = "Cliente Exemplo Ltda"
            enderDest = @{
                xLgr = "Av. Paulista"
                nro = "1000"
                xBairro = "Bela Vista"
                cMun = 3550308
                xMun = "São Paulo"
                UF = "SP"
                CEP = "01310100"
                cPais = 1058
                xPais = "Brasil"
            }
            indIEDest = 1
            IE = "987654321"
        }
        det = @(
            @{
                nItem = 1
                prod = @{
                    cProd = "001"
                    cEAN = "7891234567890"
                    xProd = "Produto Exemplo"
                    NCM = "61091000"
                    CFOP = 5102
                    uCom = "UN"
                    qCom = 1.0000
                    vUnCom = 100.00
                    vProd = 100.00
                    cEANTrib = "7891234567890"
                    uTrib = "UN"
                    qTrib = 1.0000
                    vUnTrib = 100.00
                    indTot = 1
                }
                imposto = @{
                    ICMS = @{
                        ICMS00 = @{
                            orig = "0"
                            CST = "00"
                            modBC = 3
                            vBC = 100.00
                            pICMS = 18.00
                            vICMS = 18.00
                        }
                    }
                }
            }
        )
        total = @{
            ICMSTot = @{
                vBC = 100.00
                vICMS = 18.00
                vICMSDeson = 0.00
                vFCP = 0.00
                vBCST = 0.00
                vST = 0.00
                vFCPST = 0.00
                vFCPSTRet = 0.00
                vProd = 100.00
                vFrete = 0.00
                vSeg = 0.00
                vDesc = 0.00
                vII = 0.00
                vIPI = 0.00
                vIPIDevol = 0.00
                vPIS = 0.00
                vCOFINS = 0.00
                vOutro = 0.00
                vNF = 118.00
                vTotTrib = 18.00
            }
        }
        transp = @{
            modFrete = 0
        }
        pag = @{
            detPag = @(
                @{
                    indPag = 0
                    tPag = "01"
                    vPag = 118.00
                }
            )
        }
        infAdic = @{
            infCpl = "Informações complementares da NF-e"
        }
        gruposIBS = @(
            @{
                nItem = 1
                cst = "00"
                vBCIBS = 100.00
                pIBS = 8.5
                vIBS = 8.50
                vFCP = 0.00
                pFCP = 0.00
            }
        )
        gruposCBS = @(
            @{
                nItem = 1
                cst = "00"
                vBCCBS = 100.00
                pCBS = 1.5
                vCBS = 1.50
            }
        )
        gruposIS = @()
        totaisIBS = @{
            vBCIBS = 100.00
            vIBS = 8.50
            vFCP = 0.00
            vIBSST = 0.00
            vFCPST = 0.00
        }
        totaisCBS = @{
            vBCCBS = 100.00
            vCBS = 1.50
            vCBSST = 0.00
        }
        totaisIS = @{
            vBCIS = 0.00
            vIS = 0.00
            vISST = 0.00
        }
        rastreabilidade = @(
            @{
                nItem = 1
                GTIN = "7891234567890"
                numeroLote = "LOTE001"
                dataFabricacao = "2024-01-01"
                dataValidade = "2025-01-01"
                codigoRastreamento = "TRACK001"
            }
        )
        referencias = @(
            @{
                tipo = "NFe"
                chave = "35200114200166000187550010000000015123456788"
                numero = "1"
                serie = "1"
                dataEmissao = "2024-01-14T10:00:00-03:00"
            }
        )
    }
    IsContingencia = $false
} | ConvertTo-Json -Depth 10

Write-Host "📤 Enviando requisição para: $endpoint" -ForegroundColor Yellow
Write-Host "📋 Payload: $($payload.Length) caracteres" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method POST -Body $payload -ContentType "application/json" -TimeoutSec 60
    
    if ($response.sucesso) {
        Write-Host "✅ DANFE 2026 gerada com sucesso!" -ForegroundColor Green
        Write-Host "📄 Número de DANFEs: $($response.danfes.Count)" -ForegroundColor Green
        
        foreach ($danfe in $response.danfes) {
            Write-Host "   - Versão: $($danfe.versao)" -ForegroundColor Cyan
            Write-Host "   - Tamanho: $($danfe.tamanhoBytes) bytes" -ForegroundColor Cyan
            Write-Host "   - Arquivo: $($danfe.nomeArquivo)" -ForegroundColor Cyan
            
            # Salvar PDF para teste
            $pdfPath = "danfe_2026_test_$(Get-Date -Format 'yyyyMMdd_HHmmss').pdf"
            $pdfBytes = [System.Convert]::FromBase64String($danfe.pdfBase64)
            [System.IO.File]::WriteAllBytes($pdfPath, $pdfBytes)
            Write-Host "💾 PDF salvo em: $pdfPath" -ForegroundColor Green
        }
        
        Write-Host "🎉 Teste concluído com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro na geração da DANFE:" -ForegroundColor Red
        Write-Host "   $($response.erro)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro na requisição:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Resposta: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`n🔧 Correções aplicadas:" -ForegroundColor Yellow
Write-Host "   ✅ Larguras de colunas ajustadas para evitar overflow" -ForegroundColor White
Write-Host "   ✅ Tabela simplificada com colunas principais" -ForegroundColor White
Write-Host "   ✅ Componente de detalhes fiscais separado" -ForegroundColor White
Write-Host "   ✅ Texto truncado para evitar problemas de layout" -ForegroundColor White
