<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Configurações</h1>
    <form @submit.prevent="saveSettings" class="bg-white p-6 rounded-lg shadow-md">
      <h2 class="text-xl font-semibold mb-3">Dados da Empresa</h2>
      <InputField label="CNPJ da Empresa" v-model="settings.cnpj" placeholder="00.000.000/0000-00" class="mb-2" />
      <InputField label="Razão Social da Empresa" v-model="settings.legalName" class="mb-2" />
      <InputField label="Logradouro" v-model="settings.logradouro" class="mb-2" />
      <InputField label="Número" v-model="settings.numero" class="mb-2" />
      <InputField label="Bairro" v-model="settings.bairro" class="mb-2" />
      <InputField label="Município" v-model="settings.municipio" class="mb-2" />
      <InputField label="UF" v-model="settings.uf" class="mb-2" />
      <InputField label="CEP" v-model="settings.cep" placeholder="00000-000" class="mb-2" />
      <InputField label="Inscrição Municipal" v-model="settings.inscricaoMunicipal" class="mb-2" />
      <InputField label="Código do Município" v-model="settings.codigoMunicipio" class="mb-2" />
      <InputField label="Telefone" v-model="settings.telefone" class="mb-2" />
      <InputField label="Email" v-model="settings.email" type="email" class="mb-4" />

      <h2 class="text-xl font-semibold mb-3">Configurações de Impostos Padrão</h2>
      <InputField label="Alíquota Padrão IBS (%)" v-model.number="settings.defaultIbsRate" type="number" step="0.01" min="0" max="100" class="mb-4" />

      <!-- Further settings like Digital Certificate and SEFAZ integration can be added here -->

      <button type="submit" class="bg-green-600 text-white px-4 py-2 rounded-lg">Salvar Configurações</button>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router' // Importar useRouter
import InputField from '@/components/InputField.vue'
import { useSettingsStore } from '@/store/settings'

const settingsStore = useSettingsStore()
const router = useRouter() // Instanciar o router

const settings = ref({
  cnpj: '',
  legalName: '',
  logradouro: '',
  numero: '',
  bairro: '',
  municipio: '',
  uf: '',
  cep: '',
  inscricaoMunicipal: '',
  codigoMunicipio: '',
  telefone: '',
  email: '',
  defaultIbsRate: 0,
})

// Função para resetar os campos do formulário
const resetSettingsForm = () => {
  settings.value = {
    cnpj: '',
    legalName: '',
    logradouro: '',
    numero: '',
    bairro: '',
    municipio: '',
    uf: '',
    cep: '',
    inscricaoMunicipal: '',
    codigoMunicipio: '',
    telefone: '',
    email: '',
    defaultIbsRate: 0,
  };
};

onMounted(async () => {
  await settingsStore.fetchSettings();
  if (settingsStore.companySettings) {
    // Ensure all properties are copied, handling potential nulls from backend
    settings.value = {
      cnpj: settingsStore.companySettings.cnpj || '',
      legalName: settingsStore.companySettings.legalName || '',
      logradouro: settingsStore.companySettings.logradouro || '',
      numero: settingsStore.companySettings.numero || '',
      bairro: settingsStore.companySettings.bairro || '',
      municipio: settingsStore.companySettings.municipio || '',
      uf: settingsStore.companySettings.uf || '',
      cep: settingsStore.companySettings.cep || '',
      inscricaoMunicipal: settingsStore.companySettings.inscricaoMunicipal || '',
      codigoMunicipio: settingsStore.companySettings.codigoMunicipio || '',
      telefone: settingsStore.companySettings.telefone || '',
      email: settingsStore.companySettings.email || '',
      defaultIbsRate: settingsStore.companySettings.defaultIbsRate || 0,
    };
  }
});

const saveSettings = async () => {
  console.log('Settings Data:', settings.value)
  try {
    await settingsStore.saveSettings(settings.value)
    alert('Configurações salvas com sucesso!')
    resetSettingsForm(); // Limpar o formulário após salvar
    router.push('/dashboard'); // Redirecionar para o dashboard
  } catch (error) {
    console.error('Erro ao salvar configurações:', error)
    alert('Erro ao salvar configurações. Verifique o console para mais detalhes.')
  }
}
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>