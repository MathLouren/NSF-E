import { defineStore } from 'pinia'
import api from '@/services/api'

export const useCompanyStore = defineStore('company', {
  state: () => ({
    companies: [],
    selectedCompany: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchAllCompanies() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/companies')
        this.companies = response.data
      } catch (error) {
        this.error = error
        console.error('Error fetching companies:', error)
      } finally {
        this.loading = false
      }
    },
    async fetchCompanyById(id) {
      this.loading = true
      this.error = null
      try {
        const response = await api.get(`/companies/${id}`)
        this.selectedCompany = response.data
      } catch (error) {
        this.error = error
        console.error(`Error fetching company with id ${id}:`, error)
      } finally {
        this.loading = false
      }
    },
    setSelectedCompany(company) {
      this.selectedCompany = company;
    },
  },
})
