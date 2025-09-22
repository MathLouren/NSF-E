import { defineStore } from 'pinia'
import api from '@/services/api'

export const useServicesStore = defineStore('services', {
  state: () => ({
    services: [],
    service: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchServices() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/services')
        this.services = response.data
      } catch (error) {
        this.error = error
        console.error('Error fetching services:', error)
      } finally {
        this.loading = false
      }
    },
    async fetchServiceById(id) {
      this.loading = true
      this.error = null
      try {
        const response = await api.get(`/services/${id}`)
        this.service = response.data
      } catch (error) {
        this.error = error
        console.error(`Error fetching service with id ${id}:`, error)
      } finally {
        this.loading = false
      }
    },
    async createService(serviceData) {
      this.loading = true
      this.error = null
      try {
        const response = await api.post('/services', serviceData)
        this.services.push(response.data)
        return response.data
      } catch (error) {
        this.error = error
        console.error('Error creating service:', error)
      } finally {
        this.loading = false
      }
    },
  },
})
