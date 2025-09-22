# Esquemas XSD para NF-e e NFS-e

## Estrutura de Diretórios

```
Schemas/
├── NFe/
│   └── v4.00/
│       ├── nfe_v4.00.xsd
│       ├── leiauteNFe_v4.00.xsd
│       └── ... (outros XSDs)
└── NFSe/
    ├── ABRASF/
    │   └── nfse.xsd
    └── Nacional/
        └── nfse_v1.00.xsd
```

## Como obter os XSDs

### NF-e (Nota Fiscal Eletrônica de Produtos)

1. Acesse o Portal Nacional da NF-e:
   https://www.nfe.fazenda.gov.br/portal/

2. Vá para a seção de esquemas XML:
   https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D

3. Baixe o pacote de liberação mais recente (PL_009_V4)

4. Extraia os arquivos XSD na pasta `NFe/v4.00/`

Script auxiliar (Windows PowerShell):
```powershell
$ErrorActionPreference = 'Stop'
$dest = "Schemas/NFe/v4.00"
New-Item -ItemType Directory -Force -Path $dest | Out-Null
Write-Host "Baixe o pacote oficial no portal e extraia em $dest" -ForegroundColor Yellow
```

### NFS-e (Nota Fiscal de Serviços Eletrônica)

1. Para o padrão ABRASF:
   - Acesse: https://abrasf.org.br/biblioteca/arquivos-publicos/nfs-e
   - Baixe os XSDs do padrão ABRASF

2. Para o padrão Nacional:
   - Acesse: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/
   - Baixe o pacote: xsd_pl_nfse_1-00-producao.zip

## Observações Importantes

- Os arquivos XSD são essenciais para validação dos XMLs antes do envio
- Mantenha os XSDs atualizados conforme novas versões são liberadas
- Cada município pode ter variações no padrão NFS-e
- Sempre consulte a documentação oficial da SEFAZ para atualizações
