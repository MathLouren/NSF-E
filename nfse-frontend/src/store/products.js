import { defineStore } from 'pinia'
import api from '@/services/api'

export const useProductsStore = defineStore('products', {
  state: () => ({
    products: [],
    product: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchProducts() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/products')
        this.products = response.data
      } catch (error) {
        this.error = error
        console.error('Error fetching products:', error)
      } finally {
        this.loading = false
      }
    },
    async fetchProductById(id) {
      this.loading = true
      this.error = null
      try {
        const response = await api.get(`/products/${id}`)
        this.product = response.data
      } catch (error) {
        this.error = error
        console.error(`Error fetching product with id ${id}:`, error)
      } finally {
        this.loading = false
      }
    },
    async createProduct(productData) {
      this.loading = true
      this.error = null
      try {
        const response = await api.post('/products', productData)
        this.products.push(response.data)
        return response.data
      } catch (error) {
        this.error = error
        console.error('Error creating product:', error)
      } finally {
        this.loading = false
      }
    },
  },
})
