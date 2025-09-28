<template>
  <div class="bg-white border rounded-lg p-6">
    <h3 class="text-lg font-semibold text-gray-900 mb-4">Totais dos Impostos</h3>
    
    <!-- Resumo por UF -->
    <div v-if="totalsByUF.length > 0" class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Por UF</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="ufTotal in totalsByUF"
          :key="ufTotal.uf"
          class="border rounded-lg p-4"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="font-medium text-gray-900">{{ ufTotal.uf }}</span>
            <span class="text-sm text-gray-500">{{ ufTotal.items }} item(s)</span>
          </div>
          
          <div class="space-y-2">
            <div v-if="ufTotal.ibs > 0" class="flex justify-between text-sm">
              <span class="text-gray-600">IBS:</span>
              <span class="font-medium">{{ formatCurrency(ufTotal.ibs) }}</span>
            </div>
            <div v-if="ufTotal.cbs > 0" class="flex justify-between text-sm">
              <span class="text-gray-600">CBS:</span>
              <span class="font-medium">{{ formatCurrency(ufTotal.cbs) }}</span>
            </div>
            <div v-if="ufTotal.is > 0" class="flex justify-between text-sm">
              <span class="text-gray-600">IS:</span>
              <span class="font-medium">{{ formatCurrency(ufTotal.is) }}</span>
            </div>
            <div class="border-t pt-2 flex justify-between font-medium">
              <span>Subtotal:</span>
              <span>{{ formatCurrency(ufTotal.subtotal) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Resumo por Município -->
    <div v-if="totalsByMunicipio.length > 0" class="mb-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Por Município</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="municipioTotal in totalsByMunicipio"
          :key="municipioTotal.municipio"
          class="border rounded-lg p-4"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="font-medium text-gray-900">{{ municipioTotal.municipio }}</span>
            <span class="text-sm text-gray-500">{{ municipioTotal.uf }}</span>
          </div>
          
          <div class="space-y-2">
            <div v-if="municipioTotal.ibs > 0" class="flex justify-between text-sm">
              <span class="text-gray-600">IBS:</span>
              <span class="font-medium">{{ formatCurrency(municipioTotal.ibs) }}</span>
            </div>
            <div v-if="municipioTotal.cbs > 0" class="flex justify-between text-sm">
              <span class="text-gray-600">CBS:</span>
              <span class="font-medium">{{ formatCurrency(municipioTotal.cbs) }}</span>
            </div>
            <div v-if="municipioTotal.is > 0" class="flex justify-between text-sm">
              <span class="text-gray-600">IS:</span>
              <span class="font-medium">{{ formatCurrency(municipioTotal.is) }}</span>
            </div>
            <div class="border-t pt-2 flex justify-between font-medium">
              <span>Subtotal:</span>
              <span>{{ formatCurrency(municipioTotal.subtotal) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Totais Gerais -->
    <div class="border-t pt-4">
      <h4 class="text-md font-medium text-gray-700 mb-3">Totais Gerais</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-blue-600">IBS Total</p>
              <p class="text-2xl font-bold text-blue-900">{{ formatCurrency(totalIBS) }}</p>
            </div>
            <div class="bg-blue-100 rounded-full p-2">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"></path>
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-green-50 border border-green-200 rounded-lg p-4">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-green-600">CBS Total</p>
              <p class="text-2xl font-bold text-green-900">{{ formatCurrency(totalCBS) }}</p>
            </div>
            <div class="bg-green-100 rounded-full p-2">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-purple-50 border border-purple-200 rounded-lg p-4">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-purple-600">IS Total</p>
              <p class="text-2xl font-bold text-purple-900">{{ formatCurrency(totalIS) }}</p>
            </div>
            <div class="bg-purple-100 rounded-full p-2">
              <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path>
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">Total Geral</p>
              <p class="text-2xl font-bold text-gray-900">{{ formatCurrency(totalGeral) }}</p>
            </div>
            <div class="bg-gray-100 rounded-full p-2">
              <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"></path>
              </svg>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Detalhamento por Item -->
    <div v-if="showItemDetails" class="mt-6">
      <h4 class="text-md font-medium text-gray-700 mb-3">Detalhamento por Item</h4>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Item</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Descrição</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">UF</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Valor</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">IBS</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">CBS</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">IS</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="(item, index) in items" :key="index">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {{ index + 1 }}
              </td>
              <td class="px-6 py-4 text-sm text-gray-900">
                {{ item.descricao }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ item.uf }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {{ formatCurrency(item.valor) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-blue-600">
                {{ formatCurrency(item.ibs) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-green-600">
                {{ formatCurrency(item.cbs) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-purple-600">
                {{ formatCurrency(item.is) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {{ formatCurrency(item.total) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Botões de ação -->
    <div class="mt-6 flex justify-between">
      <button
        @click="toggleItemDetails"
        class="text-blue-600 hover:text-blue-800 text-sm font-medium"
      >
        {{ showItemDetails ? 'Ocultar' : 'Mostrar' }} detalhamento
      </button>
      
      <div class="space-x-2">
        <button
          @click="exportTotals"
          class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 text-sm"
        >
          Exportar Totais
        </button>
        <button
          @click="printTotals"
          class="bg-gray-600 text-white px-4 py-2 rounded-lg hover:bg-gray-700 text-sm"
        >
          Imprimir
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  items: { type: Array, default: () => [] },
  emitenteUF: { type: String, default: '' },
  destinatarioUF: { type: String, default: '' }
})

const showItemDetails = ref(false)

// Calcula totais por UF
const totalsByUF = computed(() => {
  const ufTotals = {}
  
  props.items.forEach(item => {
    const uf = item.uf || props.destinatarioUF
    if (!ufTotals[uf]) {
      ufTotals[uf] = {
        uf,
        items: 0,
        ibs: 0,
        cbs: 0,
        is: 0,
        subtotal: 0
      }
    }
    
    ufTotals[uf].items++
    ufTotals[uf].ibs += item.ibs || 0
    ufTotals[uf].cbs += item.cbs || 0
    ufTotals[uf].is += item.is || 0
    ufTotals[uf].subtotal += (item.ibs || 0) + (item.cbs || 0) + (item.is || 0)
  })
  
  return Object.values(ufTotals)
})

// Calcula totais por município
const totalsByMunicipio = computed(() => {
  const municipioTotals = {}
  
  props.items.forEach(item => {
    const municipio = item.municipio || 'Não informado'
    const uf = item.uf || props.destinatarioUF
    
    if (!municipioTotals[municipio]) {
      municipioTotals[municipio] = {
        municipio,
        uf,
        ibs: 0,
        cbs: 0,
        is: 0,
        subtotal: 0
      }
    }
    
    municipioTotals[municipio].ibs += item.ibs || 0
    municipioTotals[municipio].cbs += item.cbs || 0
    municipioTotals[municipio].is += item.is || 0
    municipioTotals[municipio].subtotal += (item.ibs || 0) + (item.cbs || 0) + (item.is || 0)
  })
  
  return Object.values(municipioTotals)
})

// Totais gerais
const totalIBS = computed(() => {
  return props.items.reduce((total, item) => total + (item.ibs || 0), 0)
})

const totalCBS = computed(() => {
  return props.items.reduce((total, item) => total + (item.cbs || 0), 0)
})

const totalIS = computed(() => {
  return props.items.reduce((total, item) => total + (item.is || 0), 0)
})

const totalGeral = computed(() => {
  return totalIBS.value + totalCBS.value + totalIS.value
})

const formatCurrency = (value) => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(value || 0)
}

const toggleItemDetails = () => {
  showItemDetails.value = !showItemDetails.value
}

const exportTotals = () => {
  const data = {
    totalsByUF: totalsByUF.value,
    totalsByMunicipio: totalsByMunicipio.value,
    totalIBS: totalIBS.value,
    totalCBS: totalCBS.value,
    totalIS: totalIS.value,
    totalGeral: totalGeral.value,
    items: props.items,
    generatedAt: new Date().toISOString()
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `totais-impostos-${new Date().toISOString().split('T')[0]}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

const printTotals = () => {
  window.print()
}
</script>

<style scoped>
@media print {
  .no-print {
    display: none !important;
  }
}
</style>
