<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Detalhe da NFS-e</h1>
    <div v-if="nfse" class="bg-white p-6 rounded-lg shadow-md">
      <p class="mb-2"><strong>ID da NFS-e:</strong> {{ nfse.id }}</p>
      <p class="mb-2"><strong>Número:</strong> {{ nfse.numero }}</p>
      <p class="mb-2"><strong>Série:</strong> {{ nfse.serie }}</p>
      <p class="mb-4"><strong>Status:</strong> {{ nfse.status }}</p>

      <h2 class="text-xl font-semibold mb-3">Dados do Prestador</h2>
      <p class="mb-2"><strong>CNPJ:</strong> {{ nfse.prestador.cnpj }}</p>
      <p class="mb-2"><strong>Razão Social:</strong> {{ nfse.prestador.razaoSocial }}</p>
      <p class="mb-2"><strong>Endereço:</strong> {{ nfse.prestador.logradouro }}, {{ nfse.prestador.numero }} - {{ nfse.prestador.bairro }}</p>
      <p class="mb-2"><strong>Município:</strong> {{ nfse.prestador.municipio }}/{{ nfse.prestador.uf }} - CEP: {{ nfse.prestador.cep }}</p>
      <p class="mb-2"><strong>Inscrição Municipal:</strong> {{ nfse.prestador.inscricaoMunicipal }}</p>
      <p class="mb-2"><strong>Código do Município:</strong> {{ nfse.prestador.codigoMunicipio }}</p>
      <p class="mb-2"><strong>Telefone:</strong> {{ nfse.prestador.telefone }}</p>
      <p class="mb-4"><strong>Email:</strong> {{ nfse.prestador.email }}</p>

      <h2 class="text-xl font-semibold mb-3">Dados do Tomador</h2>
      <p class="mb-2"><strong>CPF/CNPJ:</strong> {{ nfse.tomador.cpfCnpj }}</p>
      <p class="mb-2"><strong>Nome/Razão Social:</strong> {{ nfse.tomador.nomeRazaoSocial }}</p>
      <p class="mb-2"><strong>Endereço:</strong> {{ nfse.tomador.logradouro }}, {{ nfse.tomador.numero }} - {{ nfse.tomador.bairro }}</p>
      <p class="mb-2"><strong>Município:</strong> {{ nfse.tomador.municipio }}/{{ nfse.tomador.uf }} - CEP: {{ nfse.tomador.cep }}</p>
      <p class="mb-2"><strong>Telefone:</strong> {{ nfse.tomador.telefone }}</p>
      <p class="mb-4"><strong>Email:</strong> {{ nfse.tomador.email }}</p>

      <h2 class="text-xl font-semibold mb-3">Dados dos Serviços</h2>
      <div v-for="(servico, index) in nfse.servicos" :key="index" class="border p-4 mb-4 rounded-md">
        <h3 class="font-semibold mb-2">Serviço {{ index + 1 }}</h3>
        <p class="mb-2"><strong>Código:</strong> {{ servico.codigo }}</p>
        <p class="mb-2"><strong>Descrição:</strong> {{ servico.descricao }}</p>
        <p class="mb-2"><strong>Quantidade:</strong> {{ servico.quantidade }}</p>
        <p class="mb-2"><strong>Valor Unitário:</strong> R$ {{ servico.valorUnitario?.toFixed(2) }}</p>
        <p class="mb-2"><strong>Valor Total:</strong> R$ {{ servico.valorTotal?.toFixed(2) }}</p>
        <p class="mb-2"><strong>Desconto:</strong> R$ {{ servico.desconto?.toFixed(2) }}</p>
        <p class="mb-2"><strong>Alíquota IBS:</strong> {{ servico.aliquotaIbs }}%</p>
        <p class="mb-2"><strong>ISS Retido:</strong> {{ servico.issRetido ? 'Sim' : 'Não' }}</p>
        <p class="mb-2"><strong>Base de Cálculo ISS:</strong> R$ {{ servico.baseCalculoIss?.toFixed(2) }}</p>
        <p class="mb-2"><strong>Valor ISS:</strong> R$ {{ servico.valorIss?.toFixed(2) }}</p>
        <p class="mb-2"><strong>Unidade:</strong> {{ servico.unidade }}</p>
      </div>

      <h2 class="text-xl font-semibold mb-3">Informações Adicionais</h2>
      <p class="mb-2"><strong>Forma de Pagamento:</strong> {{ nfse.informacoesAdicionais.formaPagamento }}</p>
      <p class="mb-2"><strong>Observações:</strong> {{ nfse.informacoesAdicionais.observacoes }}</p>
      <p class="mb-2"><strong>Município do Prestador:</strong> {{ nfse.informacoesAdicionais.municipioPrestador }}</p>
      <p class="mb-2"><strong>Código de Tributação Municipal:</strong> {{ nfse.informacoesAdicionais.codigoTributacaoMunicipal }}</p>
      <p class="mb-4"><strong>Protocolo de Envio:</strong> {{ nfse.informacoesAdicionais.protocoloEnvio }}</p>
      
      <p class="text-gray-600 text-sm"><strong>Data de Emissão:</strong> {{ new Date(nfse.dataEmissao).toLocaleString() }}</p>

      <button @click="downloadDanfe" class="mt-6 bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-lg">
        Baixar DANFE
      </button>
    </div>
    <div v-else class="text-red-500">NFS-e não encontrada ou carregando...</div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useNfseStore } from '@/store/nfse'
import api from '@/services/api'

const route = useRoute()
const nfseStore = useNfseStore()
const nfse = ref(null)

const fetchNfseDetails = async (id) => {
  await nfseStore.fetchNfseById(id)
  nfse.value = nfseStore.nfse
}

const downloadDanfe = async () => {
  if (!nfse.value || !nfse.value.id) {
    alert('NFS-e ID não disponível para download.');
    return;
  }
  try {
    const response = await api.get(`/nfse/${nfse.value.id}/danfe`, {
      responseType: 'blob' // Important: tell Axios to expect a binary blob
    });
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `DANFE_NFS-e_${nfse.value.numero}.pdf`); // Use nfse.numero for filename
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url); // Clean up the URL object
  } catch (error) {
    console.error('Erro ao baixar DANFE:', error);
    alert('Erro ao baixar o DANFE. Verifique o console para mais detalhes.');
  }
};

onMounted(() => {
  fetchNfseDetails(route.params.id)
})

// Watch for changes in route params (e.g., navigating from one detail to another)
watch(() => route.params.id, (newId) => {
  if (newId) {
    fetchNfseDetails(newId)
  }
})
</script>

<style scoped>
/* Adicione estilos específicos para esta página aqui */
</style>