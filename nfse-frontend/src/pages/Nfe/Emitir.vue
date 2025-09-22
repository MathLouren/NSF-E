<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Emitir NF-e de Produtos</h1>
        <p class="text-gray-600 mt-2">Sistema de emissão de Nota Fiscal Eletrônica - Homologação</p>
      </div>

      <!-- Status do Ambiente -->
      <div class="mb-6">
        <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-yellow-800">Ambiente de Homologação</h3>
              <p class="text-sm text-yellow-700 mt-1">Certifique-se de que o certificado digital está carregado e o ambiente está configurado para homologação.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Formulário Principal -->
      <form @submit.prevent="emitirNFe" class="space-y-8">
        <!-- Dados da NF-e -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Dados da NF-e</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Número da NF-e</label>
              <input 
                v-model.number="nfeData.ide.nNF" 
                type="number" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="1"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Série</label>
              <input 
                v-model.number="nfeData.ide.serie" 
                type="number" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="1"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Natureza da Operação</label>
              <input 
                v-model="nfeData.ide.natOp" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="VENDA"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Código do Município de Ocorrência</label>
              <input 
                v-model="nfeData.ide.cMunFG" 
                type="number" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="3304557"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Tipo de NF-e</label>
              <select 
                v-model="nfeData.ide.tpNF" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Selecione</option>
                <option value="0">0 - Entrada</option>
                <option value="1">1 - Saída</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Identificação do Destino</label>
              <select 
                v-model="nfeData.ide.idDest" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Selecione</option>
                <option value="1">1 - Operação interna</option>
                <option value="2">2 - Operação interestadual</option>
                <option value="3">3 - Operação com exterior</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Consumidor Final</label>
              <select 
                v-model="nfeData.ide.indFinal" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Selecione</option>
                <option value="0">0 - Não</option>
                <option value="1">1 - Sim</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Indicador de Presença</label>
              <select 
                v-model="nfeData.ide.indPres" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Selecione</option>
                <option value="0">0 - Não se aplica</option>
                <option value="1">1 - Operação presencial</option>
                <option value="2">2 - Operação não presencial, internet</option>
                <option value="3">3 - Operação não presencial, teleatendimento</option>
                <option value="4">4 - NFC-e em operação com entrega em domicílio</option>
                <option value="5">5 - Operação presencial, fora do estabelecimento</option>
                <option value="9">9 - Operação não presencial, outros</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Data/Hora de Saída/Entrada</label>
              <input 
                v-model="nfeData.ide.dhSaiEnt" 
                type="datetime-local" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
        </div>

        <!-- Dados do Emitente -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Dados do Emitente</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">CNPJ</label>
              <input 
                v-model="nfeData.emit.CNPJ" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="00000000000000"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Razão Social</label>
              <input 
                v-model="nfeData.emit.xNome" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Empresa Exemplo Ltda"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Nome Fantasia</label>
              <input 
                v-model="nfeData.emit.xFant" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Empresa Exemplo"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Inscrição Estadual</label>
              <input 
                v-model="nfeData.emit.IE" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="123456789"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Inscrição Municipal</label>
              <input 
                v-model="nfeData.emit.IM" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="123456"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">CNAE Fiscal</label>
              <input 
                v-model="nfeData.emit.CNAE" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="1234567"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Regime Tributário</label>
              <select 
                v-model="nfeData.emit.CRT" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Selecione</option>
                <option value="1">1 - Microempresa Municipal</option>
                <option value="2">2 - Estimativa</option>
                <option value="3">3 - Sociedade de Profissionais</option>
                <option value="4">4 - Cooperativa</option>
                <option value="5">5 - Microempresário Individual (MEI)</option>
                <option value="6">6 - Microempresário e Empresa de Pequeno Porte (ME/EPP)</option>
              </select>
            </div>
          </div>
          
          <!-- Endereço do Emitente -->
          <div class="mt-4">
            <h3 class="text-md font-medium text-gray-900 mb-3">Endereço</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="md:col-span-2">
                <label class="block text-sm font-medium text-gray-700 mb-1">Logradouro</label>
                <input 
                  v-model="nfeData.emit.enderEmit.xLgr" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Rua Exemplo"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Número</label>
                <input 
                  v-model="nfeData.emit.enderEmit.nro" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="100"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Bairro</label>
                <input 
                  v-model="nfeData.emit.enderEmit.xBairro" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Centro"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Cidade</label>
                <input 
                  v-model="nfeData.emit.enderEmit.xMun" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Rio de Janeiro"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">UF</label>
                <select 
                  v-model="nfeData.emit.enderEmit.UF" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Selecione</option>
                  <option value="RJ">RJ</option>
                  <option value="SP">SP</option>
                  <option value="MG">MG</option>
                  <option value="ES">ES</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">CEP</label>
                <input 
                  v-model="nfeData.emit.enderEmit.CEP" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="20000000"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Código do Município (IBGE)</label>
                <input 
                  v-model="nfeData.emit.enderEmit.cMun" 
                  type="number" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="3304557"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Código do País</label>
                <input 
                  v-model="nfeData.emit.enderEmit.cPais" 
                  type="number" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="1058"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Nome do País</label>
                <input 
                  v-model="nfeData.emit.enderEmit.xPais" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="BRASIL"
                  required
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Dados do Destinatário -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Dados do Destinatário</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">CNPJ/CPF</label>
              <input 
                v-model="nfeData.dest.CNPJ" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="00000000000000"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Razão Social/Nome</label>
              <input 
                v-model="nfeData.dest.xNome" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Cliente Exemplo"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Inscrição Estadual</label>
              <input 
                v-model="nfeData.dest.IE" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="123456789"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input 
                v-model="nfeData.dest.email" 
                type="email" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="cliente@exemplo.com"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Inscrição Municipal</label>
              <input 
                v-model="nfeData.dest.IM" 
                type="text" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="123456"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Indicador IE</label>
              <select 
                v-model="nfeData.dest.indIEDest" 
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Selecione</option>
                <option value="1">1 - Contribuinte ICMS</option>
                <option value="2">2 - Contribuinte isento de Inscrição</option>
                <option value="9">9 - Não Contribuinte</option>
              </select>
            </div>
          </div>
          
          <!-- Endereço do Destinatário -->
          <div class="mt-4">
            <h3 class="text-md font-medium text-gray-900 mb-3">Endereço</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="md:col-span-2">
                <label class="block text-sm font-medium text-gray-700 mb-1">Logradouro</label>
                <input 
                  v-model="nfeData.dest.enderDest.xLgr" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Rua Cliente"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Número</label>
                <input 
                  v-model="nfeData.dest.enderDest.nro" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="200"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Bairro</label>
                <input 
                  v-model="nfeData.dest.enderDest.xBairro" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Centro"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Cidade</label>
                <input 
                  v-model="nfeData.dest.enderDest.xMun" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Rio de Janeiro"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">UF</label>
                <select 
                  v-model="nfeData.dest.enderDest.UF" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Selecione</option>
                  <option value="RJ">RJ</option>
                  <option value="SP">SP</option>
                  <option value="MG">MG</option>
                  <option value="ES">ES</option>
                </select>
              </div>
        <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">CEP</label>
                <input 
                  v-model="nfeData.dest.enderDest.CEP" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="20000000"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Código do Município (IBGE)</label>
                <input 
                  v-model="nfeData.dest.enderDest.cMun" 
                  type="number" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="3304557"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Código do País</label>
                <input 
                  v-model="nfeData.dest.enderDest.cPais" 
                  type="number" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="1058"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Nome do País</label>
                <input 
                  v-model="nfeData.dest.enderDest.xPais" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="BRASIL"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Telefone</label>
                <input 
                  v-model="nfeData.dest.enderDest.fone" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="(21) 99999-9999"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Produtos -->
        <div class="bg-white shadow rounded-lg p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-lg font-semibold text-gray-900">Produtos</h2>
            <button 
              type="button" 
              @click="adicionarProduto"
              class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
            >
              Adicionar Produto
            </button>
          </div>
          
          <div v-for="(produto, index) in nfeData.det" :key="index" class="border border-gray-200 rounded-lg p-4 mb-4">
            <div class="flex justify-between items-center mb-3">
              <h3 class="font-medium text-gray-900">Produto {{ index + 1 }}</h3>
              <button 
                v-if="nfeData.det.length > 1"
                type="button" 
                @click="removerProduto(index)"
                class="text-red-600 hover:text-red-800"
              >
                Remover
              </button>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Código do Produto</label>
                <input 
                  v-model="produto.prod.cProd" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="001"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Descrição</label>
                <input 
                  v-model="produto.prod.xProd" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Produto Exemplo"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">NCM</label>
                <input 
                  v-model="produto.prod.NCM" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="6109.10.00"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">CFOP</label>
                <input 
                  v-model.number="produto.prod.CFOP" 
                  type="number" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="5102"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Unidade Comercial</label>
                <input 
                  v-model="produto.prod.uCom" 
                  type="text" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="UN"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Quantidade</label>
                <input 
                  v-model.number="produto.prod.qCom" 
                  type="number" 
                  step="0.01"
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="1.00"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Valor Unitário</label>
                <input 
                  v-model.number="produto.prod.vUnCom" 
                  type="number" 
                  step="0.01"
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="100.00"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Valor Total</label>
                <input 
                  v-model.number="produto.prod.vProd" 
                  type="number" 
                  step="0.01"
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="100.00"
                  readonly
                />
        </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Alíquota ICMS (%)</label>
                <input 
                  v-model.number="produto.imposto.ICMS.ICMS00.pICMS" 
                  type="number" 
                  step="0.01"
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="18.00"
                  required
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Origem da Mercadoria</label>
                <select 
                  v-model="produto.imposto.ICMS.ICMS00.orig" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Selecione</option>
                  <option value="0">0 - Nacional</option>
                  <option value="1">1 - Estrangeira - Importação direta</option>
                  <option value="2">2 - Estrangeira - Adquirida no mercado interno</option>
                  <option value="3">3 - Nacional - Mercadoria com mais de 40% de conteúdo de importação</option>
                  <option value="4">4 - Nacional - Produção conforme processo produtivo básico</option>
                  <option value="5">5 - Nacional - Mercadoria com menos de 40% de conteúdo de importação</option>
                  <option value="6">6 - Estrangeira - Importação direta sem similar nacional</option>
                  <option value="7">7 - Estrangeira - Mercado interno sem similar nacional</option>
                  <option value="8">8 - Nacional - Mercadoria com mais de 70% de conteúdo de importação</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">CST</label>
                <select 
                  v-model="produto.imposto.ICMS.ICMS00.CST" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Selecione</option>
                  <option value="00">00 - Tributada integralmente</option>
                  <option value="10">10 - Tributada e com cobrança do ICMS por substituição tributária</option>
                  <option value="20">20 - Com redução de base de cálculo</option>
                  <option value="30">30 - Isenta ou não tributada e com cobrança do ICMS por substituição tributária</option>
                  <option value="40">40 - Isenta</option>
                  <option value="41">41 - Não tributada</option>
                  <option value="50">50 - Suspensão</option>
                  <option value="51">51 - Diferimento</option>
                  <option value="60">60 - ICMS cobrado anteriormente por substituição tributária</option>
                  <option value="70">70 - Com redução de base de cálculo e cobrança do ICMS por substituição tributária</option>
                  <option value="90">90 - Outras</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Modalidade BC</label>
                <select 
                  v-model="produto.imposto.ICMS.ICMS00.modBC" 
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Selecione</option>
                  <option value="0">0 - Margem Valor Agregado (%)</option>
                  <option value="1">1 - Pauta (Valor)</option>
                  <option value="2">2 - Preço Tabelado Máximo (Valor)</option>
                  <option value="3">3 - Valor da Operação</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Percentual FCP (%)</label>
                <input 
                  v-model.number="produto.imposto.ICMS.ICMS00.pFCP" 
                  type="number" 
                  step="0.01"
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="0.00"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Valor FCP</label>
                <input 
                  v-model.number="produto.imposto.ICMS.ICMS00.vFCP" 
                  type="number" 
                  step="0.01"
                  class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="0.00"
                  readonly
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Totais -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Totais</h2>
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Valor Total dos Produtos</label>
              <input 
                v-model.number="nfeData.total.ICMSTot.vProd" 
                type="number" 
                step="0.01"
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                readonly
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Valor Total do ICMS</label>
              <input 
                v-model.number="nfeData.total.ICMSTot.vICMS" 
                type="number" 
                step="0.01"
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                readonly
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Valor Total da NF-e</label>
              <input 
                v-model.number="nfeData.total.ICMSTot.vNF" 
                type="number" 
                step="0.01"
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                readonly
              />
        </div>
        <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Base de Cálculo ICMS</label>
              <input 
                v-model.number="nfeData.total.ICMSTot.vBC" 
                type="number" 
                step="0.01"
                class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                readonly
              />
            </div>
          </div>
        </div>

        <!-- Botões de Ação -->
        <div class="flex justify-end space-x-4">
          <button 
            type="button" 
            @click="calcularTotais"
            class="bg-gray-600 text-white px-6 py-3 rounded-md hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-500"
          >
            Calcular Totais
          </button>
          <button 
            type="submit" 
            :disabled="loading"
            class="bg-blue-600 text-white px-6 py-3 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          >
            <span v-if="loading">Emitindo...</span>
            <span v-else>Emitir NF-e</span>
          </button>
      </div>
    </form>

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
              <h3 class="text-sm font-medium text-green-800">NF-e Autorizada com Sucesso!</h3>
              <div class="mt-2 text-sm text-green-700">
                <p><strong>Protocolo:</strong> {{ resultado.protocolo }}</p>
                <p><strong>Número NF-e:</strong> {{ resultado.numeroNFe }}</p>
                <p><strong>Série:</strong> {{ resultado.serie }}</p>
                <p><strong>Data de Autorização:</strong> {{ formatarData(resultado.dataAutorizacao) }}</p>
              </div>
              <div class="mt-4">
                <button 
                  @click="gerarDANFE"
                  class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  Gerar DANFE
                </button>
              </div>
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
              <h3 class="text-sm font-medium text-red-800">Erro na Emissão</h3>
              <div class="mt-2 text-sm text-red-700">
                <p><strong>Mensagem:</strong> {{ resultado.mensagem }}</p>
                <p v-if="resultado.codigoStatus"><strong>Código:</strong> {{ resultado.codigoStatus }}</p>
                <div v-if="resultado.erros && resultado.erros.length > 0" class="mt-2">
                  <p><strong>Detalhes dos Erros:</strong></p>
                  <ul class="list-disc list-inside">
                    <li v-for="erro in resultado.erros" :key="erro">{{ erro }}</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, watch } from 'vue'
import api from '@/services/api'

const loading = ref(false)
const resultado = ref(null)

// Dados da NF-e
const nfeData = reactive({
  ide: {
    cNF: "",
    natOp: "",
    mod: "",
    serie: "",
    nNF: "",
    dhEmi: "",
    tpNF: "",
    idDest: "",
    cMunFG: "",
    tpImp: "",
    tpEmis: "",
    cDV: "",
    tpAmb: "",
    finNFe: "",
    indFinal: "",
    indPres: "",
    procEmi: "",
    verProc: ""
  },
  emit: {
    CNPJ: "",
    xNome: "",
    xFant: "",
    enderEmit: {
      xLgr: "",
      nro: "",
      xBairro: "",
      cMun: "",
      xMun: "",
      UF: "",
      CEP: "",
      cPais: "",
      xPais: ""
    },
    IE: "",
    IEST: "",
    IM: "",
    CNAE: "",
    CRT: ""
  },
  dest: {
    CNPJ: "",
    xNome: "",
    enderDest: {
      xLgr: "",
      nro: "",
      xBairro: "",
      cMun: "",
      xMun: "",
      UF: "",
      CEP: "",
      cPais: "",
      xPais: "",
      fone: ""
    },
    indIEDest: "",
    IE: "",
    IM: "",
    email: ""
  },
  det: [
    {
      nItem: "",
      prod: {
        cProd: "",
        cEAN: "",
        xProd: "",
        NCM: "",
        CFOP: "",
        uCom: "",
        qCom: "",
        vUnCom: "",
        vProd: "",
        cEANTrib: "",
        uTrib: "",
        qTrib: "",
        vUnTrib: "",
        indTot: "" 
      },
      imposto: {
        ICMS: {
          ICMS00: {
            orig: "",
            CST: "",
            modBC: "",
            vBC: "",
            pICMS: "",
            vICMS: "",
            pFCP: "",
            vFCP: ""
          }
        }
      }
    }
  ],
  total: {
    ICMSTot: {
      vBC: "",
      vICMS: "",
      vICMSDeson: "",
      vFCP: "",
      vBCST: "",
      vST: "",
      vFCPST: "",
      vFCPSTRet: "",
      vProd: "",
      vFrete: "",
      vSeg: "",
      vDesc: "",
      vII: "",
      vIPI: "",
      vIPIDevol: "",
      vPIS: "",
      vCOFINS: "",
      vOutro: "",
      vNF: ""
    }
  },
  transp: {
    modFrete: ""
  },
  pag: {
    detPag: [
      {
        indPag: "",
        tPag: "",
        vPag: ""
      }
    ]
  },
  infAdic: {
    infCpl: ""
  }
})

// Adicionar produto
const adicionarProduto = () => {
  const novoItem = nfeData.det.length + 1
  nfeData.det.push({
    nItem: novoItem,
    prod: {
      cProd: "",
      cEAN: "",
      xProd: "",
      NCM: "",
      CFOP: "",
      uCom: "",
      qCom: "",
      vUnCom: "",
      vProd: "",
      cEANTrib: "",
      uTrib: "",
      qTrib: "",
      vUnTrib: "",
      indTot: ""
    },
      imposto: {
        ICMS: {
          ICMS00: {
            orig: "",
            CST: "",
            modBC: "",
            vBC: "",
            pICMS: "",
            vICMS: "",
            pFCP: "",
            vFCP: ""
          }
        }
      }
  })
}

// Remover produto
const removerProduto = (index) => {
  if (nfeData.det.length > 1) {
    nfeData.det.splice(index, 1)
    // Renumerar itens
    nfeData.det.forEach((item, idx) => {
      item.nItem = idx + 1
    })
  }
}

// Calcular totais
const calcularTotais = () => {
  let vProd = 0
  let vICMS = 0
  let vBC = 0
  let vFCP = 0

  nfeData.det.forEach(item => {
    // Calcular valor do produto
    item.prod.vProd = item.prod.qCom * item.prod.vUnCom
    
    // Calcular ICMS
    const alicota = item.imposto.ICMS.ICMS00.pICMS / 100
    item.imposto.ICMS.ICMS00.vBC = item.prod.vProd
    item.imposto.ICMS.ICMS00.vICMS = item.prod.vProd * alicota
    
    // Calcular FCP
    const alicotaFCP = item.imposto.ICMS.ICMS00.pFCP / 100
    item.imposto.ICMS.ICMS00.vFCP = item.prod.vProd * alicotaFCP
    
    vProd += item.prod.vProd
    vICMS += item.imposto.ICMS.ICMS00.vICMS
    vBC += item.imposto.ICMS.ICMS00.vBC
    vFCP += item.imposto.ICMS.ICMS00.vFCP
  })

  nfeData.total.ICMSTot.vProd = vProd
  nfeData.total.ICMSTot.vICMS = vICMS
  nfeData.total.ICMSTot.vBC = vBC
  nfeData.total.ICMSTot.vFCP = vFCP
  nfeData.total.ICMSTot.vNF = vProd
  nfeData.pag.detPag[0].vPag = vProd
}

// Emitir NF-e
const emitirNFe = async () => {
  loading.value = true
  resultado.value = null
  
  try {
    // Calcular totais antes de enviar
    calcularTotais()
    
    // Definir data de emissão
    nfeData.ide.dhEmi = new Date().toISOString()
    
    const { data } = await api.post('/NFe/emitir', nfeData)
    resultado.value = data
  } catch (error) {
    resultado.value = error.response?.data || { 
      sucesso: false, 
      mensagem: error.message 
    }
  } finally {
    loading.value = false
  }
}

// Gerar DANFE
const gerarDANFE = async () => {
  try {
    if (resultado.value?.protocolo) {
      const response = await api.get(`/NFe/danfe/${resultado.value.protocolo}`, {
        responseType: 'blob'
      })
      
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `DANFE-${resultado.value.numeroNFe}.pdf`)
      document.body.appendChild(link)
      link.click()
      link.remove()
    }
  } catch (error) {
    console.error('Erro ao gerar DANFE:', error)
    alert('Erro ao gerar DANFE')
  }
}

// Formatar data
const formatarData = (data) => {
  if (!data) return ''
  return new Date(data).toLocaleString('pt-BR')
}

// Watch para calcular automaticamente quando valores mudam
watch(() => nfeData.det, () => {
  calcularTotais()
}, { deep: true })

// Calcular totais na inicialização
calcularTotais()
</script>


