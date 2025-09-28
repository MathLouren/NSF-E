<template>
  <div class="relative">
    <label class="block text-sm font-medium mb-1">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
      <span v-if="helpText" class="ml-1 text-gray-400 cursor-help" :title="helpText">
        <svg class="w-4 h-4 inline" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd"></path>
        </svg>
      </span>
    </label>
    
    <div class="relative">
      <input
        :type="inputType"
        :value="modelValue"
        @input="handleInput"
        @focus="handleFocus"
        @blur="handleBlur"
        :placeholder="placeholder"
        :disabled="disabled"
        :class="inputClasses"
        autocomplete="off"
      />
      
      <!-- Loading indicator -->
      <div v-if="loading" class="absolute right-3 top-1/2 transform -translate-y-1/2">
        <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
      </div>
      
      <!-- Clear button -->
      <button
        v-else-if="modelValue && !disabled"
        type="button"
        @click="clearValue"
        class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
        </svg>
      </button>
    </div>

    <!-- Suggestions dropdown -->
    <div
      v-if="showSuggestions && suggestions.length > 0"
      class="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto"
    >
      <div
        v-for="(suggestion, index) in suggestions"
        :key="index"
        @click="selectSuggestion(suggestion)"
        :class="[
          'px-3 py-2 cursor-pointer hover:bg-gray-100',
          index === selectedSuggestionIndex ? 'bg-blue-50' : ''
        ]"
      >
        <div class="flex justify-between items-center">
          <div>
            <div class="font-medium">{{ suggestion.code || suggestion.codigo }}</div>
            <div class="text-sm text-gray-600">{{ suggestion.description || suggestion.descricao || suggestion.nome }}</div>
          </div>
          <div v-if="suggestion.rate" class="text-sm text-gray-500">
            {{ suggestion.rate }}%
          </div>
        </div>
      </div>
    </div>

    <!-- Error message -->
    <div v-if="error" class="text-red-500 text-sm mt-1 flex items-center">
      <svg class="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
      </svg>
      {{ error }}
    </div>

    <!-- Warning message -->
    <div v-if="warning" class="text-yellow-600 text-sm mt-1 flex items-center">
      <svg class="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
      </svg>
      {{ warning }}
    </div>

    <!-- Success message -->
    <div v-if="success" class="text-green-600 text-sm mt-1 flex items-center">
      <svg class="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
      </svg>
      {{ success }}
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import fiscalTablesService from '@/services/fiscalTablesService'

const props = defineProps({
  label: { type: String, required: true },
  modelValue: { type: [String, Number], default: '' },
  type: { type: String, default: 'text' }, // cst, cfop, ncm, municipio, etc.
  inputType: { type: String, default: 'text' },
  placeholder: { type: String, default: '' },
  required: { type: Boolean, default: false },
  disabled: { type: Boolean, default: false },
  helpText: { type: String, default: '' },
  validationRules: { type: Object, default: () => ({}) },
  filters: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue', 'validation-change'])

const loading = ref(false)
const error = ref('')
const warning = ref('')
const success = ref('')
const showSuggestions = ref(false)
const suggestions = ref([])
const selectedSuggestionIndex = ref(-1)
const debounceTimer = ref(null)

const inputClasses = computed(() => {
  const baseClasses = 'w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300'
  
  if (error.value) {
    return `${baseClasses} border-red-500 focus:border-red-500 focus:ring-red-200`
  } else if (warning.value) {
    return `${baseClasses} border-yellow-500 focus:border-yellow-500 focus:ring-yellow-200`
  } else if (success.value) {
    return `${baseClasses} border-green-500 focus:border-green-500 focus:ring-green-200`
  } else {
    return `${baseClasses} border-gray-300`
  }
})

const handleInput = (event) => {
  const value = event.target.value
  emit('update:modelValue', value)
  
  // Limpa mensagens anteriores
  clearMessages()
  
  // Debounce para busca de sugestões
  if (debounceTimer.value) {
    clearTimeout(debounceTimer.value)
  }
  
  debounceTimer.value = setTimeout(() => {
    if (value.length >= 2) {
      searchSuggestions(value)
    } else {
      suggestions.value = []
      showSuggestions.value = false
    }
  }, 300)
}

const handleFocus = () => {
  if (props.modelValue && props.modelValue.length >= 2) {
    searchSuggestions(props.modelValue)
  }
}

const handleBlur = () => {
  // Delay para permitir clique nas sugestões
  setTimeout(() => {
    showSuggestions.value = false
    validateField()
  }, 200)
}

const searchSuggestions = async (query) => {
  if (!props.type) return
  
  loading.value = true
  
  try {
    const results = fiscalTablesService.getSuggestions(props.type, query, props.filters)
    suggestions.value = results
    showSuggestions.value = results.length > 0
    selectedSuggestionIndex.value = -1
  } catch (err) {
    console.error('Erro ao buscar sugestões:', err)
  } finally {
    loading.value = false
  }
}

const selectSuggestion = (suggestion) => {
  const value = suggestion.code || suggestion.codigo || suggestion.value
  emit('update:modelValue', value)
  showSuggestions.value = false
  validateField()
  
  // Emit evento de seleção se necessário
  if (suggestion.rate) {
    emit('validation-change', { 
      type: 'rate-update', 
      rate: suggestion.rate,
      description: suggestion.description || suggestion.descricao
    })
  }
}

const clearValue = () => {
  emit('update:modelValue', '')
  clearMessages()
  suggestions.value = []
  showSuggestions.value = false
}

const validateField = () => {
  const value = props.modelValue
  
  // Validação de campo obrigatório
  if (props.required && !value) {
    error.value = `${props.label} é obrigatório`
    emit('validation-change', { type: 'error', message: error.value })
    return
  }
  
  // Validações específicas por tipo
  switch (props.type) {
    case 'cst':
      const cstValidation = fiscalTablesService.validateCST(value, props.filters.operationType)
      if (value && !cstValidation) {
        warning.value = 'CST não encontrado nas tabelas fiscais'
        emit('validation-change', { type: 'warning', message: warning.value })
      } else if (cstValidation) {
        success.value = cstValidation.description
        emit('validation-change', { type: 'success', message: success.value })
      }
      break
      
    case 'cfop':
      const cfopValidation = fiscalTablesService.validateCFOP(
        value, 
        props.filters.operationType,
        props.filters.ufOrigin,
        props.filters.ufDest
      )
      if (value && !cfopValidation) {
        warning.value = 'CFOP não válido para esta operação/UF'
        emit('validation-change', { type: 'warning', message: warning.value })
      } else if (cfopValidation) {
        success.value = cfopValidation.description
        emit('validation-change', { type: 'success', message: success.value })
      }
      break
      
    case 'ncm':
      const ncmValidation = fiscalTablesService.validateNCM(value)
      if (value && !ncmValidation) {
        warning.value = 'NCM não encontrado nas tabelas fiscais'
        emit('validation-change', { type: 'warning', message: warning.value })
      } else if (ncmValidation) {
        success.value = ncmValidation.description
        emit('validation-change', { type: 'success', message: success.value })
      }
      break
  }
}

const clearMessages = () => {
  error.value = ''
  warning.value = ''
  success.value = ''
}

// Watch para validação quando o valor muda
watch(() => props.modelValue, () => {
  if (props.modelValue) {
    validateField()
  } else {
    clearMessages()
  }
})

// Watch para mudanças nos filtros
watch(() => props.filters, () => {
  if (props.modelValue) {
    validateField()
  }
}, { deep: true })

// Keyboard navigation
const handleKeydown = (event) => {
  if (!showSuggestions.value || suggestions.value.length === 0) return
  
  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      selectedSuggestionIndex.value = Math.min(
        selectedSuggestionIndex.value + 1,
        suggestions.value.length - 1
      )
      break
    case 'ArrowUp':
      event.preventDefault()
      selectedSuggestionIndex.value = Math.max(selectedSuggestionIndex.value - 1, -1)
      break
    case 'Enter':
      event.preventDefault()
      if (selectedSuggestionIndex.value >= 0) {
        selectSuggestion(suggestions.value[selectedSuggestionIndex.value])
      }
      break
    case 'Escape':
      showSuggestions.value = false
      selectedSuggestionIndex.value = -1
      break
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
  if (props.modelValue) {
    validateField()
  }
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
  if (debounceTimer.value) {
    clearTimeout(debounceTimer.value)
  }
})
</script>

<style scoped>
/* Estilos específicos do componente */
</style>
