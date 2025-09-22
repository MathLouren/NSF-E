<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Notas Emitidas</h1>
    <div class="bg-white p-6 rounded-lg shadow-md">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID (Número)</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Prestador</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tomador</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Valor do Serviço</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Data de Emissão</th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="nota in nfses" :key="nota.id">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
              <RouterLink :to="`/nfse/${nota.id}`" class="text-blue-600 underline">
                {{ nota.id }}
              </RouterLink>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ nota.providerLegalName }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ nota.recipientLegalName }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">R$ {{ nota.serviceValue?.toFixed(2) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ new Date(nota.issueDate).toLocaleDateString() }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed } from 'vue'
import { useNfseStore } from '@/store/nfse'

const nfseStore = useNfseStore()

onMounted(() => {
  nfseStore.fetchNfses()
})

const nfses = computed(() => nfseStore.nfses)
</script>

<style scoped>
/* Adicione estilos específicos para esta página aqui */
</style>