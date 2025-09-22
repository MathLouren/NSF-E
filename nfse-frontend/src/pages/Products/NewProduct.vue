<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Adicionar Novo Produto</h1>
    <form @submit.prevent="submitProduct" class="bg-white p-6 rounded-lg shadow-md">
      <h2 class="text-xl font-semibold mb-3">Dados do Produto</h2>
      <InputField label="SKU/Código Interno" v-model="product.sku" class="mb-2" />
      <InputField label="Nome/Descrição do Produto" v-model="product.name" class="mb-2" />
      <InputField label="Quantidade" v-model.number="product.quantity" type="number" min="1" class="mb-2" />
      <InputField label="Preço Unitário" v-model.number="product.unitPrice" type="number" step="0.01" min="0.01" class="mb-2" />
      <InputField label="Categoria (Opcional)" v-model="product.category" class="mb-4" />

      <h2 class="text-xl font-semibold mb-3">Dados dos Impostos</h2>
      <InputField label="Alíquota IBS (%)" v-model.number="product.ibsRate" type="number" step="0.01" min="0" max="100" class="mb-2" />
      <!-- Other applicable taxes (ICMS, PIS, COFINS, ISS) can be added here -->

      <button type="submit" class="mt-4 w-full bg-blue-600 text-white py-2 rounded-lg">Salvar Produto</button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router';
import InputField from '@/components/InputField.vue'
import { useProductsStore } from '@/store/products'

const productsStore = useProductsStore()
const router = useRouter();

const product = ref({
  sku: '',
  name: '',
  quantity: 1,
  unitPrice: 0.01,
  category: '',
  ibsRate: 0,
  // Other taxes can be initialized here
})

const submitProduct = async () => {
  console.log('Product Data:', product.value)
  await productsStore.createProduct(product.value)
  router.push('/products/list');
}
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>
