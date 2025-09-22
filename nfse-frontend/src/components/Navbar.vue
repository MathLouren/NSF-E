<template>
  <header class="bg-white shadow p-4 flex justify-between items-center">
    <h1 class="text-lg font-semibold">NFS-e 2026</h1>
    <button @click="logout" class="text-red-600 hover:underline">Sair</button>
  </header>
</template>

<script setup>
import { useAuthStore } from "@/store/auth"
const auth = useAuthStore()

const logout = () => {
  auth.logout()
}
</script>