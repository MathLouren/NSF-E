/**
 * Configuração da API para o sistema NFSe
 */

// Configurações baseadas no ambiente
const config = {
  development: {
    baseURL: 'http://localhost:5000/api',
    timeout: 30000, // 30 segundos para operações de DANFE
    retryAttempts: 3,
    retryDelay: 1000
  },
  production: {
    baseURL: import.meta.env.VITE_API_URL || '/api',
    timeout: 60000, // 60 segundos para produção
    retryAttempts: 5,
    retryDelay: 2000
  }
}

// Determinar ambiente
const environment = import.meta.env.MODE || 'development'
const currentConfig = config[environment]

// Endpoints específicos
export const endpoints = {
  // NFe endpoints
  nfe: {
    emitir: '/NFe/emitir',
    listar: '/NFe/listar',
    danfe: (protocolo) => `/NFe/danfe/${protocolo}`,
    cancelar: '/NFe/cancelar',
    inutilizar: '/NFe/inutilizar'
  },
  
  // NFe 2026 endpoints
  nfe2026: {
    emitir: '/nfe2026/emitir',
    gerarDanfe2026: '/nfe2026/gerar-danfe-2026',
    gerarDanfe2026Base64: '/nfe2026/gerar-danfe-2026-base64',
    gerarDanfeVersao: '/nfe2026/gerar-danfe-versao',
    obterVersaoLayout: '/nfe2026/versao-layout',
    verificarConformidade: '/nfe2026/verificar-conformidade'
  },
  
  // NFS-e endpoints
  nfse: {
    criar: '/nfse',
    listar: '/nfse',
    detalhe: (id) => `/nfse/${id}`,
    danfe: (id) => `/nfse/${id}/danfe`
  },
  
  // Tabelas fiscais
  fiscalTables: {
    carregar: '/fiscal-tables',
    upload: '/fiscal-tables/upload',
    validar: '/fiscal-tables/validate'
  },
  
  // Autenticação
  auth: {
    login: '/auth/login',
    logout: '/auth/logout',
    refresh: '/auth/refresh'
  },
  
  // Configurações
  config: {
    obter: '/configurations',
    salvar: '/configurations'
  }
}

// Configurações de retry para operações críticas
export const retryConfig = {
  danfe: {
    maxAttempts: 3,
    delay: 2000,
    backoff: 'exponential'
  },
  emitir: {
    maxAttempts: 2,
    delay: 5000,
    backoff: 'linear'
  },
  default: {
    maxAttempts: 2,
    delay: 1000,
    backoff: 'linear'
  }
}

// Headers padrão
export const defaultHeaders = {
  'Content-Type': 'application/json',
  'Accept': 'application/json'
}

// Configurações de timeout por operação
export const timeoutConfig = {
  danfe: 45000,      // 45 segundos para geração de DANFE
  emitir: 60000,     // 60 segundos para emissão
  upload: 120000,    // 2 minutos para upload de arquivos
  default: 30000     // 30 segundos padrão
}

export default currentConfig
