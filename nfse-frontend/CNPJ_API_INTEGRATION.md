# Integração com API de Consulta de CNPJ

## Visão Geral

Este documento descreve a implementação da funcionalidade de consulta automática de dados de empresa via CNPJ no sistema NFSe. A funcionalidade permite preencher automaticamente os campos de dados da empresa quando um CNPJ válido é inserido.

## Componentes Implementados

### 1. Serviço de Consulta de CNPJ (`cnpjService.js`)

**Localização:** `src/services/cnpjService.js`

**Funcionalidades:**
- Consulta dados de empresa via API da ReceitaWS
- Validação de CNPJ
- Formatação de CNPJ com máscara
- Mapeamento de dados da API para o formato do sistema

**Métodos principais:**
- `consultarCnpj(cnpj)`: Consulta dados da empresa
- `validarCnpj(cnpj)`: Valida se o CNPJ é válido
- `formatarCnpj(cnpj)`: Aplica máscara ao CNPJ
- `mapearDadosEmpresa(dadosApi)`: Mapeia dados da API

### 2. Componente de Input de CNPJ (`CnpjInput.vue`)

**Localização:** `src/components/CnpjInput.vue`

**Funcionalidades:**
- Input com máscara automática de CNPJ
- Validação em tempo real
- Botão de consulta automática
- Indicador de carregamento
- Mensagens de erro e sucesso
- Consulta automática ao sair do campo (blur)

**Props:**
- `label`: Rótulo do campo
- `modelValue`: Valor do CNPJ
- `placeholder`: Texto de placeholder
- `showSearchButton`: Mostrar botão de consulta (padrão: true)
- `autoConsult`: Consultar automaticamente ao sair do campo (padrão: true)

**Eventos:**
- `@update:modelValue`: Emitido quando o valor muda
- `@cnpj-consulted`: Emitido quando dados são consultados com sucesso

## Páginas Integradas

### 1. Configurações da Empresa
**Arquivo:** `src/pages/Configuracoes.vue`

**Campos preenchidos automaticamente:**
- Razão Social
- Nome Fantasia
- Logradouro
- Número
- Complemento
- Bairro
- Município
- UF
- CEP
- Código do Município
- Telefone
- Email
- Natureza Jurídica
- Porte da Empresa
- CNAE Fiscal
- Descrição CNAE

### 2. Emissão de NF-e
**Arquivo:** `src/pages/Nfe/Emitir.vue`

**Seções com preenchimento automático:**
- **Dados do Emitente:** Todos os campos de endereço e dados da empresa
- **Dados do Destinatário:** Todos os campos de endereço e dados da empresa

### 3. Emissão de NFS-e
**Arquivo:** `src/pages/Nfse/Nova.vue`

**Seções com preenchimento automático:**
- **Dados do Prestador:** Todos os campos de endereço e dados da empresa
- **Dados do Tomador:** Todos os campos de endereço e dados da empresa

## Como Usar

### 1. Uso Básico do Componente

```vue
<template>
  <CnpjInput 
    label="CNPJ da Empresa" 
    v-model="cnpj" 
    @cnpj-consulted="preencherDados"
  />
</template>

<script setup>
import CnpjInput from '@/components/CnpjInput.vue'

const cnpj = ref('')

const preencherDados = (dadosEmpresa) => {
  // Preencher campos com os dados retornados
  console.log('Dados da empresa:', dadosEmpresa)
}
</script>
```

### 2. Exemplo de Preenchimento de Dados

```javascript
const preencherDadosEmpresa = (dadosEmpresa) => {
  // Mapear dados da API para os campos do formulário
  settings.value.legalName = dadosEmpresa.razaoSocial || ''
  settings.value.nomeFantasia = dadosEmpresa.nomeFantasia || ''
  settings.value.logradouro = dadosEmpresa.logradouro || ''
  settings.value.numero = dadosEmpresa.numero || ''
  settings.value.bairro = dadosEmpresa.bairro || ''
  settings.value.municipio = dadosEmpresa.municipio || ''
  settings.value.uf = dadosEmpresa.uf || ''
  settings.value.cep = dadosEmpresa.cep || ''
  settings.value.codigoMunicipio = dadosEmpresa.codigoMunicipio || ''
  settings.value.telefone = dadosEmpresa.telefone || ''
  settings.value.email = dadosEmpresa.email || ''
  settings.value.naturezaJuridica = dadosEmpresa.naturezaJuridica || ''
  settings.value.porte = dadosEmpresa.porte || ''
  settings.value.cnaeFiscal = dadosEmpresa.cnaeFiscal || ''
  settings.value.cnaeFiscalDescricao = dadosEmpresa.cnaeFiscalDescricao || ''
}
```

## Estrutura dos Dados Retornados

A API retorna os seguintes dados mapeados:

```javascript
{
  cnpj: "92754738027109",
  razaoSocial: "LOJAS RENNER S.A.",
  nomeFantasia: "",
  logradouro: "DA LAMA PRETA",
  numero: "02805",
  complemento: " PARTE",
  bairro: "SANTA CRUZ",
  municipio: "RIO DE JANEIRO",
  uf: "RJ",
  cep: "23575450",
  telefone: "5121217384",
  email: "",
  situacaoCadastral: "ATIVA",
  dataInicioAtividade: "2013-12-16",
  capitalSocial: 9540891000,
  porte: "DEMAIS",
  naturezaJuridica: "Sociedade Anônima Aberta",
  cnaeFiscal: "4781400",
  cnaeFiscalDescricao: "Comércio varejista de artigos do vestuário e acessórios",
  codigoMunicipio: "3304557",
  regimeTributario: [],
  socios: [...]
}
```

## Tratamento de Erros

O sistema trata os seguintes tipos de erro:

1. **CNPJ inválido:** Validação local antes da consulta
2. **Erro de rede:** Timeout de 10 segundos
3. **CNPJ não encontrado:** Mensagem da API
4. **Erro de servidor:** Tratamento genérico

## Limitações e Considerações

1. **API Externa:** Dependência da API da ReceitaWS
2. **Rate Limiting:** A API pode ter limitações de requisições
3. **Dados Atualizados:** Os dados podem não estar sempre atualizados
4. **Timeout:** Configurado para 10 segundos
5. **Validação:** Sempre validar os dados antes de usar em produção

## Configurações

### Timeout da API
```javascript
// Em cnpjService.js
const response = await axios.get(`${CNPJ_API_BASE_URL}/${cnpjLimpo}`, {
  timeout: 10000 // 10 segundos
})
```

### URL da API
```javascript
// Em cnpjService.js
const CNPJ_API_BASE_URL = 'https://www.receitaws.com.br/v1/cnpj'
```

## Testes

Para testar a funcionalidade:

1. Acesse qualquer página com formulário de empresa
2. Digite um CNPJ válido (ex: 11.222.333/0001-81)
3. Aguarde a consulta automática ou clique no botão de busca
4. Verifique se os campos são preenchidos automaticamente

## Manutenção

### Atualizações da API
- Monitorar mudanças na API da ReceitaWS
- Atualizar mapeamento de dados se necessário
- Testar periodicamente a disponibilidade da API

### Melhorias Futuras
- Cache local dos dados consultados
- Suporte a CPF (pessoas físicas)
- Integração com outras APIs de consulta
- Histórico de consultas
