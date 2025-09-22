<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Configurações NF-e</h1>
        <p class="text-gray-600 mt-2">Configure o ambiente e certificado digital para emissão de NF-e</p>
      </div>

      <!-- Status do Ambiente -->
      <div class="bg-white shadow rounded-lg p-6 mb-8">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">Status do Ambiente</h2>
        <div class="flex items-center space-x-4">
          <div class="flex items-center">
            <div :class="[
              'w-3 h-3 rounded-full mr-2',
              ambienteAtual === 'Homologacao' ? 'bg-yellow-400' : 'bg-green-400'
            ]"></div>
            <span class="text-sm font-medium">
              {{ ambienteAtual === 'Homologacao' ? 'Homologação' : 'Produção' }}
            </span>
          </div>
          <button 
            @click="alterarAmbiente"
            :disabled="loading"
            class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          >
            <span v-if="loading">Alterando...</span>
            <span v-else>Alterar para {{ ambienteAtual === 'Homologacao' ? 'Produção' : 'Homologação' }}</span>
          </button>
        </div>
      </div>

      <!-- Upload de Certificado -->
      <div class="bg-white shadow rounded-lg p-6 mb-8">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">Certificado Digital</h2>
        
        <form @submit.prevent="uploadCertificado" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Arquivo do Certificado (.pfx/.p12)</label>
            <input 
              ref="certificadoInput"
              type="file" 
              accept=".pfx,.p12"
              @change="onCertificadoChange"
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Senha do Certificado</label>
            <input 
              v-model="certificadoForm.senha"
              type="password" 
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Digite a senha do certificado"
              required
            />
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">CNPJ da Empresa</label>
            <input 
              v-model="certificadoForm.cnpj"
              type="text" 
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="00000000000000"
              required
            />
          </div>
          
          <button 
            type="submit"
            :disabled="loading || !certificadoForm.arquivo"
            class="bg-green-600 text-white px-6 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50"
          >
            <span v-if="loading">Enviando...</span>
            <span v-else>Enviar Certificado</span>
          </button>
        </form>
      </div>

      <!-- Status do Certificado -->
      <div v-if="statusCertificado" class="bg-white shadow rounded-lg p-6 mb-8">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">Status do Certificado</h2>
        <div class="space-y-2">
          <div class="flex justify-between">
            <span class="text-sm text-gray-600">Status:</span>
            <span :class="[
              'text-sm font-medium',
              statusCertificado.valido ? 'text-green-600' : 'text-red-600'
            ]">
              {{ statusCertificado.valido ? 'Válido' : 'Inválido' }}
            </span>
          </div>
          <div v-if="statusCertificado.titular" class="flex justify-between">
            <span class="text-sm text-gray-600">Titular:</span>
            <span class="text-sm">{{ statusCertificado.titular }}</span>
          </div>
          <div v-if="statusCertificado.validade" class="flex justify-between">
            <span class="text-sm text-gray-600">Validade:</span>
            <span class="text-sm">{{ statusCertificado.validade }}</span>
          </div>
        </div>
      </div>

      <!-- Resultado -->
      <div v-if="resultado" class="mt-8">
        <div v-if="resultado.sucesso" class="bg-green-50 border border-green-200 rounded-lg p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-green-800">Operação Realizada com Sucesso!</h3>
              <p class="text-sm text-green-700 mt-1">{{ resultado.mensagem }}</p>
            </div>
          </div>
        </div>
        
        <div v-else class="bg-red-50 border border-red-200 rounded-lg p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800">Erro na Operação</h3>
              <p class="text-sm text-red-700 mt-1">{{ resultado.mensagem }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Informações de Homologação -->
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-6">
        <h3 class="text-lg font-medium text-blue-900 mb-3">Informações para Homologação</h3>
        <div class="text-sm text-blue-800 space-y-2">
          <p><strong>Ambiente de Homologação:</strong> Use apenas para testes. As NF-e emitidas neste ambiente não têm valor fiscal.</p>
          <p><strong>Certificado Digital:</strong> Necessário certificado A1 (.pfx/.p12) válido e não vencido.</p>
          <p><strong>CNPJ de Teste:</strong> Use CNPJs de teste fornecidos pela SEFAZ para homologação.</p>
          <p><strong>Endpoints:</strong> O sistema usa automaticamente os endpoints de homologação da SEFAZ-RJ via SVRS.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '@/services/api'

const loading = ref(false)
const resultado = ref(null)
const ambienteAtual = ref('Homologacao')
const statusCertificado = ref(null)
const certificadoInput = ref(null)

const certificadoForm = reactive({
  arquivo: null,
  senha: '',
  cnpj: ''
})

// Carregar status inicial
onMounted(async () => {
  await carregarStatusAmbiente()
  await carregarStatusCertificado()
})

// Carregar status do ambiente
const carregarStatusAmbiente = async () => {
  try {
    const { data } = await api.get('/NFe/ambiente')
    ambienteAtual.value = data.ambiente
  } catch (error) {
    console.error('Erro ao carregar status do ambiente:', error)
  }
}

// Carregar status do certificado
const carregarStatusCertificado = async () => {
  try {
    const { data } = await api.get('/NFe/certificado/status')
    statusCertificado.value = data
  } catch (error) {
    console.error('Erro ao carregar status do certificado:', error)
  }
}

// Alterar ambiente
const alterarAmbiente = async () => {
  loading.value = true
  resultado.value = null
  
  try {
    const novoAmbiente = ambienteAtual.value === 'Homologacao' ? 'Producao' : 'Homologacao'
    const { data } = await api.post('/NFe/ambiente', { Producao: novoAmbiente === 'Producao' })
    
    ambienteAtual.value = novoAmbiente
    resultado.value = { sucesso: true, mensagem: `Ambiente alterado para ${novoAmbiente === 'Producao' ? 'Produção' : 'Homologação'}` }
  } catch (error) {
    resultado.value = { sucesso: false, mensagem: error.response?.data?.mensagem || error.message }
  } finally {
    loading.value = false
  }
}

// Upload de certificado
const uploadCertificado = async () => {
  loading.value = true
  resultado.value = null
  
  try {
    const formData = new FormData()
    formData.append('Certificado', certificadoForm.arquivo)
    formData.append('Senha', certificadoForm.senha)
    formData.append('CnpjEmpresa', certificadoForm.cnpj)
    
    const { data } = await api.post('/NFe/certificado/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    
    resultado.value = { sucesso: true, mensagem: 'Certificado enviado com sucesso!' }
    
    // Limpar formulário
    certificadoForm.arquivo = null
    certificadoForm.senha = ''
    certificadoForm.cnpj = ''
    if (certificadoInput.value) {
      certificadoInput.value.value = ''
    }
    
    // Recarregar status do certificado
    await carregarStatusCertificado()
  } catch (error) {
    resultado.value = { sucesso: false, mensagem: error.response?.data?.mensagem || error.message }
  } finally {
    loading.value = false
  }
}

// Handler para mudança de arquivo
const onCertificadoChange = (event) => {
  const file = event.target.files[0]
  if (file) {
    certificadoForm.arquivo = file
  }
}
</script>
