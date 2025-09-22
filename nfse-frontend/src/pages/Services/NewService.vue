<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Adicionar Novo Serviço</h1>
    <form @submit.prevent="submitService" class="bg-white p-6 rounded-lg shadow-md">
      <h2 class="text-xl font-semibold mb-3">Dados do Serviço</h2>
      <InputField label="Código do Serviço (CNAE ou Tabela Nacional)" v-model="service.code" class="mb-2" />
      <InputField label="Descrição do Serviço" v-model="service.description" type="textarea" class="mb-2" />
      <InputField label="Valor do Serviço" v-model.number="service.value" type="number" step="0.01" min="0" class="mb-2" />
      <InputField label="Quantidade" v-model.number="service.quantity" type="number" min="1" class="mb-4" />

      <h2 class="text-xl font-semibold mb-3">Dados dos Impostos</h2>
      <InputField label="Alíquota IBS (%)" v-model.number="service.ibsRate" type="number" step="0.01" min="0" max="100" class="mb-2" />
      <!-- Other applicable taxes (ISS, Retenções) can be added here -->

      <button type="submit" class="mt-4 w-full bg-blue-600 text-white py-2 rounded-lg">Salvar Serviço</button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router';
import InputField from '@/components/InputField.vue'
import { useServicesStore } from '@/store/services'

const servicesStore = useServicesStore()
const router = useRouter();

const service = ref({
  code: '',
  description: '',
  value: 0,
  quantity: 1,
  ibsRate: 0,
  // Other taxes can be initialized here
})

const submitService = async () => {
  console.log('Service Data:', service.value)
  await servicesStore.createService(service.value)
  router.push('/services/list');
}
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>
