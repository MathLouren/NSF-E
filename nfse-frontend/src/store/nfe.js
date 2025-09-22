import { defineStore } from 'pinia'
import api from '@/services/api'

export const useNFeStore = defineStore('nfe', {
  state: () => ({
    ambiente: 'Homologacao',
    certificado: null,
    nfeList: [],
    loading: false,
    error: null
  }),

  getters: {
    isHomologacao: (state) => state.ambiente === 'Homologacao',
    isProducao: (state) => state.ambiente === 'Producao',
    certificadoValido: (state) => state.certificado && state.certificado.valido
  },

  actions: {
    // Carregar status do ambiente
    async carregarAmbiente() {
      try {
        const { data } = await api.get('/NFe/ambiente')
        this.ambiente = data.ambiente
        return data
      } catch (error) {
        this.error = error.message
        throw error
      }
    },

    // Alterar ambiente
    async alterarAmbiente(producao = false) {
      try {
        const { data } = await api.post('/NFe/ambiente', { Producao: producao })
        this.ambiente = producao ? 'Producao' : 'Homologacao'
        return data
      } catch (error) {
        this.error = error.message
        throw error
      }
    },

    // Carregar status do certificado
    async carregarCertificado() {
      try {
        const { data } = await api.get('/NFe/certificado/status')
        this.certificado = data
        return data
      } catch (error) {
        this.error = error.message
        throw error
      }
    },

    // Upload de certificado
    async uploadCertificado(formData) {
      try {
        const { data } = await api.post('/NFe/certificado/upload', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        })
        
        // Recarregar status do certificado
        await this.carregarCertificado()
        
        return data
      } catch (error) {
        this.error = error.message
        throw error
      }
    },

    // Emitir NF-e
    async emitirNFe(nfeData) {
      this.loading = true
      this.error = null
      
      try {
        const { data } = await api.post('/NFe/emitir', nfeData)
        
        // Recarregar lista de NF-e
        await this.carregarNFeList()
        
        return data
      } catch (error) {
        this.error = error.response?.data?.mensagem || error.message
        throw error
      } finally {
        this.loading = false
      }
    },

    // Carregar lista de NF-e
    async carregarNFeList(filtros = {}) {
      this.loading = true
      this.error = null
      
      try {
        const { data } = await api.get('/NFe/lista', { params: filtros })
        this.nfeList = data.itens || []
        return data
      } catch (error) {
        this.error = error.message
        this.nfeList = []
        throw error
      } finally {
        this.loading = false
      }
    },

    // Gerar DANFE
    async gerarDANFE(protocolo) {
      try {
        const response = await api.get(`/NFe/danfe/${protocolo}`, {
          responseType: 'blob'
        })
        
        return response.data
      } catch (error) {
        this.error = error.message
        throw error
      }
    },

    // Cancelar NF-e
    async cancelarNFe(chaveAcesso, justificativa) {
      try {
        const { data } = await api.post('/NFe/cancelar', {
          chaveAcesso,
          justificativa
        })
        
        // Recarregar lista de NF-e
        await this.carregarNFeList()
        
        return data
      } catch (error) {
        this.error = error.response?.data?.mensagem || error.message
        throw error
      }
    },

    // Consultar status de uma NF-e
    async consultarStatus(chaveAcesso) {
      try {
        const { data } = await api.get(`/NFe/consulta/${chaveAcesso}`)
        return data
      } catch (error) {
        this.error = error.message
        throw error
      }
    },

    // Limpar erro
    limparErro() {
      this.error = null
    }
  }
})
