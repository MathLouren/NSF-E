<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Dashboard</h1>
    <p class="mt-4 mb-6 text-gray-700">Bem-vindo ao sistema de emissão de NFS-e e NF-e 2026 (Produtos + Serviços).</p>

    <!-- Status do Sistema -->
    <div class="mb-6">
      <div class="bg-white p-4 rounded-lg shadow-md">
        <h2 class="text-lg font-semibold mb-3">Status do Sistema</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="flex items-center">
            <div :class="[
              'w-3 h-3 rounded-full mr-2',
              nfeStore.isHomologacao ? 'bg-yellow-400' : 'bg-green-400'
            ]"></div>
            <span class="text-sm">
              <strong>Ambiente NF-e:</strong> {{ nfeStore.isHomologacao ? 'Homologação' : 'Produção' }}
            </span>
          </div>
          <div class="flex items-center">
            <div :class="[
              'w-3 h-3 rounded-full mr-2',
              nfeStore.certificadoValido ? 'bg-green-400' : 'bg-red-400'
            ]"></div>
            <span class="text-sm">
              <strong>Certificado:</strong> {{ nfeStore.certificadoValido ? 'Válido' : 'Não configurado' }}
            </span>
          </div>
          <div class="flex items-center">
            <div class="w-3 h-3 rounded-full mr-2 bg-green-400"></div>
            <span class="text-sm">
              <strong>Sistema:</strong> Online
            </span>
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <!-- NFS-e -->
      <div class="bg-white p-6 rounded-lg shadow-md">
        <h2 class="text-xl font-semibold mb-3 text-blue-600">Resumo NFS-e</h2>
        <p class="text-gray-700 mb-2">Total de NFS-e Emitidas: <span class="font-bold">150</span></p>
        <p class="text-gray-700 mb-2">NFS-e Pendentes: <span class="font-bold">5</span></p>
        <p class="text-gray-700">Valor Total Emitido: <span class="font-bold">R$ 250.000,00</span></p>
      </div>
      
      <!-- NF-e -->
      <div class="bg-white p-6 rounded-lg shadow-md">
        <h2 class="text-xl font-semibold mb-3 text-orange-600">Resumo NF-e</h2>
        <p class="text-gray-700 mb-2">Total de NF-e Emitidas: <span class="font-bold">{{ nfeStats.total }}</span></p>
        <p class="text-gray-700 mb-2">NF-e Autorizadas: <span class="font-bold text-green-600">{{ nfeStats.autorizadas }}</span></p>
        <p class="text-gray-700">Valor Total Emitido: <span class="font-bold">{{ formatarMoeda(nfeStats.valorTotal) }}</span></p>
      </div>
      
      <!-- Produtos -->
      <div class="bg-white p-6 rounded-lg shadow-md">
        <h2 class="text-xl font-semibold mb-3 text-green-600">Visão Geral de Produtos</h2>
        <p class="text-gray-700 mb-2">Total de Produtos Cadastrados: <span class="font-bold">45</span></p>
        <p class="text-gray-700 mb-2">Produtos em Estoque Baixo: <span class="font-bold text-red-500">3</span></p>
        <p class="text-gray-700">Último Produto Adicionado: <span class="font-bold">Notebook Ultra</span></p>
      </div>
      
      <!-- Serviços -->
      <div class="bg-white p-6 rounded-lg shadow-md">
        <h2 class="text-xl font-semibold mb-3 text-purple-600">Visão Geral de Serviços</h2>
        <p class="text-gray-700 mb-2">Total de Serviços Cadastrados: <span class="font-bold">20</span></p>
        <p class="text-gray-700 mb-2">Serviços Mais Contratados: <span class="font-bold">Consultoria</span></p>
        <p class="text-gray-700">Próximo Serviço Agendado: <span class="font-bold">25/09/2025</span></p>
      </div>
    </div>

    <!-- Ações Rápidas -->
    <div class="bg-white p-6 rounded-lg shadow-md mb-8">
      <h2 class="text-xl font-semibold mb-3">Ações Rápidas</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-4">
        <router-link to="/nfse/nova" class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded text-center">Emitir NFS-e</router-link>
        <router-link to="/nfe/emitir" class="bg-orange-500 hover:bg-orange-700 text-white font-bold py-2 px-4 rounded text-center">Emitir NF-e</router-link>
        <router-link to="/nfe/configuracao" class="bg-yellow-500 hover:bg-yellow-700 text-white font-bold py-2 px-4 rounded text-center">Config. NF-e</router-link>
        <router-link to="/products/new" class="bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded text-center">Adicionar Produto</router-link>
        <router-link to="/services/new" class="bg-purple-500 hover:bg-purple-700 text-white font-bold py-2 px-4 rounded text-center">Adicionar Serviço</router-link>
        <router-link to="/configuracoes" class="bg-gray-500 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded text-center">Configurações</router-link>
      </div>
    </div>

    <!-- Últimas NF-e -->
    <div class="bg-white p-6 rounded-lg shadow-md">
      <h2 class="text-xl font-semibold mb-3">Últimas NF-e Emitidas</h2>
      <div v-if="nfeStore.loading" class="text-center py-4">
        <div class="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-orange-600"></div>
        <p class="mt-2 text-gray-600">Carregando...</p>
      </div>
      <div v-else-if="nfeStore.nfeList.length === 0" class="text-center py-4 text-gray-500">
        Nenhuma NF-e emitida ainda
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Número</th>
              <th class="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Destinatário</th>
              <th class="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Valor</th>
              <th class="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
              <th class="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Data</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="nfe in nfeStore.nfeList.slice(0, 5)" :key="nfe.id">
              <td class="px-4 py-2 text-sm font-medium text-gray-900">{{ nfe.numero }}</td>
              <td class="px-4 py-2 text-sm text-gray-500">{{ nfe.destinatario }}</td>
              <td class="px-4 py-2 text-sm text-gray-500">{{ formatarMoeda(nfe.valor) }}</td>
              <td class="px-4 py-2">
                <span :class="[
                  'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                  nfe.status === 'Autorizada' ? 'bg-green-100 text-green-800' :
                  nfe.status === 'Rejeitada' ? 'bg-red-100 text-red-800' :
                  'bg-yellow-100 text-yellow-800'
                ]">
                  {{ nfe.status }}
                </span>
              </td>
              <td class="px-4 py-2 text-sm text-gray-500">{{ formatarData(nfe.dataEmissao) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="nfeStore.nfeList.length > 0" class="mt-4 text-center">
        <router-link to="/nfe/lista" class="text-orange-600 hover:text-orange-800 text-sm font-medium">
          Ver todas as NF-e →
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useNFeStore } from '@/store/nfe'

const nfeStore = useNFeStore()

// Estatísticas mockadas (em produção, viriam do backend)
const nfeStats = ref({
  total: 0,
  autorizadas: 0,
  valorTotal: 0
})

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
onMounted(async () => {
  try {
    // Carregar status do ambiente e certificado
    await Promise.all([
      nfeStore.carregarAmbiente(),
      nfeStore.carregarCertificado(),
      nfeStore.carregarNFeList()
    ])
    
    // Calcular estatísticas
    nfeStats.value = {
      total: nfeStore.nfeList.length,
      autorizadas: nfeStore.nfeList.filter(nfe => nfe.status === 'Autorizada').length,
      valorTotal: nfeStore.nfeList.reduce((total, nfe) => total + (nfe.valor || 0), 0)
    }
  } catch (error) {
    console.error('Erro ao carregar dados do dashboard:', error)
  }
})
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>