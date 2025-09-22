<template>
  <div class="flex h-screen bg-gray-100">
    <Sidebar v-if="auth.isAuthenticated" />
    <div class="flex-1 flex flex-col">
      <Navbar v-if="auth.isAuthenticated" />
      <main class="p-6 overflow-y-auto">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<script setup>
import Sidebar from "@/components/Sidebar.vue"
import Navbar from "@/components/Navbar.vue"
import { useAuthStore } from "@/store/auth"

const auth = useAuthStore()
</script>