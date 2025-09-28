<template>
  <div>
    <label class="block text-sm font-medium mb-1">{{ label }}</label>
    <div class="relative">
      <input
        :type="type"
        :value="formattedValue"
        @input="handleInput"
        @blur="handleBlur"
        :placeholder="placeholder"
        :disabled="loading"
        class="w-full border rounded-lg px-3 py-2 pr-10 focus:outline-none focus:ring focus:border-blue-300 disabled:bg-gray-100"
        :class="{ 'border-red-500': error }"
      />
      <div v-if="loading" class="absolute right-3 top-1/2 transform -translate-y-1/2">
        <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
      </div>
      <button
        v-else-if="showSearchButton && isValidCnpj"
        type="button"
        @click="consultarCnpj"
        class="absolute right-3 top-1/2 transform -translate-y-1/2 text-blue-600 hover:text-blue-800"
        title="Consultar dados da empresa"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
        </svg>
      </button>
    </div>
    <div v-if="error" class="text-red-500 text-sm mt-1">{{ error }}</div>
    <div v-if="successMessage" class="text-green-600 text-sm mt-1">{{ successMessage }}</div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import cnpjService from '@/services/cnpjService'

const props = defineProps({
  label: { type: String, required: true },
  modelValue: { type: String, default: '' },
  type: { type: String, default: 'text' },
  placeholder: { type: String, default: '00.000.000/0000-00' },
  showSearchButton: { type: Boolean, default: true },
  autoConsult: { type: Boolean, default: true }
})

const emit = defineEmits(['update:modelValue', 'cnpj-consulted'])

const loading = ref(false)
const error = ref('')
const successMessage = ref('')

// Valor formatado para exibição
const formattedValue = computed(() => {
  if (!props.modelValue) return ''
  return cnpjService.formatarCnpj(props.modelValue)
})

// Verifica se o CNPJ é válido
const isValidCnpj = computed(() => {
  if (!props.modelValue) return false
  return cnpjService.validarCnpj(props.modelValue)
})

// Limpa mensagens de erro/sucesso
const clearMessages = () => {
  error.value = ''
  successMessage.value = ''
}

// Manipula input do usuário
const handleInput = (event) => {
  clearMessages()
  const value = event.target.value.replace(/\D/g, '') // Remove caracteres não numéricos
  emit('update:modelValue', value)
}

// Manipula blur do input
const handleBlur = () => {
  if (props.autoConsult && isValidCnpj.value) {
    consultarCnpj()
  }
}

// Consulta dados do CNPJ
const consultarCnpj = async () => {
  if (!isValidCnpj.value) {
    error.value = 'CNPJ inválido'
    return
  }

  loading.value = true
  clearMessages()

  try {
    const dadosEmpresa = await cnpjService.consultarCnpj(props.modelValue)
    
    // Emite evento com os dados da empresa
    emit('cnpj-consulted', dadosEmpresa)
    
    successMessage.value = 'Dados da empresa carregados com sucesso!'
    
    // Remove mensagem de sucesso após 3 segundos
    setTimeout(() => {
      successMessage.value = ''
    }, 3000)
    
  } catch (err) {
    error.value = err.message || 'Erro ao consultar CNPJ'
    console.error('Erro ao consultar CNPJ:', err)
  } finally {
    loading.value = false
  }
}

// Watch para limpar mensagens quando o valor muda
watch(() => props.modelValue, () => {
  if (error.value || successMessage.value) {
    clearMessages()
  }
})
</script>

<style scoped>
/* Estilos específicos do componente */
</style>
