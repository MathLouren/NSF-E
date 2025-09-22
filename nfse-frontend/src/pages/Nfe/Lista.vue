<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">NF-e Emitidas</h1>
        <p class="text-gray-600 mt-2">Gerencie suas Notas Fiscais Eletrônicas</p>
      </div>

      <!-- Filtros -->
      <div class="bg-white shadow rounded-lg p-6 mb-6">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">Filtros</h2>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Data Início</label>
            <input 
              v-model="filtros.dataInicio"
              type="date" 
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Data Fim</label>
            <input 
              v-model="filtros.dataFim"
              type="date" 
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select 
              v-model="filtros.status"
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos</option>
              <option value="Autorizada">Autorizada</option>
              <option value="Rejeitada">Rejeitada</option>
              <option value="Cancelada">Cancelada</option>
            </select>
          </div>
          <div class="flex items-end">
            <button 
              @click="aplicarFiltros"
              class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              Filtrar
            </button>
          </div>
        </div>
      </div>

      <!-- Lista de NF-e -->
      <div class="bg-white shadow rounded-lg overflow-hidden">
        <div class="px-6 py-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Notas Fiscais</h2>
        </div>
        
        <div v-if="loading" class="p-6 text-center">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p class="mt-2 text-gray-600">Carregando...</p>
        </div>
        
        <div v-else-if="nfeList.length === 0" class="p-6 text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900">Nenhuma NF-e encontrada</h3>
          <p class="mt-1 text-sm text-gray-500">Comece emitindo uma nova NF-e.</p>
        </div>
        
        <div v-else class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Número
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Série
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Destinatário
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Valor
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Data
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="nfe in nfeList" :key="nfe.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {{ nfe.numero }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ nfe.serie }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ nfe.destinatario }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatarMoeda(nfe.valor) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span :class="[
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                    nfe.status === 'Autorizada' ? 'bg-green-100 text-green-800' :
                    nfe.status === 'Rejeitada' ? 'bg-red-100 text-red-800' :
                    nfe.status === 'Cancelada' ? 'bg-gray-100 text-gray-800' :
                    'bg-yellow-100 text-yellow-800'
                  ]">
                    {{ nfe.status }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatarData(nfe.dataEmissao) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <div class="flex space-x-2">
                    <button 
                      @click="visualizarNFe(nfe)"
                      class="text-blue-600 hover:text-blue-900"
                    >
                      Visualizar
                    </button>
                    <button 
                      v-if="nfe.status === 'Autorizada'"
                      @click="gerarDANFE(nfe)"
                      class="text-green-600 hover:text-green-900"
                    >
                      DANFE
                    </button>
                    <button 
                      v-if="nfe.status === 'Autorizada'"
                      @click="cancelarNFe(nfe)"
                      class="text-red-600 hover:text-red-900"
                    >
                      Cancelar
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Paginação -->
      <div v-if="nfeList.length > 0" class="mt-6 flex items-center justify-between">
        <div class="text-sm text-gray-700">
          Mostrando {{ nfeList.length }} de {{ totalRegistros }} registros
        </div>
        <div class="flex space-x-2">
          <button 
            @click="paginaAnterior"
            :disabled="paginaAtual === 1"
            class="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
          >
            Anterior
          </button>
          <button 
            @click="proximaPagina"
            :disabled="paginaAtual >= totalPaginas"
            class="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
          >
            Próxima
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'

const router = useRouter()
const loading = ref(false)
const nfeList = ref([])
const totalRegistros = ref(0)
const paginaAtual = ref(1)
const totalPaginas = ref(1)

const filtros = reactive({
  dataInicio: '',
  dataFim: '',
  status: ''
})

// Carregar lista de NF-e
const carregarNFeList = async () => {
  loading.value = true
  
  try {
    const params = {
      pagina: paginaAtual.value,
      limite: 10,
      ...filtros
    }
    
    const { data } = await api.get('/NFe/lista', { params })
    nfeList.value = data.itens || []
    totalRegistros.value = data.total || 0
    totalPaginas.value = Math.ceil(totalRegistros.value / 10)
  } catch (error) {
    console.error('Erro ao carregar lista de NF-e:', error)
    nfeList.value = []
  } finally {
    loading.value = false
  }
}

// Aplicar filtros
const aplicarFiltros = () => {
  paginaAtual.value = 1
  carregarNFeList()
}

// Paginação
const paginaAnterior = () => {
  if (paginaAtual.value > 1) {
    paginaAtual.value--
    carregarNFeList()
  }
}

const proximaPagina = () => {
  if (paginaAtual.value < totalPaginas.value) {
    paginaAtual.value++
    carregarNFeList()
  }
}

// Visualizar NF-e
const visualizarNFe = (nfe) => {
  router.push(`/nfe/${nfe.id}`)
}

// Gerar DANFE
const gerarDANFE = async (nfe) => {
  try {
    const response = await api.get(`/NFe/danfe/${nfe.protocolo}`, {
      responseType: 'blob'
    })
    
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `DANFE-${nfe.numero}.pdf`)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    console.error('Erro ao gerar DANFE:', error)
    alert('Erro ao gerar DANFE')
  }
}

// Cancelar NF-e
const cancelarNFe = async (nfe) => {
  if (!confirm('Tem certeza que deseja cancelar esta NF-e?')) {
    return
  }
  
  try {
    const justificativa = prompt('Digite a justificativa para o cancelamento:')
    if (!justificativa) return
    
    await api.post('/NFe/cancelar', {
      chaveAcesso: nfe.chaveAcesso,
      justificativa: justificativa
    })
    
    alert('NF-e cancelada com sucesso!')
    carregarNFeList()
  } catch (error) {
    console.error('Erro ao cancelar NF-e:', error)
    alert('Erro ao cancelar NF-e: ' + (error.response?.data?.mensagem || error.message))
  }
}

// Formatar moeda
const formatarMoeda = (valor) => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(valor)
}

// Formatar data
const formatarData = (data) => {
  if (!data) return ''
  return new Date(data).toLocaleDateString('pt-BR')
}

// Carregar dados na inicialização
onMounted(() => {
  carregarNFeList()
})
</script>
