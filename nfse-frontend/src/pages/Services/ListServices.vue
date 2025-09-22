<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Lista de Serviços</h1>
    <div class="bg-white p-6 rounded-lg shadow-md">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Código</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Descrição</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Valor</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Quantidade</th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Alíquota IBS (%)</th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="service in services" :key="service.code">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ service.code }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ service.description }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ service.value }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ service.quantity }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ service.ibsRate }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useServicesStore } from '@/store/services'

const servicesStore = useServicesStore()

onMounted(() => {
  servicesStore.fetchServices()
})

const services = computed(() => servicesStore.services)
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>
