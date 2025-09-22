import { defineStore } from 'pinia'
import api from '@/services/api'

export const useSettingsStore = defineStore('settings', {
  state: () => ({
    companySettings: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchSettings() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/configurations')
        this.companySettings = response.data
      } catch (error) {
        this.error = error
        console.error('Error fetching settings:', error)
      } finally {
        this.loading = false
      }
    },
    async saveSettings(settingsData) {
      this.loading = true
      this.error = null
      try {
        const response = await api.post('/configurations', settingsData)
        this.companySettings = response.data
        return response.data
      } catch (error) {
        this.error = error
        console.error('Error saving settings:', error)
      } finally {
        this.loading = false
      }
    },
  },
})
