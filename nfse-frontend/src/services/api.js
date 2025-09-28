import axios from "axios"
import config, { defaultHeaders, timeoutConfig } from "@/config/api"

// Criar instância do axios com configurações
const api = axios.create({
  baseURL: config.baseURL,
  timeout: timeoutConfig.default,
  headers: defaultHeaders
})

// Interceptor para requisições
api.interceptors.request.use(
  (config) => {
    // Adicionar token de autenticação se disponível
    const token = localStorage.getItem('userToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    // Log para debug em desenvolvimento
    if (import.meta.env.DEV) {
      console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`)
    }
    
    return config
  },
  (error) => {
    console.error('❌ API Request Error:', error)
    return Promise.reject(error)
  }
)

// Interceptor para respostas
api.interceptors.response.use(
  (response) => {
    // Log para debug em desenvolvimento
    if (import.meta.env.DEV) {
      console.log(`✅ API Response: ${response.status} ${response.config.url}`)
    }
    
    return response
  },
  (error) => {
    console.error('❌ API Response Error:', error)
    
    // Tratar erros específicos
    if (error.response?.status === 401) {
      // Token expirado ou inválido
      localStorage.removeItem('userToken')
      window.location.href = '/login'
    } else if (error.response?.status === 403) {
      // Sem permissão
      console.error('Acesso negado')
    } else if (error.response?.status >= 500) {
      // Erro do servidor
      console.error('Erro interno do servidor')
    }
    
    return Promise.reject(error)
  }
)

// Função para fazer requisições com retry
export const apiWithRetry = async (requestConfig, retryConfig = {}) => {
  const maxAttempts = retryConfig.maxAttempts || 2
  const delay = retryConfig.delay || 1000
  
  for (let attempt = 1; attempt <= maxAttempts; attempt++) {
    try {
      return await api(requestConfig)
    } catch (error) {
      if (attempt === maxAttempts) {
        throw error
      }
      
      // Aguardar antes da próxima tentativa
      await new Promise(resolve => setTimeout(resolve, delay * attempt))
    }
  }
}

export default api