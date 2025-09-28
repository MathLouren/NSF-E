# Script simples para testar DANFE 2026
Write-Host "🧪 Testando DANFE 2026..." -ForegroundColor Green

$body = @{
    Versao = "2026"
    NFe = @{
        versao = "4.00"
        chaveAcesso = "35200114200166000187550010000000015123456789"
        ide = @{
            cUF = 35
            natOp = "VENDA"
            mod = 55
            serie = 1
            nNF = 1
            dhEmi = "2024-01-15T10:00:00-03:00"
        }
        emit = @{
            CNPJ = "14200166000187"
            xNome = "Empresa Exemplo Ltda"
        }
        dest = @{
            CNPJ = "12345678000195"
            xNome = "Cliente Exemplo Ltda"
        }
        det = @(
            @{
                nItem = 1
                prod = @{
                    cProd = "001"
                    xProd = "Produto Exemplo"
                    NCM = "61091000"
                    CFOP = 5102
                    uCom = "UN"
                    qCom = 1.0
                    vUnCom = 100.0
                    vProd = 100.0
                }
                imposto = @{
                    ICMS = @{
                        ICMS00 = @{
                            orig = "0"
                            CST = "00"
                            vBC = 100.0
                            pICMS = 18.0
                            vICMS = 18.0
                        }
                    }
                }
            }
        )
        total = @{
            ICMSTot = @{
                vBC = 100.0
                vICMS = 18.0
                vProd = 100.0
                vNF = 118.0
            }
        }
        gruposIBS = @(
            @{
                nItem = 1
                cst = "00"
                vBCIBS = 100.0
                pIBS = 8.5
                vIBS = 8.5
            }
        )
        gruposCBS = @(
            @{
                nItem = 1
                cst = "00"
                vBCCBS = 100.0
                pCBS = 1.5
                vCBS = 1.5
            }
        )
        gruposIS = @()
        totaisIBS = @{
            vBCIBS = 100.0
            vIBS = 8.5
        }
        totaisCBS = @{
            vBCCBS = 100.0
            vCBS = 1.5
        }
        totaisIS = @{
            vBCIS = 0.0
            vIS = 0.0
        }
    }
    IsContingencia = $false
} | ConvertTo-Json -Depth 10

Write-Host "📤 Enviando requisição..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/nfe2026/gerar-danfe-versao" -Method POST -Body $body -ContentType "application/json"
    
    if ($response.sucesso) {
        Write-Host "✅ Sucesso! DANFE gerada com $($response.danfes.Count) arquivo(s)" -ForegroundColor Green
        Write-Host "📄 Versão: $($response.danfes[0].versao)" -ForegroundColor Cyan
        Write-Host "📊 Tamanho: $($response.danfes[0].tamanhoBytes) bytes" -ForegroundColor Cyan
        
        # Salvar PDF para teste
        $pdfPath = "danfe_2026_test_$(Get-Date -Format 'yyyyMMdd_HHmmss').pdf"
        $pdfBytes = [System.Convert]::FromBase64String($response.danfes[0].pdfBase64)
        [System.IO.File]::WriteAllBytes($pdfPath, $pdfBytes)
        Write-Host "💾 PDF salvo em: $pdfPath" -ForegroundColor Green
        
        Write-Host "🎉 Teste concluído com sucesso!" -ForegroundColor Green
    } 
    else {
        Write-Host "❌ Erro na geração da DANFE:" -ForegroundColor Red
        Write-Host "   $($response.erro)" -ForegroundColor Red
    }
} 
catch {
    Write-Host "❌ Erro na requisição:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Resposta: $responseBody" -ForegroundColor Red
    }
}
