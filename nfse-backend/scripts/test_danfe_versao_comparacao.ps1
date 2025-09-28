# Script para testar geração de DANFE com seleção de versão
# Permite escolher entre versão atual, 2026, ou ambas para comparação

Write-Host "=== TESTE DANFE - SELEÇÃO DE VERSÃO ===" -ForegroundColor Green
Write-Host "Testando geração de DANFE com opções de versão..." -ForegroundColor Yellow

# Verificar se a aplicação está rodando
$response = try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/nfe2026/versao-layout" -Method Get -TimeoutSec 5
} catch {
    Write-Host "ERRO: Aplicação não está rodando. Execute 'dotnet run' primeiro." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Aplicação está rodando" -ForegroundColor Green

# Ler o arquivo de exemplo
$exemploPath = "C:\Users\user\Desktop\nfse\nfse-backend\Exemplos\exemplo-nfe-2026-minima.json"
if (-not (Test-Path $exemploPath)) {
    Write-Host "ERRO: Arquivo de exemplo não encontrado: $exemploPath" -ForegroundColor Red
    exit 1
}

$nfeData = Get-Content $exemploPath -Raw | ConvertFrom-Json
Write-Host "✅ Dados de exemplo carregados" -ForegroundColor Green

# Função para testar uma versão específica
function Test-DanfeVersao {
    param(
        [string]$versao,
        [string]$descricao
    )
    
    Write-Host "`n--- Testando $descricao ---" -ForegroundColor Cyan
    
    $requestData = @{
        NFe = $nfeData
        Versao = $versao
        IsContingencia = $false
    }
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/nfe2026/gerar-danfe-versao" -Method Post -Body ($requestData | ConvertTo-Json -Depth 10) -ContentType "application/json"
        
        if ($response.sucesso -eq $true) {
            Write-Host "✅ $descricao gerada com sucesso!" -ForegroundColor Green
            Write-Host "   Versão solicitada: $($response.versaoSolicitada)" -ForegroundColor White
            Write-Host "   Quantidade de DANFEs: $($response.danfes.Count)" -ForegroundColor White
            
            foreach ($danfe in $response.danfes) {
                Write-Host "   📄 $($danfe.versao) - Layout: $($danfe.layout)" -ForegroundColor Yellow
                Write-Host "      Arquivo: $($danfe.nomeArquivo)" -ForegroundColor White
                Write-Host "      Tamanho: $($danfe.tamanhoBytes) bytes" -ForegroundColor White
                Write-Host "      Campos incluídos:" -ForegroundColor White
                
                foreach ($campo in $danfe.camposIncluidos) {
                    Write-Host "        - $campo" -ForegroundColor Gray
                }
                
                # Salvar o arquivo PDF
                $pdfBytes = [System.Convert]::FromBase64String($danfe.danfeBase64)
                $nomeArquivo = $danfe.nomeArquivo
                [System.IO.File]::WriteAllBytes($nomeArquivo, $pdfBytes)
                Write-Host "      💾 Arquivo salvo: $nomeArquivo" -ForegroundColor Green
            }
            
            # Se for "ambas", mostrar comparação
            if ($versao -eq "ambas" -and $response.comparacao) {
                Write-Host "`n   📊 COMPARAÇÃO DAS VERSÕES:" -ForegroundColor Magenta
                Write-Host "   Diferenças principais:" -ForegroundColor White
                foreach ($diff in $response.comparacao.diferencasPrincipais) {
                    Write-Host "     • $diff" -ForegroundColor Yellow
                }
                
                Write-Host "`n   Novos campos na versão 2026:" -ForegroundColor White
                foreach ($campo in $response.comparacao.camposNovos) {
                    Write-Host "     + $campo" -ForegroundColor Green
                }
                
                Write-Host "`n   Campos mantidos:" -ForegroundColor White
                foreach ($campo in $response.comparacao.camposMantidos) {
                    Write-Host "     = $campo" -ForegroundColor Blue
                }
            }
            
            return $true
        } else {
            Write-Host "❌ Erro na resposta da API" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Erro ao gerar $descricao : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Testar versão atual
$sucessoAtual = Test-DanfeVersao -versao "atual" -descricao "DANFE Versão Atual (4.00)"

# Testar versão 2026
$sucesso2026 = Test-DanfeVersao -versao "2026" -descricao "DANFE Versão 2026 (Reforma Tributária)"

# Testar ambas as versões para comparação
$sucessoAmbas = Test-DanfeVersao -versao "ambas" -descricao "DANFE Ambas as Versões (Comparação)"

# Resumo dos testes
Write-Host "`n=== RESUMO DOS TESTES ===" -ForegroundColor Green
Write-Host "✅ Versão Atual: $(if($sucessoAtual) {'Sucesso'} else {'Falhou'})" -ForegroundColor $(if($sucessoAtual) {'Green'} else {'Red'})
Write-Host "✅ Versão 2026: $(if($sucesso2026) {'Sucesso'} else {'Falhou'})" -ForegroundColor $(if($sucesso2026) {'Green'} else {'Red'})
Write-Host "✅ Comparação: $(if($sucessoAmbas) {'Sucesso'} else {'Falhou'})" -ForegroundColor $(if($sucessoAmbas) {'Green'} else {'Red'})

if ($sucessoAtual -and $sucesso2026 -and $sucessoAmbas) {
    Write-Host "`n🎉 TODOS OS TESTES PASSARAM!" -ForegroundColor Green
    Write-Host "📁 Arquivos PDF gerados:" -ForegroundColor Cyan
    Get-ChildItem -Path "." -Filter "DANFE_*.pdf" | ForEach-Object {
        Write-Host "   📄 $($_.Name) ($($_.Length) bytes)" -ForegroundColor White
    }
    
    Write-Host "`n💡 DIFERENÇAS PRINCIPAIS ENTRE AS VERSÕES:" -ForegroundColor Magenta
    Write-Host "   🔵 Versão Atual (4.00):" -ForegroundColor Blue
    Write-Host "      - ICMS tradicional" -ForegroundColor White
    Write-Host "      - IPI, PIS, COFINS" -ForegroundColor White
    Write-Host "      - Layout básico" -ForegroundColor White
    
    Write-Host "`n   🟢 Versão 2026 (Reforma Tributária):" -ForegroundColor Green
    Write-Host "      - IBS (Imposto sobre Bens e Serviços)" -ForegroundColor White
    Write-Host "      - CBS (Contribuição sobre Bens e Serviços)" -ForegroundColor White
    Write-Host "      - IS (Imposto Seletivo)" -ForegroundColor White
    Write-Host "      - Rastreabilidade (GTIN)" -ForegroundColor White
    Write-Host "      - Totais por UF/Município" -ForegroundColor White
    Write-Host "      - Referências de documentos" -ForegroundColor White
    Write-Host "      - Layout visual aprimorado" -ForegroundColor White
} else {
    Write-Host "`n❌ Alguns testes falharam. Verifique os erros acima." -ForegroundColor Red
}

Write-Host "`n📋 INSTRUÇÕES DE USO:" -ForegroundColor Cyan
Write-Host "   1. Use 'atual' para gerar apenas a DANFE tradicional" -ForegroundColor White
Write-Host "   2. Use '2026' para gerar apenas a DANFE da reforma tributária" -ForegroundColor White
Write-Host "   3. Use 'ambas' para gerar as duas versões e comparar diferenças" -ForegroundColor White
Write-Host "   4. Os arquivos PDF são salvos automaticamente no diretório atual" -ForegroundColor White
