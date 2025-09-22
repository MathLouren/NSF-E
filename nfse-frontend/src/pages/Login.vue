<template>
  <div class="flex items-center justify-center h-screen bg-gray-50">
    <div class="bg-white p-6 rounded-lg shadow-md w-96">
      <h1 class="text-xl font-bold mb-4">Login</h1>
      <form @submit.prevent="login">
        <InputField label="Usuário" v-model="user" />
        <InputField type="password" label="Senha" v-model="password" class="mt-2"/>
        <button type="submit" class="mt-4 w-full bg-blue-600 text-white py-2 rounded-lg">Entrar</button>
      </form>
    </div>
  </div>
</template>

<script setup>
import InputField from "@/components/InputField.vue"
import { useAuthStore } from "@/store/auth"
import { ref } from "vue"

const auth = useAuthStore()
const user = ref("")
const password = ref("")

const login = async () => {
  await auth.login(user.value, password.value)
}
</script>