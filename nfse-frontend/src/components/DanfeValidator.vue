<template>
  <div class="danfe-validator">
    <!-- Validação de Dados -->
    <div v-if="showValidation" class="mb-6 bg-white rounded-lg shadow-md p-6">
      <h3 class="text-lg font-semibold text-gray-800 mb-4">
        🔍 Validação de Dados para DANFE
      </h3>
      
      <!-- Erros Críticos -->
      <div v-if="validationResult.errors.length > 0" class="mb-4">
        <div class="flex items-center mb-2">
          <svg class="h-5 w-5 text-red-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
          </svg>
          <h4 class="font-medium text-red-800">Erros Críticos</h4>
        </div>
        <ul class="list-disc list-inside space-y-1 text-sm text-red-700">
          <li v-for="error in validationResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
      
      <!-- Avisos -->
      <div v-if="validationResult.warnings.length > 0" class="mb-4">
        <div class="flex items-center mb-2">
          <svg class="h-5 w-5 text-yellow-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
          </svg>
          <h4 class="font-medium text-yellow-800">Avisos</h4>
        </div>
        <ul class="list-disc list-inside space-y-1 text-sm text-yellow-700">
          <li v-for="warning in validationResult.warnings" :key="warning">{{ warning }}</li>
        </ul>
      </div>
      
      <!-- Status da Validação -->
      <div class="flex items-center justify-between pt-4 border-t">
        <div class="flex items-center">
          <div v-if="validationResult.errors.length === 0" class="flex items-center text-green-600">
            <svg class="h-5 w-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
            </svg>
            <span class="font-medium">Dados válidos para geração de DANFE</span>
          </div>
          <div v-else class="flex items-center text-red-600">
            <svg class="h-5 w-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
            </svg>
            <span class="font-medium">Corrija os erros antes de gerar DANFE</span>
          </div>
        </div>
        
        <button 
          @click="validateData"
          :disabled="loading"
          class="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:bg-gray-400 transition-colors duration-200"
        >
          <span v-if="loading" class="flex items-center">
            <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Validando...
          </span>
          <span v-else>🔄 Revalidar</span>
        </button>
      </div>
    </div>
    
    <!-- Botão para mostrar/ocultar validação -->
    <div class="mb-4">
      <button 
        @click="toggleValidation"
        class="bg-gray-100 text-gray-700 px-4 py-2 rounded-lg text-sm hover:bg-gray-200 transition-colors duration-200"
      >
        <span v-if="!showValidation">🔍 Validar Dados</span>
        <span v-else>👁️ Ocultar Validação</span>
      </button>
    </div>
  </div>
</template>

<script>
import fiscalTablesService from '@/services/fiscalTablesService'

export default {
  name: 'DanfeValidator',
  props: {
    nfeData: {
      type: Object,
      required: true
    },
    operationType: {
      type: String,
      default: 'produto'
    }
  },
  data() {
    return {
      showValidation: false,
      loading: false,
      validationResult: {
        errors: [],
        warnings: []
      }
    }
  },
  watch: {
    nfeData: {
      handler() {
        if (this.showValidation) {
          this.validateData()
        }
      },
      deep: true
    }
  },
  methods: {
    toggleValidation() {
      this.showValidation = !this.showValidation
      if (this.showValidation) {
        this.validateData()
      }
    },
    
    async validateData() {
      this.loading = true
      
      try {
        // Carregar tabelas fiscais se necessário
        if (!fiscalTablesService.isTablesUpdated()) {
          await fiscalTablesService.loadFiscalTables()
        }
        
        // Validar dados
        this.validationResult = fiscalTablesService.validateRequiredFields(
          this.nfeData, 
          this.operationType
        )
        
        // Emitir evento com resultado da validação
        this.$emit('validation-result', this.validationResult)
        
      } catch (error) {
        console.error('Erro na validação:', error)
        // Em caso de erro, fazer validação básica
        this.validationResult = this.validateBasicFields()
        this.$emit('validation-result', this.validationResult)
      } finally {
        this.loading = false
      }
    },
    
    // Validação básica quando o serviço fiscal não está disponível
    validateBasicFields() {
      const errors = []
      const warnings = []
      
      // Validações básicas
      if (!this.nfeData.ide?.natOp) errors.push('Natureza da operação é obrigatória')
      if (!this.nfeData.ide?.mod) errors.push('Modelo do documento é obrigatório')
      if (!this.nfeData.ide?.serie) errors.push('Série é obrigatória')
      if (!this.nfeData.ide?.nNF) errors.push('Número da NF-e é obrigatório')
      
      // Validações do emitente
      if (!this.nfeData.emit?.CNPJ) errors.push('CNPJ do emitente é obrigatório')
      if (!this.nfeData.emit?.xNome) errors.push('Razão social do emitente é obrigatória')
      if (!this.nfeData.emit?.enderEmit?.UF) errors.push('UF do emitente é obrigatória')
      
      // Validações do destinatário
      if (!this.nfeData.dest?.CNPJ && !this.nfeData.dest?.CPF) {
        errors.push('CNPJ ou CPF do destinatário é obrigatório')
      }
      
      // Validações dos produtos
      if (!this.nfeData.det || this.nfeData.det.length === 0) {
        errors.push('Pelo menos um produto deve ser informado')
      } else {
        this.nfeData.det.forEach((item, index) => {
          if (!item.prod?.cProd) errors.push(`Código do produto ${index + 1} é obrigatório`)
          if (!item.prod?.xProd) errors.push(`Descrição do produto ${index + 1} é obrigatória`)
          if (!item.prod?.NCM) errors.push(`NCM do produto ${index + 1} é obrigatório`)
          if (!item.prod?.CFOP) errors.push(`CFOP do produto ${index + 1} é obrigatório`)
          if (!item.prod?.uCom) errors.push(`Unidade comercial do produto ${index + 1} é obrigatória`)
          if (!item.prod?.qCom) errors.push(`Quantidade do produto ${index + 1} é obrigatória`)
          if (!item.prod?.vUnCom) errors.push(`Valor unitário do produto ${index + 1} é obrigatório`)
        })
      }
      
      return { errors, warnings }
    },
    
    // Método público para validar sem mostrar a interface
    async validateSilently() {
      try {
        if (!fiscalTablesService.isTablesUpdated()) {
          await fiscalTablesService.loadFiscalTables()
        }
        
        return fiscalTablesService.validateRequiredFields(
          this.nfeData, 
          this.operationType
        )
      } catch (error) {
        console.warn('Erro na validação fiscal, usando validação básica:', error.message)
        return this.validateBasicFields()
      }
    }
  }
}
</script>

<style scoped>
.danfe-validator {
  max-width: 100%;
}

/* Animações suaves */
.transition-colors {
  transition: background-color 0.2s ease-in-out, color 0.2s ease-in-out;
}
</style>
