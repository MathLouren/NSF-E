import { defineStore } from "pinia"
import api from "@/services/api"

export const useAuthStore = defineStore("auth", {
  state: () => ({ user: null, token: null }),
  getters: {
    isAuthenticated: (state) => !!state.user,
  },
  actions: {
    async login(username, password) {
      try {
        const response = await api.post("/auth/login", { username, password });
        this.user = username; // For now, just set user to username
        this.token = response.data.token; // Assuming backend returns a token
        // Optionally, store token in localStorage
        localStorage.setItem('userToken', this.token);
      } catch (error) {
        console.error("Login failed:", error);
        this.user = null;
        this.token = null;
      }
    },
    logout() {
      this.user = null
      this.token = null;
      localStorage.removeItem('userToken');
    }
  }
})