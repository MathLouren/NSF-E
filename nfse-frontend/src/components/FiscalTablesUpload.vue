<template>
  <div class="bg-white border rounded-lg p-6">
    <h3 class="text-lg font-semibold text-gray-900 mb-4">Gerenciar Tabelas Fiscais</h3>
    
    <!-- Status das tabelas -->
    <div class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Status das Tabelas</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="table in tableStatus"
          :key="table.name"
          class="border rounded-lg p-4"
          :class="table.updated ? 'border-green-200 bg-green-50' : 'border-yellow-200 bg-yellow-50'"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="font-medium text-gray-900">{{ table.label }}</span>
            <span
              class="px-2 py-1 text-xs rounded-full"
              :class="table.updated ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'"
            >
              {{ table.updated ? 'Atualizada' : 'Desatualizada' }}
            </span>
          </div>
          <div class="text-sm text-gray-600">
            <div>Registros: {{ table.records }}</div>
            <div>Última atualização: {{ formatDate(table.lastUpdate) }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Upload de tabelas -->
    <div class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Upload de Tabelas</h4>
      <div class="space-y-4">
        <div
          v-for="tableType in availableTables"
          :key="tableType.value"
          class="border border-gray-200 rounded-lg p-4"
        >
          <div class="flex items-center justify-between mb-3">
            <div>
              <h5 class="font-medium text-gray-900">{{ tableType.label }}</h5>
              <p class="text-sm text-gray-600">{{ tableType.description }}</p>
            </div>
            <div class="flex items-center space-x-2">
              <input
                :ref="`file-${tableType.value}`"
                :id="`file-${tableType.value}`"
                type="file"
                accept=".csv,.xlsx,.json"
                @change="handleFileSelect($event, tableType.value)"
                class="hidden"
              />
              <label
                :for="`file-${tableType.value}`"
                class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 cursor-pointer text-sm"
              >
                Selecionar Arquivo
              </label>
              <button
                v-if="selectedFiles[tableType.value]"
                @click="uploadTable(tableType.value)"
                :disabled="uploading[tableType.value]"
                class="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 disabled:opacity-50 text-sm"
              >
                <span v-if="uploading[tableType.value]">Enviando...</span>
                <span v-else>Enviar</span>
              </button>
            </div>
          </div>
          
          <div v-if="selectedFiles[tableType.value]" class="text-sm text-gray-600">
            <div class="flex items-center">
              <svg class="w-4 h-4 mr-2 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              Arquivo selecionado: {{ selectedFiles[tableType.value].name }}
              ({{ formatFileSize(selectedFiles[tableType.value].size) }})
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Download de templates -->
    <div class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Templates para Download</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="template in templates"
          :key="template.name"
          class="border border-gray-200 rounded-lg p-4"
        >
          <div class="flex items-center justify-between">
            <div>
              <h5 class="font-medium text-gray-900">{{ template.label }}</h5>
              <p class="text-sm text-gray-600">{{ template.description }}</p>
            </div>
            <button
              @click="downloadTemplate(template.name)"
              class="bg-gray-600 text-white px-3 py-2 rounded-lg hover:bg-gray-700 text-sm"
            >
              Download
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Atualização automática -->
    <div class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Atualização Automática</h4>
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex items-center justify-between">
          <div>
            <h5 class="font-medium text-blue-900">Sincronização com SEFAZ</h5>
            <p class="text-sm text-blue-700">Atualizar tabelas automaticamente com dados oficiais</p>
          </div>
          <div class="flex items-center space-x-2">
            <button
              @click="syncWithSefaz"
              :disabled="syncing"
              class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              <span v-if="syncing">Sincronizando...</span>
              <span v-else>Sincronizar</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Log de atividades -->
    <div v-if="activityLog.length > 0">
      <h4 class="text-md font-medium text-gray-700 mb-3">Log de Atividades</h4>
      <div class="bg-gray-50 border border-gray-200 rounded-lg p-4 max-h-64 overflow-y-auto">
        <div
          v-for="(log, index) in activityLog"
          :key="index"
          class="flex items-center py-2 border-b border-gray-200 last:border-b-0"
        >
          <div
            class="w-2 h-2 rounded-full mr-3"
            :class="getLogColor(log.type)"
          ></div>
          <div class="flex-1">
            <div class="text-sm font-medium text-gray-900">{{ log.message }}</div>
            <div class="text-xs text-gray-500">{{ formatDateTime(log.timestamp) }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Mensagens de erro/sucesso -->
    <div v-if="message" class="mt-4">
      <div
        class="p-4 rounded-lg"
        :class="message.type === 'success' ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'"
      >
        <div class="flex items-center">
          <svg
            class="w-5 h-5 mr-2"
            :class="message.type === 'success' ? 'text-green-400' : 'text-red-400'"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              v-if="message.type === 'success'"
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
              clip-rule="evenodd"
            ></path>
            <path
              v-else
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
              clip-rule="evenodd"
            ></path>
          </svg>
          <span
            class="text-sm font-medium"
            :class="message.type === 'success' ? 'text-green-800' : 'text-red-800'"
          >
            {{ message.text }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '@/services/api'
import fiscalTablesService from '@/services/fiscalTablesService'

const selectedFiles = reactive({})
const uploading = reactive({})
const syncing = ref(false)
const message = ref(null)
const activityLog = ref([])

// Status das tabelas
const tableStatus = ref([
  { name: 'cst', label: 'CST', records: 0, lastUpdate: null, updated: false },
  { name: 'cfop', label: 'CFOP', records: 0, lastUpdate: null, updated: false },
  { name: 'ncm', label: 'NCM', records: 0, lastUpdate: null, updated: false },
  { name: 'municipios', label: 'Municípios', records: 0, lastUpdate: null, updated: false },
  { name: 'ibsRates', label: 'Alíquotas IBS', records: 0, lastUpdate: null, updated: false },
  { name: 'cbsRates', label: 'Alíquotas CBS', records: 0, lastUpdate: null, updated: false }
])

// Tabelas disponíveis para upload
const availableTables = [
  {
    value: 'cst',
    label: 'CST (Código de Situação Tributária)',
    description: 'Códigos de situação tributária para produtos e serviços'
  },
  {
    value: 'cfop',
    label: 'CFOP (Código Fiscal de Operações)',
    description: 'Códigos fiscais de operações e prestações'
  },
  {
    value: 'ncm',
    label: 'NCM (Nomenclatura Comum do Mercosul)',
    description: 'Classificação de mercadorias'
  },
  {
    value: 'municipios',
    label: 'Municípios IBGE',
    description: 'Códigos e nomes dos municípios brasileiros'
  },
  {
    value: 'ibsRates',
    label: 'Alíquotas IBS',
    description: 'Alíquotas do Imposto sobre Bens e Serviços por UF'
  },
  {
    value: 'cbsRates',
    label: 'Alíquotas CBS',
    description: 'Alíquotas da Contribuição sobre Bens e Serviços por UF'
  }
]

// Templates disponíveis
const templates = [
  {
    name: 'cst-template',
    label: 'Template CST',
    description: 'Modelo para upload de CST'
  },
  {
    name: 'cfop-template',
    label: 'Template CFOP',
    description: 'Modelo para upload de CFOP'
  },
  {
    name: 'ncm-template',
    label: 'Template NCM',
    description: 'Modelo para upload de NCM'
  },
  {
    name: 'municipios-template',
    label: 'Template Municípios',
    description: 'Modelo para upload de municípios'
  },
  {
    name: 'ibs-rates-template',
    label: 'Template Alíquotas IBS',
    description: 'Modelo para upload de alíquotas IBS'
  },
  {
    name: 'cbs-rates-template',
    label: 'Template Alíquotas CBS',
    description: 'Modelo para upload de alíquotas CBS'
  }
]

const handleFileSelect = (event, tableType) => {
  const file = event.target.files[0]
  if (file) {
    selectedFiles[tableType] = file
    addToLog('info', `Arquivo selecionado para ${tableType}: ${file.name}`)
  }
}

const uploadTable = async (tableType) => {
  if (!selectedFiles[tableType]) return
  
  uploading[tableType] = true
  message.value = null
  
  try {
    const result = await fiscalTablesService.uploadFiscalTable(tableType, selectedFiles[tableType])
    
    message.value = {
      type: 'success',
      text: `Tabela ${tableType} atualizada com sucesso! ${result.records} registros importados.`
    }
    
    addToLog('success', `Tabela ${tableType} atualizada: ${result.records} registros`)
    
    // Atualizar status da tabela
    const table = tableStatus.value.find(t => t.name === tableType)
    if (table) {
      table.records = result.records
      table.lastUpdate = new Date()
      table.updated = true
    }
    
    // Limpar arquivo selecionado
    delete selectedFiles[tableType]
    
  } catch (error) {
    message.value = {
      type: 'error',
      text: `Erro ao atualizar tabela ${tableType}: ${error.message}`
    }
    
    addToLog('error', `Erro ao atualizar ${tableType}: ${error.message}`)
  } finally {
    uploading[tableType] = false
  }
}

const syncWithSefaz = async () => {
  syncing.value = true
  message.value = null
  
  try {
    const result = await fiscalTablesService.syncWithSefaz()
    
    message.value = {
      type: 'success',
      text: result.message
    }
    
    addToLog('success', 'Sincronização com SEFAZ concluída via endpoint DANFE')
    
    // Atualizar status de todas as tabelas
    tableStatus.value.forEach(table => {
      table.updated = true
      table.lastUpdate = new Date()
    })
    
  } catch (error) {
    message.value = {
      type: 'error',
      text: `Erro na sincronização: ${error.message}`
    }
    
    addToLog('error', `Erro na sincronização: ${error.message}`)
  } finally {
    syncing.value = false
  }
}

const downloadTemplate = async (templateName) => {
  try {
    const response = await api.get(`/fiscal-tables/template/${templateName}`, {
      responseType: 'blob'
    })
    
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `${templateName}.csv`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
    
    addToLog('info', `Template ${templateName} baixado`)
    
  } catch (error) {
    message.value = {
      type: 'error',
      text: `Erro ao baixar template: ${error.message}`
    }
  }
}

const addToLog = (type, message) => {
  activityLog.value.unshift({
    type,
    message,
    timestamp: new Date()
  })
  
  // Manter apenas os últimos 50 logs
  if (activityLog.value.length > 50) {
    activityLog.value = activityLog.value.slice(0, 50)
  }
}

const formatDate = (date) => {
  if (!date) return 'Nunca'
  return new Date(date).toLocaleDateString('pt-BR')
}

const formatDateTime = (date) => {
  return new Date(date).toLocaleString('pt-BR')
}

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const getLogColor = (type) => {
  switch (type) {
    case 'success':
      return 'bg-green-500'
    case 'error':
      return 'bg-red-500'
    case 'warning':
      return 'bg-yellow-500'
    default:
      return 'bg-blue-500'
  }
}

// Carregar status das tabelas na inicialização
onMounted(async () => {
  try {
    await fiscalTablesService.loadFiscalTables()
    
    // Atualizar status das tabelas
    tableStatus.value.forEach(table => {
      const tableData = fiscalTablesService.tables[table.name]
      if (tableData) {
        table.records = Array.isArray(tableData) ? tableData.length : Object.keys(tableData).length
        table.updated = fiscalTablesService.isTablesUpdated()
        table.lastUpdate = fiscalTablesService.lastUpdate
      }
    })
    
    addToLog('info', 'Sistema de tabelas fiscais carregado')
    
  } catch (error) {
    addToLog('error', `Erro ao carregar tabelas: ${error.message}`)
  }
})
</script>

<style scoped>
/* Estilos específicos do componente */
</style>
