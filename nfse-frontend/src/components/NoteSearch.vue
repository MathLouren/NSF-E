<template>
  <div class="bg-white border rounded-lg p-6">
    <h3 class="text-lg font-semibold text-gray-900 mb-4">Buscar Nota para Devolução/Crédito</h3>
    
    <!-- Filtros de busca -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Tipo de Busca</label>
        <select
          v-model="searchType"
          class="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
        >
          <option value="chave">Chave de Acesso</option>
          <option value="numero">Número da NF-e</option>
          <option value="cnpj">CNPJ do Emitente</option>
          <option value="periodo">Período</option>
        </select>
      </div>
      
      <div v-if="searchType === 'chave'">
        <label class="block text-sm font-medium text-gray-700 mb-1">Chave de Acesso</label>
        <input
          v-model="searchValue"
          type="text"
          placeholder="44 dígitos"
          maxlength="44"
          class="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
        />
      </div>
      
      <div v-else-if="searchType === 'numero'">
        <label class="block text-sm font-medium text-gray-700 mb-1">Número da NF-e</label>
        <input
          v-model="searchValue"
          type="number"
          placeholder="Número da nota"
          class="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
        />
      </div>
      
      <div v-else-if="searchType === 'cnpj'">
        <label class="block text-sm font-medium text-gray-700 mb-1">CNPJ do Emitente</label>
        <input
          v-model="searchValue"
          type="text"
          placeholder="00.000.000/0000-00"
          class="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
        />
      </div>
      
      <div v-else-if="searchType === 'periodo'" class="md:col-span-2">
        <div class="grid grid-cols-2 gap-2">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Data Inicial</label>
            <input
              v-model="dateFrom"
              type="date"
              class="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Data Final</label>
            <input
              v-model="dateTo"
              type="date"
              class="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
            />
          </div>
        </div>
      </div>
      
      <div class="flex items-end">
        <button
          @click="searchNotes"
          :disabled="loading || !canSearch"
          class="w-full bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span v-if="loading">Buscando...</span>
          <span v-else>Buscar</span>
        </button>
      </div>
    </div>

    <!-- Resultados da busca -->
    <div v-if="searchResults.length > 0" class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Notas Encontradas</h4>
      <div class="space-y-3">
        <div
          v-for="note in searchResults"
          :key="note.chave"
          class="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 cursor-pointer"
          :class="{ 'bg-blue-50 border-blue-300': selectedNote?.chave === note.chave }"
          @click="selectNote(note)"
        >
          <div class="flex justify-between items-start">
            <div class="flex-1">
              <div class="flex items-center space-x-4 mb-2">
                <span class="font-medium text-gray-900">NF-e {{ note.numero }}</span>
                <span class="text-sm text-gray-500">Série {{ note.serie }}</span>
                <span class="text-sm text-gray-500">{{ formatDate(note.dataEmissao) }}</span>
                <span class="px-2 py-1 text-xs rounded-full" :class="getStatusClass(note.status)">
                  {{ note.status }}
                </span>
              </div>
              
              <div class="text-sm text-gray-600 mb-2">
                <div><strong>Emitente:</strong> {{ note.emitente.razaoSocial }} ({{ note.emitente.cnpj }})</div>
                <div><strong>Destinatário:</strong> {{ note.destinatario.razaoSocial }} ({{ note.destinatario.cnpj }})</div>
                <div><strong>Valor Total:</strong> {{ formatCurrency(note.valorTotal) }}</div>
              </div>
              
              <div class="text-sm text-gray-500">
                <strong>Chave:</strong> {{ note.chave }}
              </div>
            </div>
            
            <div class="ml-4">
              <button
                @click.stop="viewNoteDetails(note)"
                class="text-blue-600 hover:text-blue-800 text-sm font-medium"
              >
                Ver Detalhes
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Nota selecionada -->
    <div v-if="selectedNote" class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Nota Selecionada</h4>
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex justify-between items-start mb-4">
          <div>
            <h5 class="font-medium text-blue-900">NF-e {{ selectedNote.numero }} - Série {{ selectedNote.serie }}</h5>
            <p class="text-sm text-blue-700">{{ formatDate(selectedNote.dataEmissao) }}</p>
          </div>
          <button
            @click="clearSelection"
            class="text-blue-600 hover:text-blue-800"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <div>
            <h6 class="font-medium text-blue-900 mb-2">Emitente</h6>
            <p class="text-sm text-blue-700">{{ selectedNote.emitente.razaoSocial }}</p>
            <p class="text-sm text-blue-600">{{ selectedNote.emitente.cnpj }}</p>
          </div>
          <div>
            <h6 class="font-medium text-blue-900 mb-2">Destinatário</h6>
            <p class="text-sm text-blue-700">{{ selectedNote.destinatario.razaoSocial }}</p>
            <p class="text-sm text-blue-600">{{ selectedNote.destinatario.cnpj }}</p>
          </div>
        </div>
        
        <div class="mb-4">
          <h6 class="font-medium text-blue-900 mb-2">Itens da Nota</h6>
          <div class="space-y-2">
            <div
              v-for="(item, index) in selectedNote.itens"
              :key="index"
              class="flex justify-between items-center text-sm"
            >
              <div class="flex-1">
                <span class="font-medium">{{ item.descricao }}</span>
                <span class="text-gray-500 ml-2">({{ item.quantidade }} {{ item.unidade }})</span>
              </div>
              <div class="text-right">
                <div>{{ formatCurrency(item.valorUnitario) }} × {{ item.quantidade }}</div>
                <div class="font-medium">{{ formatCurrency(item.valorTotal) }}</div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="flex justify-between items-center">
          <div class="text-sm">
            <span class="font-medium text-blue-900">Valor Total: </span>
            <span class="text-lg font-bold text-blue-900">{{ formatCurrency(selectedNote.valorTotal) }}</span>
          </div>
          <button
            @click="useSelectedNote"
            class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
          >
            Usar Esta Nota
          </button>
        </div>
      </div>
    </div>

    <!-- Mensagens de erro -->
    <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-4">
      <div class="flex items-center">
        <svg class="w-5 h-5 text-red-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
        </svg>
        <span class="text-red-700">{{ error }}</span>
      </div>
    </div>

    <!-- Mensagem quando não há resultados -->
    <div v-if="searchPerformed && searchResults.length === 0" class="text-center py-8">
      <svg class="w-12 h-12 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
      </svg>
      <p class="text-gray-500">Nenhuma nota encontrada com os critérios informados.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import api from '@/services/api'

const props = defineProps({
  operationType: { type: String, default: 'devolucao' } // devolucao, credito
})

const emit = defineEmits(['note-selected'])

const searchType = ref('chave')
const searchValue = ref('')
const dateFrom = ref('')
const dateTo = ref('')
const loading = ref(false)
const searchResults = ref([])
const selectedNote = ref(null)
const error = ref('')
const searchPerformed = ref(false)

const canSearch = computed(() => {
  switch (searchType.value) {
    case 'chave':
      return searchValue.value.length === 44
    case 'numero':
      return searchValue.value.length > 0
    case 'cnpj':
      return searchValue.value.length >= 14
    case 'periodo':
      return dateFrom.value && dateTo.value
    default:
      return false
  }
})

const searchNotes = async () => {
  if (!canSearch.value) return
  
  loading.value = true
  error.value = ''
  searchResults.value = []
  searchPerformed.value = true
  
  try {
    const params = {
      tipo: searchType.value,
      operacao: props.operationType
    }
    
    switch (searchType.value) {
      case 'chave':
        params.chave = searchValue.value
        break
      case 'numero':
        params.numero = searchValue.value
        break
      case 'cnpj':
        params.cnpj = searchValue.value.replace(/\D/g, '')
        break
      case 'periodo':
        params.dataInicial = dateFrom.value
        params.dataFinal = dateTo.value
        break
    }
    
    const response = await api.get('/nfe/buscar', { params })
    searchResults.value = response.data
  } catch (err) {
    error.value = err.response?.data?.message || 'Erro ao buscar notas'
    console.error('Erro ao buscar notas:', err)
  } finally {
    loading.value = false
  }
}

const selectNote = (note) => {
  selectedNote.value = note
}

const clearSelection = () => {
  selectedNote.value = null
}

const useSelectedNote = () => {
  if (selectedNote.value) {
    emit('note-selected', selectedNote.value)
  }
}

const viewNoteDetails = (note) => {
  // Implementar visualização de detalhes da nota
  console.log('Ver detalhes da nota:', note)
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('pt-BR')
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(value || 0)
}

const getStatusClass = (status) => {
  switch (status.toLowerCase()) {
    case 'autorizada':
      return 'bg-green-100 text-green-800'
    case 'cancelada':
      return 'bg-red-100 text-red-800'
    case 'rejeitada':
      return 'bg-red-100 text-red-800'
    case 'inutilizada':
      return 'bg-gray-100 text-gray-800'
    default:
      return 'bg-yellow-100 text-yellow-800'
  }
}
</script>

<style scoped>
/* Estilos específicos do componente */
</style>
