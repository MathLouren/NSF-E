<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Detalhes do Produto</h1>
    <div v-if="product" class="bg-white p-6 rounded-lg shadow-md">
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">SKU:</label>
        <p class="text-gray-900">{{ product.sku }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Nome:</label>
        <p class="text-gray-900">{{ product.name }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Quantidade:</label>
        <p class="text-gray-900">{{ product.quantity }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Preço Unitário:</label>
        <p class="text-gray-900">{{ product.unitPrice }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Categoria:</label>
        <p class="text-gray-900">{{ product.category }}</p>
      </div>
      <div class="mb-4">
        <label class="block text-gray-700 text-sm font-bold mb-2">Alíquota IBS (%):</label>
        <p class="text-gray-900">{{ product.ibsRate }}</p>
      </div>
      <button @click="$router.back()" class="mt-4 bg-gray-600 text-white py-2 px-4 rounded-lg">Voltar</button>
    </div>
    <div v-else class="text-red-500">Produto não encontrado.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useProductsStore } from '@/store/products'

const route = useRoute()
const productsStore = useProductsStore();
const product = ref(null)

onMounted(async () => {
  const productId = route.params.id
  await productsStore.fetchProductById(productId);
  product.value = productsStore.product;
})
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>
