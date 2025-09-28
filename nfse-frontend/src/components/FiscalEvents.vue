<template>
  <div class="bg-white border rounded-lg p-6">
    <h3 class="text-lg font-semibold text-gray-900 mb-4">Eventos Fiscais Especiais</h3>
    
    <!-- Seleção do tipo de evento -->
    <div class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Tipo de Evento</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <label
          v-for="eventType in eventTypes"
          :key="eventType.value"
          class="relative cursor-pointer"
        >
          <input
            v-model="selectedEventType"
            :value="eventType.value"
            type="radio"
            class="sr-only"
            @change="onEventTypeChange"
          />
          <div
            class="border-2 rounded-lg p-4 text-center transition-colors"
            :class="selectedEventType === eventType.value
              ? 'border-blue-500 bg-blue-50'
              : 'border-gray-200 hover:border-gray-300'"
          >
            <div class="text-2xl mb-2">{{ eventType.icon }}</div>
            <div class="font-medium text-gray-900">{{ eventType.label }}</div>
            <div class="text-sm text-gray-500 mt-1">{{ eventType.description }}</div>
          </div>
        </label>
      </div>
    </div>

    <!-- Formulário específico para cada tipo de evento -->
    <div v-if="selectedEventType" class="space-y-6">
      
      <!-- Crédito Presumido -->
      <div v-if="selectedEventType === 'credito-presumido'" class="border border-gray-200 rounded-lg p-4">
        <h5 class="font-medium text-gray-900 mb-3">Crédito Presumido</h5>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FiscalField
            label="Chave de Acesso da NF-e Original"
            v-model="eventData.chaveAcesso"
            type="chaveAcesso"
            :required="true"
            help-text="Chave de acesso da NF-e que gerou o crédito"
          />
          <FiscalField
            label="Número do Item"
            v-model="eventData.numeroItem"
            input-type="number"
            :required="true"
            help-text="Número do item da NF-e original"
          />
          <FiscalField
            label="Valor do Crédito"
            v-model="eventData.valorCredito"
            input-type="number"
            :required="true"
            help-text="Valor do crédito presumido"
          />
          <FiscalField
            label="Justificativa"
            v-model="eventData.justificativa"
            type="text"
            :required="true"
            help-text="Justificativa para o crédito presumido"
          />
        </div>
      </div>

      <!-- Cancelamento -->
      <div v-if="selectedEventType === 'cancelamento'" class="border border-gray-200 rounded-lg p-4">
        <h5 class="font-medium text-gray-900 mb-3">Cancelamento de NF-e</h5>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FiscalField
            label="Chave de Acesso da NF-e"
            v-model="eventData.chaveAcesso"
            type="chaveAcesso"
            :required="true"
            help-text="Chave de acesso da NF-e a ser cancelada"
          />
          <FiscalField
            label="Protocolo de Autorização"
            v-model="eventData.protocoloAutorizacao"
            type="text"
            :required="true"
            help-text="Protocolo de autorização da NF-e"
          />
          <div class="md:col-span-2">
            <FiscalField
              label="Justificativa do Cancelamento"
              v-model="eventData.justificativa"
              type="text"
              :required="true"
              help-text="Justificativa obrigatória para o cancelamento"
            />
          </div>
        </div>
      </div>

      <!-- Manifestação do Destinatário -->
      <div v-if="selectedEventType === 'manifestacao'" class="border border-gray-200 rounded-lg p-4">
        <h5 class="font-medium text-gray-900 mb-3">Manifestação do Destinatário</h5>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FiscalField
            label="Chave de Acesso da NF-e"
            v-model="eventData.chaveAcesso"
            type="chaveAcesso"
            :required="true"
            help-text="Chave de acesso da NF-e"
          />
          <FiscalField
            label="Tipo de Manifestação"
            v-model="eventData.tipoManifestacao"
            type="tipoManifestacao"
            :required="true"
            help-text="Tipo de manifestação do destinatário"
          />
          <div v-if="eventData.tipoManifestacao === '210240'" class="md:col-span-2">
            <FiscalField
              label="Justificativa"
              v-model="eventData.justificativa"
              type="text"
              :required="true"
              help-text="Justificativa para operação não realizada"
            />
          </div>
        </div>
      </div>

      <!-- Carta de Correção Eletrônica -->
      <div v-if="selectedEventType === 'cce'" class="border border-gray-200 rounded-lg p-4">
        <h5 class="font-medium text-gray-900 mb-3">Carta de Correção Eletrônica</h5>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FiscalField
            label="Chave de Acesso da NF-e"
            v-model="eventData.chaveAcesso"
            type="chaveAcesso"
            :required="true"
            help-text="Chave de acesso da NF-e a ser corrigida"
          />
          <FiscalField
            label="Protocolo de Autorização"
            v-model="eventData.protocoloAutorizacao"
            type="text"
            :required="true"
            help-text="Protocolo de autorização da NF-e"
          />
          <div class="md:col-span-2">
            <FiscalField
              label="Correção"
              v-model="eventData.correcao"
              type="text"
              :required="true"
              help-text="Descrição da correção a ser aplicada"
            />
          </div>
          <div class="md:col-span-2">
            <FiscalField
              label="Condições de Uso"
              v-model="eventData.condicoesUso"
              type="text"
              :required="true"
              help-text="Condições de uso da correção"
            />
          </div>
        </div>
      </div>

      <!-- Inutilização -->
      <div v-if="selectedEventType === 'inutilizacao'" class="border border-gray-200 rounded-lg p-4">
        <h5 class="font-medium text-gray-900 mb-3">Inutilização de Numeração</h5>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FiscalField
            label="Ano"
            v-model="eventData.ano"
            input-type="number"
            :required="true"
            help-text="Ano da numeração a ser inutilizada"
          />
          <FiscalField
            label="CNPJ"
            v-model="eventData.cnpj"
            type="cnpj"
            :required="true"
            help-text="CNPJ do emitente"
          />
          <FiscalField
            label="Modelo"
            v-model="eventData.modelo"
            type="modelo"
            :required="true"
            help-text="Modelo do documento (55=NF-e)"
          />
          <FiscalField
            label="Série"
            v-model="eventData.serie"
            input-type="number"
            :required="true"
            help-text="Série da numeração"
          />
          <FiscalField
            label="Número Inicial"
            v-model="eventData.numeroInicial"
            input-type="number"
            :required="true"
            help-text="Número inicial da sequência"
          />
          <FiscalField
            label="Número Final"
            v-model="eventData.numeroFinal"
            input-type="number"
            :required="true"
            help-text="Número final da sequência"
          />
          <div class="md:col-span-2">
            <FiscalField
              label="Justificativa"
              v-model="eventData.justificativa"
              type="text"
              :required="true"
              help-text="Justificativa para a inutilização"
            />
          </div>
        </div>
      </div>

      <!-- Botões de ação -->
      <div class="flex justify-end space-x-4">
        <button
          @click="validarEvento"
          class="bg-yellow-600 text-white px-4 py-2 rounded-lg hover:bg-yellow-700"
        >
          Validar Evento
        </button>
        <button
          @click="enviarEvento"
          :disabled="loading"
          class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50"
        >
          <span v-if="loading">Enviando...</span>
          <span v-else>Enviar Evento</span>
        </button>
      </div>
    </div>

    <!-- Resultado do evento -->
    <div v-if="resultado" class="mt-6">
      <div v-if="resultado.sucesso" class="bg-green-50 border border-green-200 rounded-lg p-4">
        <div class="flex items-center">
          <svg class="h-5 w-5 text-green-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
          </svg>
          <div>
            <h4 class="text-sm font-medium text-green-800">Evento Processado com Sucesso!</h4>
            <div class="mt-1 text-sm text-green-700">
              <p><strong>Protocolo:</strong> {{ resultado.protocolo }}</p>
              <p><strong>Data/Hora:</strong> {{ formatarData(resultado.dataHora) }}</p>
              <p v-if="resultado.numeroSequencial"><strong>Número Sequencial:</strong> {{ resultado.numeroSequencial }}</p>
            </div>
          </div>
        </div>
      </div>
      
      <div v-else class="bg-red-50 border border-red-200 rounded-lg p-4">
        <div class="flex items-center">
          <svg class="h-5 w-5 text-red-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
          </svg>
          <div>
            <h4 class="text-sm font-medium text-red-800">Erro no Processamento</h4>
            <div class="mt-1 text-sm text-red-700">
              <p><strong>Mensagem:</strong> {{ resultado.mensagem }}</p>
              <p v-if="resultado.codigoStatus"><strong>Código:</strong> {{ resultado.codigoStatus }}</p>
              <div v-if="resultado.erros && resultado.erros.length > 0" class="mt-2">
                <p><strong>Detalhes:</strong></p>
                <ul class="list-disc list-inside">
                  <li v-for="erro in resultado.erros" :key="erro">{{ erro }}</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Histórico de eventos -->
    <div v-if="eventosHistorico.length > 0" class="mt-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Histórico de Eventos</h4>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Data/Hora</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tipo</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Chave</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Protocolo</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="evento in eventosHistorico" :key="evento.id">
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {{ formatarData(evento.dataHora) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {{ evento.tipo }}
              </td>
              <td class="px-6 py-4 text-sm text-gray-900">
                {{ evento.chaveAcesso }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 py-1 text-xs rounded-full"
                  :class="evento.status === 'processado' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                >
                  {{ evento.status }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {{ evento.protocolo }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '@/services/api'
import FiscalField from '@/components/FiscalField.vue'

const selectedEventType = ref('')
const loading = ref(false)
const resultado = ref(null)
const eventosHistorico = ref([])

// Dados do evento
const eventData = reactive({
  chaveAcesso: '',
  protocoloAutorizacao: '',
  justificativa: '',
  correcao: '',
  condicoesUso: '',
  numeroItem: '',
  valorCredito: '',
  tipoManifestacao: '',
  ano: new Date().getFullYear(),
  cnpj: '',
  modelo: '55',
  serie: '',
  numeroInicial: '',
  numeroFinal: ''
})

// Tipos de eventos disponíveis
const eventTypes = [
  {
    value: 'credito-presumido',
    label: 'Crédito Presumido',
    icon: '💳',
    description: 'Registrar crédito presumido'
  },
  {
    value: 'cancelamento',
    label: 'Cancelamento',
    icon: '❌',
    description: 'Cancelar NF-e'
  },
  {
    value: 'manifestacao',
    label: 'Manifestação',
    icon: '📝',
    description: 'Manifestação do destinatário'
  },
  {
    value: 'cce',
    label: 'Carta de Correção',
    icon: '📄',
    description: 'Corrigir dados da NF-e'
  },
  {
    value: 'inutilizacao',
    label: 'Inutilização',
    icon: '🗑️',
    description: 'Inutilizar numeração'
  }
]

const onEventTypeChange = () => {
  // Limpar dados do evento anterior
  Object.keys(eventData).forEach(key => {
    eventData[key] = ''
  })
  
  // Definir valores padrão
  eventData.ano = new Date().getFullYear()
  eventData.modelo = '55'
  
  resultado.value = null
}

const validarEvento = () => {
  const errors = []
  
  // Validações comuns
  if (!selectedEventType.value) {
    errors.push('Tipo de evento é obrigatório')
  }
  
  // Validações específicas por tipo
  switch (selectedEventType.value) {
    case 'credito-presumido':
      if (!eventData.chaveAcesso) errors.push('Chave de acesso é obrigatória')
      if (!eventData.numeroItem) errors.push('Número do item é obrigatório')
      if (!eventData.valorCredito) errors.push('Valor do crédito é obrigatório')
      if (!eventData.justificativa) errors.push('Justificativa é obrigatória')
      break
      
    case 'cancelamento':
      if (!eventData.chaveAcesso) errors.push('Chave de acesso é obrigatória')
      if (!eventData.protocoloAutorizacao) errors.push('Protocolo de autorização é obrigatório')
      if (!eventData.justificativa) errors.push('Justificativa é obrigatória')
      break
      
    case 'manifestacao':
      if (!eventData.chaveAcesso) errors.push('Chave de acesso é obrigatória')
      if (!eventData.tipoManifestacao) errors.push('Tipo de manifestação é obrigatório')
      if (eventData.tipoManifestacao === '210240' && !eventData.justificativa) {
        errors.push('Justificativa é obrigatória para operação não realizada')
      }
      break
      
    case 'cce':
      if (!eventData.chaveAcesso) errors.push('Chave de acesso é obrigatória')
      if (!eventData.protocoloAutorizacao) errors.push('Protocolo de autorização é obrigatório')
      if (!eventData.correcao) errors.push('Correção é obrigatória')
      if (!eventData.condicoesUso) errors.push('Condições de uso são obrigatórias')
      break
      
    case 'inutilizacao':
      if (!eventData.ano) errors.push('Ano é obrigatório')
      if (!eventData.cnpj) errors.push('CNPJ é obrigatório')
      if (!eventData.modelo) errors.push('Modelo é obrigatório')
      if (!eventData.serie) errors.push('Série é obrigatória')
      if (!eventData.numeroInicial) errors.push('Número inicial é obrigatório')
      if (!eventData.numeroFinal) errors.push('Número final é obrigatório')
      if (!eventData.justificativa) errors.push('Justificativa é obrigatória')
      break
  }
  
  if (errors.length > 0) {
    alert('Erros encontrados:\n' + errors.join('\n'))
  } else {
    alert('Evento validado com sucesso!')
  }
}

const enviarEvento = async () => {
  loading.value = true
  resultado.value = null
  
  try {
    const payload = {
      tipoEvento: selectedEventType.value,
      dados: { ...eventData }
    }
    
    const response = await api.post('/nfe/evento', payload)
    resultado.value = response.data
    
    // Adicionar ao histórico
    eventosHistorico.value.unshift({
      id: Date.now(),
      tipo: selectedEventType.value,
      chaveAcesso: eventData.chaveAcesso,
      status: response.data.sucesso ? 'processado' : 'erro',
      protocolo: response.data.protocolo || '',
      dataHora: new Date()
    })
    
  } catch (error) {
    resultado.value = {
      sucesso: false,
      mensagem: error.response?.data?.message || error.message
    }
  } finally {
    loading.value = false
  }
}

const formatarData = (data) => {
  if (!data) return ''
  return new Date(data).toLocaleString('pt-BR')
}

// Carregar histórico de eventos na inicialização
onMounted(async () => {
  try {
    const response = await api.get('/nfe/eventos/historico')
    eventosHistorico.value = response.data
  } catch (error) {
    console.error('Erro ao carregar histórico de eventos:', error)
  }
})
</script>

<style scoped>
/* Estilos específicos do componente */
</style>
