<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Emitir Nova NFS-e</h1>
    
    <div class="mb-4">
      <label for="company-select" class="block text-gray-700 text-sm font-bold mb-2">Selecionar Empresa Prestadora:</label>
      <select
        id="company-select"
        v-model="selectedCompanyId"
        @change="selectCompany"
        class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
      >
        <option value="" disabled>Selecione uma empresa</option>
        <option v-for="company in companyStore.companies" :key="company.id" :value="company.id">
          {{ company.legalName }} ({{ company.cnpj }})
        </option>
      </select>
    </div>

    <form @submit.prevent="submitNfse" class="bg-white p-6 rounded-lg shadow-md">
      <InputField label="Data de Emissão" v-model="nfse.data_emissao" type="datetime-local" class="mb-4" />
      <h2 class="text-xl font-semibold mb-3">Dados do Prestador</h2>
      <CnpjInput 
        label="CNPJ do Prestador" 
        v-model="nfse.prestador.cnpj" 
        @cnpj-consulted="preencherDadosPrestador"
        placeholder="00.000.000/0000-00" 
        class="mb-2" 
      />
      <InputField label="Razão Social do Prestador" v-model="nfse.prestador.razaoSocial" class="mb-2" />
      <InputField label="Logradouro do Prestador" v-model="nfse.prestador.logradouro" class="mb-2" />
      <InputField label="Número do Prestador" v-model="nfse.prestador.numero" class="mb-2" />
      <InputField label="Bairro do Prestador" v-model="nfse.prestador.bairro" class="mb-2" />
      <InputField label="Município do Prestador" v-model="nfse.prestador.municipio" class="mb-2" />
      <InputField label="UF do Prestador" v-model="nfse.prestador.uf" class="mb-2" />
      <InputField label="CEP do Prestador" v-model="nfse.prestador.cep" placeholder="00000-000" class="mb-2" />
      <InputField label="Inscrição Municipal do Prestador" v-model="nfse.prestador.inscricaoMunicipal" class="mb-2" />
      <InputField label="Código do Município do Prestador" v-model="nfse.prestador.codigoMunicipio" class="mb-2" />
      <InputField label="Telefone do Prestador" v-model="nfse.prestador.telefone" class="mb-2" />
      <InputField label="Email do Prestador" v-model="nfse.prestador.email" type="email" class="mb-4" />

      <h2 class="text-xl font-semibold mb-3">Dados do Tomador</h2>
      <CnpjInput 
        label="CPF/CNPJ do Tomador" 
        v-model="nfse.tomador.cpfCnpj" 
        @cnpj-consulted="preencherDadosTomador"
        placeholder="000.000.000-00 ou 00.000.000/0000-00" 
        class="mb-2" 
      />
      <InputField label="Nome/Razão Social do Tomador" v-model="nfse.tomador.nomeRazaoSocial" class="mb-2" />
      <InputField label="Logradouro do Tomador" v-model="nfse.tomador.logradouro" class="mb-2" />
      <InputField label="Número do Tomador" v-model="nfse.tomador.numero" class="mb-2" />
      <InputField label="Bairro do Tomador" v-model="nfse.tomador.bairro" class="mb-2" />
      <InputField label="Município do Tomador" v-model="nfse.tomador.municipio" class="mb-2" />
      <InputField label="UF do Tomador" v-model="nfse.tomador.uf" class="mb-2" />
      <InputField label="CEP do Tomador" v-model="nfse.tomador.cep" placeholder="00000-000" class="mb-2" />
      <InputField label="Telefone do Tomador" v-model="nfse.tomador.telefone" class="mb-2" />
      <InputField label="Email do Tomador" v-model="nfse.tomador.email" type="email" class="mb-4" />

      <h2 class="text-xl font-semibold mb-3">Dados dos Serviços</h2>
      <div v-for="(servico, index) in nfse.servicos" :key="index" class="border p-4 mb-4 rounded-md">
        <h3 class="font-semibold mb-2">Serviço {{ index + 1 }}</h3>
        <div class="mb-4">
          <label :for="`service-select-${index}`" class="block text-gray-700 text-sm font-bold mb-2">Selecionar Serviço Pré-definido:</label>
          <select
            :id="`service-select-${index}`"
            v-model="selectedServiceIds[index]"
            @change="(event) => selectService(event.target.value, index)"
            class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline mb-2"
          >
            <option value="" disabled>Selecione um serviço ou produto</option>
            <option v-for="item in combinedServiceProducts" :key="item.id" :value="JSON.stringify({ id: item.id, type: item.type })">
              {{ item.descricao || item.name }} ({{ item.codigo || item.sku }}) - {{ item.type === 'service' ? 'Serviço' : 'Produto' }}
            </option>
          </select>
        </div>
        <InputField label="Código do Serviço" v-model="servico.codigo" class="mb-2" />
        <InputField label="Descrição do Serviço" v-model="servico.descricao" type="textarea" class="mb-2" />
        <InputField label="Quantidade" v-model.number="servico.quantidade" type="number" class="mb-2" />
        <InputField label="Valor Unitário" v-model.number="servico.valorUnitario" type="number" step="0.01" class="mb-2" />
        <InputField label="Valor Total" v-model.number="servico.valorTotal" type="number" step="0.01" class="mb-2" />
        <InputField label="Desconto" v-model.number="servico.desconto" type="number" step="0.01" class="mb-2" />
        <InputField label="Alíquota IBS (%)" v-model.number="servico.aliquotaIbs" type="number" step="0.01" min="0" max="100" class="mb-2" />
        <InputField label="Base de Cálculo ISS" v-model.number="servico.baseCalculoIss" type="number" step="0.01" class="mb-2" />
        <InputField label="Valor ISS" v-model.number="servico.valorIss" type="number" step="0.01" class="mb-2" />
        <label class="block text-gray-700 text-sm font-bold mb-2">
          ISS Retido:
          <input type="checkbox" v-model="servico.issRetido" class="ml-2 leading-tight">
        </label>
        <InputField label="Unidade" v-model="servico.unidade" class="mb-2" />
        <button v-if="nfse.servicos.length > 1" @click="removeService(index)" type="button" class="mt-2 bg-red-500 hover:bg-red-600 text-white py-1 px-3 rounded-lg text-sm">Remover Serviço</button>
      </div>
      <button @click="addService" type="button" class="bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded-lg mb-4">Adicionar Serviço</button>

      <h2 class="text-xl font-semibold mb-3">Informações Adicionais</h2>
      <InputField label="Forma de Pagamento" v-model="nfse.informacoesAdicionais.formaPagamento" class="mb-2" />
      <InputField label="Observações" v-model="nfse.informacoesAdicionais.observacoes" type="textarea" class="mb-2" />
      <InputField label="Município do Prestador (Inf. Adicionais)" v-model="nfse.informacoesAdicionais.municipioPrestador" class="mb-2" />
      <InputField label="Código de Tributação Municipal" v-model="nfse.informacoesAdicionais.codigoTributacaoMunicipal" class="mb-2" />
      <InputField label="Protocolo de Envio" v-model="nfse.informacoesAdicionais.protocoloEnvio" class="mb-4" />
      
      <button type="submit" class="mt-4 w-full bg-green-600 text-white py-2 rounded-lg">Emitir NFS-e</button>
    </form>
    <div class="mt-4 flex justify-between">
      <button @click="saveDraft" class="bg-yellow-500 hover:bg-yellow-600 text-white font-bold py-2 px-4 rounded-lg">Salvar Rascunho</button>
      <button @click="loadDraft(true)" class="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded-lg">Carregar Rascunho</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import InputField from '@/components/InputField.vue'
import CnpjInput from '@/components/CnpjInput.vue'
import { useNfseStore } from '@/store/nfse'
import { useCompanyStore } from '@/store/company'
import { useServicesStore } from '@/store/services' // Adicionado
import { useProductsStore } from '@/store/products'   // Adicionado

const nfseStore = useNfseStore()
const router = useRouter()
const companyStore = useCompanyStore()
const servicesStore = useServicesStore() // Instanciar o store de serviços
const productsStore = useProductsStore() // Instanciar o store de produtos

const selectedCompanyId = ref('');
const selectedServiceIds = ref({}); // Para controlar os serviços selecionados em cada item de serviço

const combinedServiceProducts = computed(() => {
  const services = servicesStore.services.map(s => ({
    ...s,
    type: 'service',
    // Propriedades para compatibilidade com o dropdown
    codigo: s.codigo,
    descricao: s.descricao,
    valorUnitario: s.valorUnitario,
    valorTotal: s.valorTotal,
    desconto: s.desconto,
    aliquotaIbs: s.aliquotaIbs,
    unidade: s.unidade,
    baseCalculoIss: s.baseCalculoIss,
    valorIss: s.valorIss,
  }));
  const products = productsStore.products.map(p => ({
    ...p,
    type: 'product',
    // Mapeia propriedades do produto para nomes de serviço para compatibilidade com o dropdown e ServicoItem
    codigo: p.sku,
    descricao: p.name,
    valorUnitario: p.unitPrice,
    valorTotal: p.totalValue || (p.unitPrice * p.quantity), // Recalcula se TotalValue não estiver presente
    desconto: p.discount,
    aliquotaIbs: p.ibsRate,
    unidade: p.unit,
    baseCalculoIss: p.baseCalculoIss,
    valorIss: p.valorIss,
  }));
  return [...services, ...products];
});

const nfse = ref({
  numero: 0,
  serie: '1',
  data_emissao: new Date().toISOString().slice(0, 16), // YYYY-MM-DDTHH:MM
  status: 'Issued',
  prestador: {
    cnpj: '',
    razaoSocial: '',
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
  },
  tomador: {
    cpfCnpj: '',
    nomeRazaoSocial: '',
    logradouro: '',
    numero: '',
    bairro: '',
    municipio: '',
    uf: '',
    cep: '',
    telefone: '',
    email: '',
  },
  servicos: [
    {
      codigo: '',
      descricao: '',
      quantidade: 1,
      valorUnitario: 0,
      valorTotal: 0,
      desconto: 0,
      aliquotaIbs: 0,
      issRetido: false,
      baseCalculoIss: 0,
      valorIss: 0,
      unidade: 'serviço',
      categoria: '', // Adicionado
    },
  ],
  informacoesAdicionais: {
    formaPagamento: '',
    observacoes: '',
    municipioPrestador: '',
    codigoTributacaoMunicipal: '',
    protocoloEnvio: '',
  },
});

const addService = () => {
  nfse.value.servicos.push({
    codigo: '',
    descricao: '',
    quantidade: 1,
    valorUnitario: 0,
    valorTotal: 0,
    desconto: 0,
    aliquotaIbs: companyStore.selectedCompany?.defaultIbsRate || 0,
    issRetido: false,
    baseCalculoIss: 0,
    valorIss: 0,
    unidade: 'serviço',
    categoria: '', // Adicionado
  });
  selectedServiceIds.value[nfse.value.servicos.length - 1] = '';
};

const removeService = (index) => {
  nfse.value.servicos.splice(index, 1);
  if (selectedServiceIds.value[index] !== undefined) {
    delete selectedServiceIds.value[index];
    selectedServiceIds.value = nfse.value.servicos.map((_, i) => {
      const servicoItem = nfse.value.servicos[i];
      const foundItem = combinedServiceProducts.value.find(item => 
        (item.type === 'service' && item.codigo === servicoItem.codigo) ||
        (item.type === 'product' && item.sku === servicoItem.codigo)
      );
      return foundItem ? JSON.stringify({ id: foundItem.id, type: foundItem.type }) : '';
    });
  }
};

const saveDraft = () => {
  const draftToSave = JSON.parse(JSON.stringify(nfse.value));
  localStorage.setItem('nfseDraft', JSON.stringify(draftToSave));
  alert('Rascunho salvo com sucesso!');
};

const loadDraft = (showAlert = true) => {
  const draft = localStorage.getItem('nfseDraft');
  if (draft) {
    const loadedDraft = JSON.parse(draft);
    nfse.value = loadedDraft;

    selectedServiceIds.value = loadedDraft.servicos.map(servicoItem => {
      const foundItem = combinedServiceProducts.value.find(item => 
        (item.type === 'service' && item.codigo === servicoItem.codigo) ||
        (item.type === 'product' && item.sku === servicoItem.codigo)
      );
      return foundItem ? JSON.stringify({ id: foundItem.id, type: foundItem.type }) : '';
    });

    if (showAlert) {
      alert('Rascunho carregado com sucesso!');
    }
    return true;
  } else {
    if (showAlert) {
      alert('Nenhum rascunho encontrado.');
    }
    return false;
  }
};

const selectCompany = () => {
  const selectedCompany = companyStore.companies.find(c => c.id === selectedCompanyId.value);
  if (selectedCompany) {
    nfse.value.prestador = {
      cnpj: selectedCompany.cnpj || '',
      razaoSocial: selectedCompany.legalName || '',
      logradouro: selectedCompany.logradouro || '',
      numero: selectedCompany.numero || '',
      bairro: selectedCompany.bairro || '',
      municipio: selectedCompany.municipio || '',
      uf: selectedCompany.uf || '',
      cep: selectedCompany.cep || '',
      inscricaoMunicipal: selectedCompany.inscricaoMunicipal || '',
      codigoMunicipio: selectedCompany.codigoMunicipio || '',
      telefone: selectedCompany.telefone || '',
      email: selectedCompany.email || '',
    };
    if (nfse.value.servicos.length > 0) {
      nfse.value.servicos[0].aliquotaIbs = selectedCompany.defaultIbsRate || 0;
    }
    companyStore.setSelectedCompany(selectedCompany);
    alert(`Dados da empresa "${selectedCompany.legalName}" carregados com sucesso!`);
  } else {
    alert('Nenhuma empresa selecionada ou dados não encontrados.');
  }
};

const selectService = (selectedItemValue, index) => {
  const parsedValue = JSON.parse(selectedItemValue);
  const selectedItem = combinedServiceProducts.value.find(item => item.id === parsedValue.id && item.type === parsedValue.type);

  if (selectedItem) {
    const currentService = nfse.value.servicos[index];

    // Mapeamento de propriedades com base no tipo do item
    if (selectedItem.type === 'service') {
      currentService.codigo = selectedItem.codigo;
      currentService.descricao = selectedItem.descricao;
      currentService.quantidade = selectedItem.quantidade;
      currentService.valorUnitario = selectedItem.valorUnitario;
      currentService.valorTotal = selectedItem.valorTotal;
      currentService.desconto = selectedItem.desconto || 0;
      currentService.aliquotaIbs = selectedItem.aliquotaIbs;
      currentService.issRetido = selectedItem.issRetido;
      currentService.baseCalculoIss = selectedItem.baseCalculoIss;
      currentService.valorIss = selectedItem.valorIss;
      currentService.unidade = selectedItem.unidade;
      currentService.categoria = selectedItem.categoria; // Categoria do serviço
    } else if (selectedItem.type === 'product') {
      currentService.codigo = selectedItem.sku; // SKU do produto para Código do Serviço
      currentService.descricao = selectedItem.name; // Nome do produto para Descrição do Serviço
      currentService.quantidade = selectedItem.quantity; // Quantidade do produto
      currentService.valorUnitario = selectedItem.unitPrice; // Preço unitário do produto
      currentService.valorTotal = selectedItem.totalValue || (selectedItem.unitPrice * selectedItem.quantity); // Valor total do produto
      currentService.desconto = selectedItem.discount || 0;
      currentService.aliquotaIbs = selectedItem.ibsRate; // IBS do produto
      currentService.issRetido = false; // Produtos geralmente não têm ISS retido em NFS-e, ajustar conforme regra de negócio
      currentService.baseCalculoIss = selectedItem.baseCalculoIss;
      currentService.valorIss = selectedItem.valorIss;
      currentService.unidade = selectedItem.unit; // Unidade do produto
      currentService.categoria = selectedItem.category; // Categoria do produto
    }

    alert(`${selectedItem.type === 'service' ? 'Serviço' : 'Produto'} "${currentService.descricao}" selecionado com sucesso!`);
  } else {
    alert('Item não encontrado.');
  }
};

onMounted(async () => {
  await companyStore.fetchAllCompanies();
  await servicesStore.fetchServices();
  await productsStore.fetchProducts();

  const draftLoaded = loadDraft(false);

  if (draftLoaded && nfse.value.prestador.cnpj) {
    const existingCompany = companyStore.companies.find(c => c.cnpj === nfse.value.prestador.cnpj);
    if (existingCompany) {
      selectedCompanyId.value = existingCompany.id;
      companyStore.setSelectedCompany(existingCompany);
    }
  }

  // selectedServiceIds já é carregado no loadDraft, mas precisamos garantir que os valores sejam string JSON
  if (draftLoaded && nfse.value.servicos && nfse.value.servicos.length > 0) {
    selectedServiceIds.value = nfse.value.servicos.map(servicoItem => {
      const foundItem = combinedServiceProducts.value.find(item => 
        (item.type === 'service' && item.codigo === servicoItem.codigo) ||
        (item.type === 'product' && item.sku === servicoItem.codigo)
      );
      return foundItem ? JSON.stringify({ id: foundItem.id, type: foundItem.type }) : '';
    });
  }
})

// Função para preencher dados do prestador após consulta do CNPJ
const preencherDadosPrestador = (dadosEmpresa) => {
  nfse.value.prestador.razaoSocial = dadosEmpresa.razaoSocial || ''
  nfse.value.prestador.logradouro = dadosEmpresa.logradouro || ''
  nfse.value.prestador.numero = dadosEmpresa.numero || ''
  nfse.value.prestador.bairro = dadosEmpresa.bairro || ''
  nfse.value.prestador.municipio = dadosEmpresa.municipio || ''
  nfse.value.prestador.uf = dadosEmpresa.uf || ''
  nfse.value.prestador.cep = dadosEmpresa.cep || ''
  nfse.value.prestador.codigoMunicipio = dadosEmpresa.codigoMunicipio || ''
  nfse.value.prestador.telefone = dadosEmpresa.telefone || ''
  nfse.value.prestador.email = dadosEmpresa.email || ''
}

// Função para preencher dados do tomador após consulta do CNPJ
const preencherDadosTomador = (dadosEmpresa) => {
  nfse.value.tomador.nomeRazaoSocial = dadosEmpresa.razaoSocial || ''
  nfse.value.tomador.logradouro = dadosEmpresa.logradouro || ''
  nfse.value.tomador.numero = dadosEmpresa.numero || ''
  nfse.value.tomador.bairro = dadosEmpresa.bairro || ''
  nfse.value.tomador.municipio = dadosEmpresa.municipio || ''
  nfse.value.tomador.uf = dadosEmpresa.uf || ''
  nfse.value.tomador.cep = dadosEmpresa.cep || ''
  nfse.value.tomador.telefone = dadosEmpresa.telefone || ''
  nfse.value.tomador.email = dadosEmpresa.email || ''
}

const submitNfse = async () => {
  console.log('NFS-e Data:', nfse.value)
  try {
    await nfseStore.issueNfse(nfse.value)
    localStorage.removeItem('nfseDraft') // Clear draft after successful submission
    router.push('/nfse/lista')
  } catch (error) {
    console.error('Erro ao emitir NFS-e:', error)
    alert('Erro ao emitir NFS-e. Verifique o console para mais detalhes.')
  }
}
</script>

<style scoped>
/* Add any specific styles for this page here */
</style>