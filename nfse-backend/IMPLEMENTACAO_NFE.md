# Implementação NF-e - Sistema Nacional de Notas Fiscais Eletrônicas

## Visão Geral

Este documento descreve a implementação completa do sistema de emissão de Notas Fiscais Eletrônicas (NF-e) modelo 55 e NFS-e, seguindo as especificações oficiais da SEFAZ e padrões nacionais.

## Componentes Implementados

### 1. Estrutura de XSD e Validação XML
- **Arquivo**: `Services/Xml/XmlValidationService.cs`
- **Funcionalidade**: Valida XMLs contra esquemas XSD oficiais antes do envio
- **Status**: ✅ Implementado

### 2. Modelos de NF-e (Modelo 55)
- **Arquivo**: `Models/NFe/NFe.cs`
- **Funcionalidade**: Estrutura completa com todos os campos obrigatórios da NF-e v4.00
- **Status**: ✅ Implementado

### 3. Gerador de XML
- **Arquivo**: `Services/Xml/XmlGeneratorService.cs`
- **Funcionalidade**: Converte modelos C# em XML seguindo o padrão oficial
- **Status**: ✅ Implementado

### 4. Assinatura Digital (XML-DSig)
- **Arquivo**: `Services/Certificado/CertificadoDigitalService.cs`
- **Funcionalidade**: 
  - Suporte a certificados A1 (arquivo) e A3 (token/cartão)
  - Assinatura XML-DSig com certificado ICP-Brasil
  - Validação de certificados
- **Status**: ✅ Implementado

### 5. Cliente SOAP WebServices
- **Arquivo**: `Services/WebService/SefazWebServiceClient.cs`
- **Funcionalidade**:
  - Comunicação com WebServices da SEFAZ
  - Suporte a múltiplos estados (SP, MG, SVRS, etc.)
  - Operações: Autorização, Consulta, Eventos, Inutilização
- **Status**: ✅ Implementado

### 6. Cálculo de Impostos
- **Arquivo**: `Services/Impostos/CalculoImpostosService.cs`
- **Funcionalidade**:
  - ICMS (interno e interestadual)
  - IPI
  - PIS/COFINS (cumulativo e não-cumulativo)
  - ISS para serviços
- **Status**: ✅ Implementado

### 7. Eventos (CCe, Cancelamento, Inutilização)
- **Arquivo**: `Services/Eventos/EventosNFeService.cs`
- **Funcionalidade**:
  - Carta de Correção Eletrônica (CCe)
  - Cancelamento de NF-e
  - Inutilização de numeração
- **Status**: ✅ Implementado

### 8. Geração de DANFE (PDF)
- **Arquivo**: `Services/Pdf/DanfeService.cs`
- **Funcionalidade**:
  - Layout oficial do DANFE
  - Código de barras da chave de acesso
  - Suporte a contingência
- **Status**: ✅ Implementado

### 9. Configuração de Ambientes
- **Arquivo**: `Services/Configuracao/ConfiguracaoNFeService.cs`
- **Funcionalidade**:
  - Alternância entre homologação e produção
  - Configurações por empresa
  - URLs de WebServices por estado
- **Status**: ✅ Implementado

### 10. Armazenamento Seguro
- **Arquivo**: `Services/Armazenamento/ArmazenamentoSeguroService.cs`
- **Funcionalidade**:
  - Criptografia de certificados
  - Organização de XMLs e PDFs
  - Backup automático
  - Limpeza de arquivos antigos
- **Status**: ✅ Implementado

## API Endpoints

### NF-e Controller (`/api/nfe`)

#### 1. Emitir NF-e
```http
POST /api/nfe/emitir
Content-Type: application/json
Authorization: Bearer {token}

{
  "ide": {
    "natOp": "VENDA DE MERCADORIA",
    "tpNF": 1,
    "idDest": 1,
    "cMunFG": 3550308,
    "finNFe": 1,
    "indFinal": 1,
    "indPres": 1
  },
  "emit": {
    "cnpj": "00000000000191",
    "xNome": "Empresa Exemplo Ltda",
    "xFant": "Empresa Exemplo",
    "enderEmit": {
      "xLgr": "Rua Exemplo",
      "nro": "100",
      "xBairro": "Centro",
      "cMun": 3550308,
      "xMun": "São Paulo",
      "uf": "SP",
      "cep": "01000000"
    },
    "ie": "000000000000",
    "crt": 3
  },
  "dest": {
    "cnpj": "00000000000272",
    "xNome": "Cliente Exemplo",
    "enderDest": {
      "xLgr": "Rua Cliente",
      "nro": "200",
      "xBairro": "Bairro",
      "cMun": 3550308,
      "xMun": "São Paulo",
      "uf": "SP",
      "cep": "02000000"
    },
    "indIEDest": 9
  },
  "det": [
    {
      "nItem": 1,
      "prod": {
        "cProd": "123",
        "cEAN": "SEM GTIN",
        "xProd": "Produto Exemplo",
        "ncm": "61091000",
        "cfop": 5102,
        "uCom": "UN",
        "qCom": 1,
        "vUnCom": 100.00,
        "vProd": 100.00,
        "cEANTrib": "SEM GTIN",
        "uTrib": "UN",
        "qTrib": 1,
        "vUnTrib": 100.00,
        "indTot": 1
      }
    }
  ],
  "transp": {
    "modFrete": 9
  },
  "pag": {
    "detPag": [
      {
        "tPag": 1,
        "vPag": 100.00
      }
    ]
  }
}
```

#### 2. Carta de Correção
```http
POST /api/nfe/carta-correcao
Content-Type: application/json
Authorization: Bearer {token}

{
  "chaveAcesso": "35190800000000000191550010000000151000000158",
  "cnpjEmitente": "00000000000191",
  "textoCorrecao": "Correção no endereço de entrega: Rua Correta, 300",
  "sequenciaEvento": 1
}
```

#### 3. Cancelar NF-e
```http
POST /api/nfe/cancelar
Content-Type: application/json
Authorization: Bearer {token}

{
  "chaveAcesso": "35190800000000000191550010000000151000000158",
  "protocolo": "135190000000001",
  "cnpjEmitente": "00000000000191",
  "justificativa": "Erro na emissão - dados incorretos do destinatário"
}
```

#### 4. Inutilizar Numeração
```http
POST /api/nfe/inutilizar
Content-Type: application/json
Authorization: Bearer {token}

{
  "cnpjEmitente": "00000000000191",
  "uf": "SP",
  "ano": 2025,
  "serie": 1,
  "numeroInicial": 100,
  "numeroFinal": 110,
  "justificativa": "Numeração pulada por erro no sistema"
}
```

#### 5. Upload de Certificado Digital
```http
POST /api/nfe/certificado/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

certificado: [arquivo .pfx]
senha: "senha_do_certificado"
cnpjEmpresa: "00000000000191"
```

#### 6. Consultar Ambiente
```http
GET /api/nfe/ambiente
Authorization: Bearer {token}
```

#### 7. Alterar Ambiente
```http
POST /api/nfe/ambiente
Content-Type: application/json
Authorization: Bearer {token}

{
  "producao": false
}
```

#### 8. Checklist de Homologação
```http
GET /api/nfe/homolog/status
Authorization: Bearer {token}
```
Retorna flags sobre presença de XSDs, endpoints e tax rules.

## Configuração Inicial

### 1. Certificado Digital

O sistema requer um certificado digital ICP-Brasil (A1 ou A3) válido:

1. Faça upload do certificado através da API
2. Ou coloque o arquivo .pfx no diretório configurado
3. Configure a senha no appsettings.json ou variável de ambiente

Também é possível carregar o certificado via API `POST /api/nfe/certificado/upload` (arquivo .pfx e senha), que será armazenado de forma criptografada em `NFeStorage/Certificados/<CNPJ>/`.

### 2. Esquemas XSD

Baixe os XSDs oficiais e coloque na pasta `Schemas/`:

```bash
Schemas/
├── NFe/
│   └── v4.00/
│       └── [arquivos XSD da NF-e]
└── NFSe/
    └── [arquivos XSD da NFS-e]
```

Script auxiliar (Windows PowerShell) para baixar XSDs da NF-e v4.00:

```powershell
$ErrorActionPreference = 'Stop'
$dest = "Schemas/NFe/v4.00"
New-Item -ItemType Directory -Force -Path $dest | Out-Null
# ATENÇÃO: As URLs reais dos XSDs variam por pacote. Baixe manualmente do portal e extraia aqui.
Write-Host "Baixe manualmente o pacote de esquemas da NF-e no portal e extraia em $dest" -ForegroundColor Yellow
```

### 3. Configurações no appsettings.json

```json
{
  "NFe": {
    "Ambiente": "HOMOLOGACAO",
    "Empresas": {
      "00000000000191": {
        "CertificadoPath": "path/to/certificado.pfx",
        "CertificadoSenha": "senha",
        "SerieNFe": 1,
        "ProximoNumeroNFe": 1,
        "CRT": 3,
        "RegimeTributario": "LUCRO_PRESUMIDO"
      }
    }
  }
}
```

## Fluxo de Emissão

1. **Preparação dos Dados**: Preencher o modelo NFe com todos os dados
2. **Cálculo de Impostos**: Sistema calcula automaticamente ICMS, IPI, PIS, COFINS
3. **Geração do XML**: Conversão do modelo para XML
4. **Validação XSD**: Validação contra esquemas oficiais
5. **Assinatura Digital**: Assinatura com certificado ICP-Brasil
6. **Envio para SEFAZ**: Transmissão via WebService SOAP
7. **Processamento**: Aguardar e consultar autorização
8. **Armazenamento**: Salvar XML autorizado e gerar DANFE

## Observações Importantes

1. **Ambiente de Homologação**: Sempre teste em homologação antes de produção
2. **Dados de Teste**: Em homologação, use:
   - Razão Social: "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
   - CNPJ: Use CNPJs de teste fornecidos pela SEFAZ

3. **Numeração**: O sistema controla automaticamente a numeração sequencial
4. **Contingência**: Implementar plano de contingência para indisponibilidade
5. **Armazenamento**: XMLs devem ser guardados por no mínimo 5 anos

## Códigos de Retorno Comuns

- **100**: Autorizado o uso da NF-e
- **101**: Cancelamento homologado
- **102**: Inutilização homologada
- **135**: Evento registrado (CCe)
- **204**: Duplicidade de NF-e
- **301**: Irregularidade fiscal do emitente
- **302**: Irregularidade fiscal do destinatário

## Suporte e Manutenção

- Manter XSDs atualizados
- Acompanhar Notas Técnicas da SEFAZ
- Realizar backups regulares dos XMLs
- Monitorar validade do certificado digital
- Verificar logs de erro regularmente

## Referências

- Portal Nacional NF-e: https://www.nfe.fazenda.gov.br/portal/
- Manual de Integração: https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=d%2FNN9SSgick%3D
- ABRASF (NFS-e): https://abrasf.org.br/
