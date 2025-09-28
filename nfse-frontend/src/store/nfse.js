import { defineStore } from 'pinia'
import api from '@/services/api'

export const useNfseStore = defineStore('nfse', {
  state: () => ({
    nfses: [],
    nfse: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchNfses() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/Nfse')
        this.nfses = response.data
      } catch (error) {
        this.error = error
        console.error('Error fetching NFS-e:', error)
      } finally {
        this.loading = false
      }
    },
    async fetchNfseById(id) {
      this.loading = true
      this.error = null
      try {
        const response = await api.get(`/Nfse/${id}`)
        this.nfse = response.data
      } catch (error) {
        this.error = error
        console.error(`Error fetching NFS-e with id ${id}:`, error)
      } finally {
        this.loading = false
      }
    },
    async issueNfse(nfseData) {
      this.loading = true
      this.error = null
      try {
        const response = await api.post('/Nfse', nfseData)
        this.nfses.push(response.data)
        return response.data
      } catch (error) {
        this.error = error
        console.error('Error issuing NFS-e:', error)
      } finally {
        this.loading = false
      }
    },
  },
})