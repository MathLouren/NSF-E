<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Detalhes do Serviço</h1>
    <div v-if="service" class="bg-white p-6 rounded-lg shadow-md">
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Código:</label>
        <p class="text-gray-900">{{ service.code }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Descrição:</label>
        <p class="text-gray-900">{{ service.description }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Valor:</label>
        <p class="text-gray-900">{{ service.value }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Quantidade:</label>
        <p class="text-gray-900">{{ service.quantity }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Alíquota IBS (%):</label>
        <p class="text-gray-900">{{ service.ibsRate }}</p>
      </div>
      <button @click="$router.back()" class="mt-4 bg-gray-600 text-white py-2 px-4 rounded-lg">Voltar</button>
    </div>
    <div v-else class="text-red-500">Serviço não encontrado.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
// import { useServicesStore } from '@/store/services' // To be created/used later

const route = useRoute()
const service = ref(null)

onMounted(() => {
  const serviceId = route.params.id
  // In the future, this will fetch from a Pinia store:
  // const servicesStore = useServicesStore()
  // service.value = servicesStore.getServiceById(serviceId)
  
  // Dummy data for now
  const dummyServices = [
    {
      id: '1',
      code: 'SERV001',
      description: 'Desenvolvimento de Software',
      value: 12000.00,
      quantity: 1,
      ibsRate: 5,
    },
    {
      id: '2',
      code: 'SERV002',
      description: 'Consultoria em TI',
      value: 3000.00,
      quantity: 1,
      ibsRate: 5,
    },
  ]
  service.value = dummyServices.find(s => s.id === serviceId)
})
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>
