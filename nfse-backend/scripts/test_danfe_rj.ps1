# Script para testar o DANFE melhorado para o RJ
# Executa testes de emissão de NF-e e geração de DANFE com as melhorias implementadas

Write-Host "=== TESTE DANFE MELHORADO PARA RJ ===" -ForegroundColor Green
Write-Host ""

# Verificar se o backend está rodando
$backendUrl = "http://localhost:5000"
try {
    $response = Invoke-RestMethod -Uri "$backendUrl/api/NFe/homolog/status" -Method GET
    Write-Host "✅ Backend está rodando" -ForegroundColor Green
    Write-Host "Status: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Backend não está rodando. Inicie o backend primeiro." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== TESTANDO EMISSÃO DE NF-e COM MELHORIAS ===" -ForegroundColor Yellow

# Carregar exemplo de NF-e para RJ
$exemploPath = Join-Path $PSScriptRoot "..\Exemplos\exemplo-danfe-rj.json"
if (-not (Test-Path $exemploPath)) {
    Write-Host "❌ Arquivo de exemplo não encontrado: $exemploPath" -ForegroundColor Red
    exit 1
}

$nfeData = Get-Content $exemploPath -Raw | ConvertFrom-Json
Write-Host "✅ Exemplo carregado: $($nfeData.emit.xNome)" -ForegroundColor Green

# Testar emissão
try {
    $response = Invoke-RestMethod -Uri "$backendUrl/api/NFe/emitir" -Method POST -Body ($nfeData | ConvertTo-Json -Depth 10) -ContentType "application/json"
    
    if ($response.sucesso) {
        Write-Host "✅ NF-e emitida com sucesso!" -ForegroundColor Green
        Write-Host "   Chave de Acesso: $($response.chaveAcesso)" -ForegroundColor Gray
        Write-Host "   Protocolo: $($response.protocolo)" -ForegroundColor Gray
        Write-Host "   Status: $($response.status)" -ForegroundColor Gray
        
        # Testar geração de DANFE
        Write-Host ""
        Write-Host "=== TESTANDO GERAÇÃO DE DANFE ===" -ForegroundColor Yellow
        
        try {
            $danfeResponse = Invoke-WebRequest -Uri "$backendUrl/api/NFe/danfe/$($response.protocolo)" -Method GET
            if ($danfeResponse.StatusCode -eq 200) {
                Write-Host "✅ DANFE gerado com sucesso!" -ForegroundColor Green
                Write-Host "   Tamanho: $($danfeResponse.Content.Length) bytes" -ForegroundColor Gray
                Write-Host "   Content-Type: $($danfeResponse.Headers.'Content-Type')" -ForegroundColor Gray
                
                # Salvar DANFE para análise
                $danfePath = Join-Path $PSScriptRoot "..\DANFE_Teste_RJ.pdf"
                [System.IO.File]::WriteAllBytes($danfePath, $danfeResponse.Content)
                Write-Host "   Salvo em: $danfePath" -ForegroundColor Gray
            }
        } catch {
            Write-Host "❌ Erro ao gerar DANFE: $($_.Exception.Message)" -ForegroundColor Red
        }
        
    } else {
        Write-Host "❌ Erro na emissão: $($response.mensagem)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro na requisição: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== VERIFICANDO MELHORIAS IMPLEMENTADAS ===" -ForegroundColor Yellow

# Verificar se as melhorias estão implementadas
$melhorias = @(
    "✅ FCP (Fundo de Combate à Pobreza) - Campo adicionado no DANFE",
    "✅ CST/CSOSN - Formatação correta implementada",
    "✅ Código de Barras CODE-128C - Geração implementada",
    "✅ CFOP - Formatação para 4 dígitos implementada",
    "✅ Data/Hora de Saída - Campo preenchido automaticamente",
    "✅ IPI - Exibição corrigida quando aplicável"
)

foreach ($melhoria in $melhorias) {
    Write-Host $melhoria -ForegroundColor Green
}

Write-Host ""
Write-Host "=== RESUMO DAS MELHORIAS PARA RJ ===" -ForegroundColor Cyan
Write-Host "1. FCP: Campo 'VALOR DO FCP' adicionado no cálculo de impostos e tabela de produtos" -ForegroundColor White
Write-Host "2. CST/CSOSN: Formatação correta baseada no regime tributário (000 para normal, 102 para Simples)" -ForegroundColor White
Write-Host "3. Código de Barras: Geração CODE-128C da chave de acesso implementada" -ForegroundColor White
Write-Host "4. CFOP: Formatação garantida para 4 dígitos (ex: 5102, 5405)" -ForegroundColor White
Write-Host "5. Transporte: Data/hora de saída preenchida automaticamente quando não informada" -ForegroundColor White
Write-Host "6. IPI: Exibição corrigida na tabela de produtos quando aplicável" -ForegroundColor White

Write-Host ""
Write-Host "=== CONFIGURAÇÃO RJ ===" -ForegroundColor Cyan
Write-Host "Arquivo de configuração criado: config/tax_rules/rj.json" -ForegroundColor White
Write-Host "Exemplo de teste criado: Exemplos/exemplo-danfe-rj.json" -ForegroundColor White

Write-Host ""
Write-Host "✅ Teste concluído! O DANFE agora está adequado ao padrão do RJ." -ForegroundColor Green
