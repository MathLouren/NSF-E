<template>
  <div class="danfe-version-selector">
    <div class="bg-white rounded-lg shadow-md p-6">
      <h3 class="text-lg font-semibold text-gray-800 mb-4">
        🎯 Selecionar Versão da DANFE
      </h3>
      
      <div class="space-y-4">
        <!-- Opções de versão -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <!-- Versão Atual -->
          <div 
            class="border-2 rounded-lg p-4 cursor-pointer transition-all duration-200"
            :class="selectedVersion === 'atual' ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'"
            @click="selectVersion('atual')"
          >
            <div class="flex items-center space-x-3">
              <input 
                type="radio" 
                :value="'atual'" 
                v-model="selectedVersion"
                class="text-blue-600"
              />
              <div>
                <h4 class="font-medium text-gray-800">Versão Atual</h4>
                <p class="text-sm text-gray-600">Layout 4.00</p>
              </div>
            </div>
            <div class="mt-2 text-xs text-gray-500">
              <ul class="list-disc list-inside space-y-1">
                <li>ICMS tradicional</li>
                <li>IPI, PIS, COFINS</li>
                <li>Layout básico</li>
              </ul>
            </div>
          </div>

          <!-- Versão 2026 -->
          <div 
            class="border-2 rounded-lg p-4 cursor-pointer transition-all duration-200"
            :class="selectedVersion === '2026' ? 'border-green-500 bg-green-50' : 'border-gray-200 hover:border-gray-300'"
            @click="selectVersion('2026')"
          >
            <div class="flex items-center space-x-3">
              <input 
                type="radio" 
                :value="'2026'" 
                v-model="selectedVersion"
                class="text-green-600"
              />
              <div>
                <h4 class="font-medium text-gray-800">Versão 2026</h4>
                <p class="text-sm text-gray-600">Reforma Tributária</p>
              </div>
            </div>
            <div class="mt-2 text-xs text-gray-500">
              <ul class="list-disc list-inside space-y-1">
                <li>IBS, CBS, IS</li>
                <li>Rastreabilidade</li>
                <li>Totais por UF/Município</li>
              </ul>
            </div>
          </div>

          <!-- Ambas as Versões -->
          <div 
            class="border-2 rounded-lg p-4 cursor-pointer transition-all duration-200"
            :class="selectedVersion === 'ambas' ? 'border-purple-500 bg-purple-50' : 'border-gray-200 hover:border-gray-300'"
            @click="selectVersion('ambas')"
          >
            <div class="flex items-center space-x-3">
              <input 
                type="radio" 
                :value="'ambas'" 
                v-model="selectedVersion"
                class="text-purple-600"
              />
              <div>
                <h4 class="font-medium text-gray-800">Ambas</h4>
                <p class="text-sm text-gray-600">Comparação</p>
              </div>
            </div>
            <div class="mt-2 text-xs text-gray-500">
              <ul class="list-disc list-inside space-y-1">
                <li>Duas DANFEs</li>
                <li>Comparação visual</li>
                <li>Destaque das diferenças</li>
              </ul>
            </div>
          </div>
        </div>

        <!-- Opções adicionais -->
        <div class="flex items-center space-x-4 pt-4 border-t">
          <label class="flex items-center space-x-2">
            <input 
              type="checkbox" 
              v-model="isContingencia"
              class="text-orange-600"
            />
            <span class="text-sm text-gray-700">Modo Contingência</span>
          </label>
        </div>

        <!-- Validação de Dados -->
        <DanfeValidator 
          :nfe-data="nfeData"
          :operation-type="getOperationType()"
          @validation-result="onValidationResult"
          ref="validator"
        />

        <!-- Botão de geração -->
        <div class="pt-4">
          <button 
            @click="gerarDanfe"
            :disabled="loading || !selectedVersion || hasValidationErrors"
            class="w-full bg-blue-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors duration-200"
          >
            <span v-if="loading" class="flex items-center justify-center">
              <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Gerando DANFE...
            </span>
            <span v-else-if="hasValidationErrors">
              ❌ Corrija os erros antes de gerar
            </span>
            <span v-else>
              🎯 Gerar DANFE {{ getVersionText() }}
            </span>
          </button>
        </div>
      </div>
    </div>

    <!-- Resultado -->
    <div v-if="resultado" class="mt-6 bg-white rounded-lg shadow-md p-6">
      <h4 class="text-lg font-semibold text-gray-800 mb-4">
        ✅ DANFE(s) Gerada(s) com Sucesso!
      </h4>
      
      <div class="space-y-4">
        <div v-for="danfe in resultado.danfes" :key="danfe.versao" class="border rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <h5 class="font-medium text-gray-800">
              📄 {{ danfe.versao === 'atual' ? 'DANFE Atual' : 'DANFE 2026' }}
            </h5>
            <span class="text-sm text-gray-500">Layout {{ danfe.layout }}</span>
          </div>
          
          <div class="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span class="text-gray-600">Arquivo:</span>
              <span class="font-medium">{{ danfe.nomeArquivo }}</span>
            </div>
            <div>
              <span class="text-gray-600">Tamanho:</span>
              <span class="font-medium">{{ formatBytes(danfe.tamanhoBytes) }}</span>
            </div>
          </div>
          
          <div class="mt-3">
            <button 
              @click="downloadDanfe(danfe)"
              class="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700 transition-colors duration-200"
            >
              📥 Baixar PDF
            </button>
          </div>
        </div>

        <!-- Comparação -->
        <div v-if="resultado.comparacao" class="border rounded-lg p-4 bg-purple-50">
          <h5 class="font-medium text-gray-800 mb-3">📊 Comparação das Versões</h5>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
            <div>
              <h6 class="font-medium text-gray-700 mb-2">🆕 Novos Campos (2026)</h6>
              <ul class="space-y-1">
                <li v-for="campo in resultado.comparacao.camposNovos" :key="campo" class="text-green-700">
                  + {{ campo }}
                </li>
              </ul>
            </div>
            
            <div>
              <h6 class="font-medium text-gray-700 mb-2">🔄 Campos Mantidos</h6>
              <ul class="space-y-1">
                <li v-for="campo in resultado.comparacao.camposMantidos" :key="campo" class="text-blue-700">
                  = {{ campo }}
                </li>
              </ul>
            </div>
            
            <div>
              <h6 class="font-medium text-gray-700 mb-2">⚡ Principais Diferenças</h6>
              <ul class="space-y-1">
                <li v-for="diff in resultado.comparacao.diferencasPrincipais" :key="diff" class="text-purple-700">
                  • {{ diff }}
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Erro -->
    <div v-if="erro" class="mt-6 bg-red-50 border border-red-200 rounded-lg p-4">
      <div class="flex items-center">
        <svg class="h-5 w-5 text-red-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
        </svg>
        <span class="text-red-800 font-medium">Erro ao gerar DANFE</span>
      </div>
      <p class="text-red-700 mt-1">{{ erro }}</p>
    </div>
  </div>
</template>

<script>
import DanfeValidator from './DanfeValidator.vue'
import api from '@/services/api'

export default {
  name: 'DanfeVersionSelector',
  components: {
    DanfeValidator
  },
  props: {
    nfeData: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      selectedVersion: '2026',
      isContingencia: false,
      loading: false,
      resultado: null,
      erro: null,
      hasValidationErrors: false,
      validationResult: {
        errors: [],
        warnings: []
      }
    }
  },
  methods: {
    selectVersion(version) {
      this.selectedVersion = version
      this.erro = null
      this.resultado = null
    },
    
    getVersionText() {
      switch (this.selectedVersion) {
        case 'atual': return '(Versão Atual)'
        case '2026': return '(Reforma Tributária)'
        case 'ambas': return '(Comparação)'
        default: return ''
      }
    },
    
    getOperationType() {
      // Determinar tipo de operação baseado nos dados da NFe
      if (this.nfeData.ide?.natOp?.toLowerCase().includes('devolução')) {
        return 'devolucao'
      } else if (this.nfeData.ide?.natOp?.toLowerCase().includes('crédito')) {
        return 'credito'
      } else if (this.nfeData.ide?.natOp?.toLowerCase().includes('monofásica')) {
        return 'monofasia'
      }
      return 'produto'
    },
    
    onValidationResult(result) {
      this.validationResult = result
      this.hasValidationErrors = result.errors.length > 0
    },
    
    async gerarDanfe() {
      // Validar dados antes de gerar
      if (this.$refs.validator) {
        const validation = await this.$refs.validator.validateSilently()
        if (validation.errors.length > 0) {
          this.erro = 'Corrija os erros de validação antes de gerar a DANFE'
          return
        }
      }
      
      this.loading = true
      this.erro = null
      this.resultado = null
      
      try {
        const requestData = {
          NFe: this.nfeData,
          Versao: this.selectedVersion,
          IsContingencia: this.isContingencia
        }
        
        console.log('Dados sendo enviados:', requestData)
        console.log('NFe data:', this.nfeData)
        
        const response = await api.post('/nfe2026/gerar-danfe-versao', requestData, {
          timeout: 45000 // 45 segundos para geração de DANFE
        })
        
        const data = response.data
        
        if (data.sucesso) {
          this.resultado = data
          this.$emit('danfe-gerada', data)
        } else {
          throw new Error(data.erro || 'Erro desconhecido')
        }
      } catch (error) {
        this.erro = error.message
        console.error('Erro ao gerar DANFE:', error)
      } finally {
        this.loading = false
      }
    },
    
    downloadDanfe(danfe) {
      try {
        const pdfBytes = this.base64ToArrayBuffer(danfe.danfeBase64)
        const blob = new Blob([pdfBytes], { type: 'application/pdf' })
        const url = window.URL.createObjectURL(blob)
        
        const link = document.createElement('a')
        link.href = url
        link.download = danfe.nomeArquivo
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        
        window.URL.revokeObjectURL(url)
      } catch (error) {
        console.error('Erro ao baixar arquivo:', error)
        this.erro = 'Erro ao baixar arquivo PDF'
      }
    },
    
    base64ToArrayBuffer(base64) {
      const binaryString = window.atob(base64)
      const bytes = new Uint8Array(binaryString.length)
      for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i)
      }
      return bytes.buffer
    },
    
    formatBytes(bytes) {
      if (bytes === 0) return '0 Bytes'
      const k = 1024
      const sizes = ['Bytes', 'KB', 'MB', 'GB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    }
  }
}
</script>

<style scoped>
.danfe-version-selector {
  max-width: 800px;
  margin: 0 auto;
}

/* Animações suaves */
.transition-all {
  transition: all 0.2s ease-in-out;
}

/* Hover effects */
.hover\:border-gray-300:hover {
  border-color: #d1d5db;
}

.hover\:bg-blue-700:hover {
  background-color: #1d4ed8;
}

.hover\:bg-green-700:hover {
  background-color: #15803d;
}
</style>
