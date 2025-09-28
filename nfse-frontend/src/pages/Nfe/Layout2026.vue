<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Emissão NF-e - Layout 2026</h1>
        <p class="text-gray-600 mt-2">Sistema de emissão com suporte completo aos novos impostos (IBS, CBS, IS)</p>
      </div>

      <!-- Status do Ambiente -->
      <div class="mb-6">
        <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-blue-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-blue-800">Layout 2026 - Reforma Tributária</h3>
              <p class="text-sm text-blue-700 mt-1">Suporte completo aos novos impostos: IBS, CBS e IS com validações automáticas.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Formulário Principal -->
      <form @submit.prevent="emitirNFe" class="space-y-8">
        <!-- Seleção do Tipo de Operação -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Tipo de Operação</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <label
              v-for="operation in operationTypes"
              :key="operation.value"
              class="relative cursor-pointer"
            >
              <input
                v-model="nfeData.ide.operationType"
                :value="operation.value"
                type="radio"
                class="sr-only"
                @change="onOperationTypeChange"
              />
              <div
                class="border-2 rounded-lg p-4 text-center transition-colors"
                :class="nfeData.ide.operationType === operation.value
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-200 hover:border-gray-300'"
              >
                <div class="text-2xl mb-2">{{ operation.icon }}</div>
                <div class="font-medium text-gray-900">{{ operation.label }}</div>
                <div class="text-sm text-gray-500 mt-1">{{ operation.description }}</div>
              </div>
            </label>
          </div>
        </div>

        <!-- Dados da NF-e -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Dados da NF-e</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <FiscalField
              label="Número da NF-e"
              v-model="nfeData.ide.nNF"
              type="numero"
              :required="true"
              help-text="Número sequencial da nota fiscal"
            />
            <FiscalField
              label="Série"
              v-model="nfeData.ide.serie"
              type="serie"
              :required="true"
              help-text="Série da nota fiscal"
            />
            <FiscalField
              label="Natureza da Operação"
              v-model="nfeData.ide.natOp"
              type="natureza"
              :required="true"
              :filters="{ operationType: nfeData.ide.operationType }"
              help-text="Descrição da natureza da operação"
            />
            <FiscalField
              label="Código do Município de Ocorrência"
              v-model="nfeData.ide.cMunFG"
              type="municipio"
              :required="true"
              help-text="Código IBGE do município de ocorrência"
            />
            <FiscalField
              label="Tipo de NF-e"
              v-model="nfeData.ide.tpNF"
              type="tipoNFe"
              :required="true"
              help-text="0=Entrada, 1=Saída"
            />
            <FiscalField
              label="Identificação do Destino"
              v-model="nfeData.ide.idDest"
              type="identificacaoDestino"
              :required="true"
              help-text="1=Interna, 2=Interestadual, 3=Exterior"
            />
            <FiscalField
              label="Consumidor Final"
              v-model="nfeData.ide.indFinal"
              type="consumidorFinal"
              :required="true"
              help-text="0=Não, 1=Sim"
            />
            <FiscalField
              label="Indicador de Presença"
              v-model="nfeData.ide.indPres"
              type="indicadorPresenca"
              :required="true"
              help-text="Indicador de presença do comprador"
            />
            <FiscalField
              label="Data/Hora de Saída/Entrada"
              v-model="nfeData.ide.dhSaiEnt"
              input-type="datetime-local"
              :required="true"
              help-text="Data e hora de saída ou entrada"
            />
          </div>
        </div>

        <!-- Dados do Emitente -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Dados do Emitente</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <CnpjInput 
              label="CNPJ" 
              v-model="nfeData.emit.CNPJ" 
              @cnpj-consulted="preencherDadosEmitente"
              placeholder="00.000.000/0000-00"
            />
            <FiscalField
              label="Razão Social"
              v-model="nfeData.emit.xNome"
              type="text"
              :required="true"
            />
            <FiscalField
              label="Nome Fantasia"
              v-model="nfeData.emit.xFant"
              type="text"
            />
            <FiscalField
              label="Inscrição Estadual"
              v-model="nfeData.emit.IE"
              type="inscricaoEstadual"
              :required="true"
            />
            <FiscalField
              label="Inscrição Municipal"
              v-model="nfeData.emit.IM"
              type="inscricaoMunicipal"
            />
            <FiscalField
              label="CNAE Fiscal"
              v-model="nfeData.emit.CNAE"
              type="cnae"
              :required="true"
            />
            <FiscalField
              label="Regime Tributário"
              v-model="nfeData.emit.CRT"
              type="regimeTributario"
              :required="true"
            />
          </div>
          
          <!-- Endereço do Emitente -->
          <div class="mt-6">
            <h3 class="text-md font-medium text-gray-900 mb-3">Endereço</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="md:col-span-2">
                <FiscalField
                  label="Logradouro"
                  v-model="nfeData.emit.enderEmit.xLgr"
                  type="text"
                  :required="true"
                />
              </div>
              <FiscalField
                label="Número"
                v-model="nfeData.emit.enderEmit.nro"
                type="text"
                :required="true"
              />
              <FiscalField
                label="Bairro"
                v-model="nfeData.emit.enderEmit.xBairro"
                type="text"
                :required="true"
              />
              <FiscalField
                label="Cidade"
                v-model="nfeData.emit.enderEmit.xMun"
                type="text"
                :required="true"
              />
              <FiscalField
                label="UF"
                v-model="nfeData.emit.enderEmit.UF"
                type="uf"
                :required="true"
              />
              <FiscalField
                label="CEP"
                v-model="nfeData.emit.enderEmit.CEP"
                type="cep"
                :required="true"
              />
              <FiscalField
                label="Código do Município (IBGE)"
                v-model="nfeData.emit.enderEmit.cMun"
                type="municipio"
                :required="true"
                :filters="{ uf: nfeData.emit.enderEmit.UF }"
              />
              <FiscalField
                label="Código do País"
                v-model="nfeData.emit.enderEmit.cPais"
                type="pais"
                :required="true"
              />
              <FiscalField
                label="Nome do País"
                v-model="nfeData.emit.enderEmit.xPais"
                type="text"
                :required="true"
              />
            </div>
          </div>
        </div>

        <!-- Dados do Destinatário -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Dados do Destinatário</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <CnpjInput 
              label="CNPJ/CPF" 
              v-model="nfeData.dest.CNPJ" 
              @cnpj-consulted="preencherDadosDestinatario"
              placeholder="00.000.000/0000-00"
            />
            <FiscalField
              label="Razão Social/Nome"
              v-model="nfeData.dest.xNome"
              type="text"
              :required="true"
            />
            <FiscalField
              label="Inscrição Estadual"
              v-model="nfeData.dest.IE"
              type="inscricaoEstadual"
            />
            <FiscalField
              label="Email"
              v-model="nfeData.dest.email"
              input-type="email"
            />
            <FiscalField
              label="Inscrição Municipal"
              v-model="nfeData.dest.IM"
              type="inscricaoMunicipal"
            />
            <FiscalField
              label="Indicador IE"
              v-model="nfeData.dest.indIEDest"
              type="indicadorIE"
              :required="true"
            />
          </div>
          
          <!-- Endereço do Destinatário -->
          <div class="mt-6">
            <h3 class="text-md font-medium text-gray-900 mb-3">Endereço</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="md:col-span-2">
                <FiscalField
                  label="Logradouro"
                  v-model="nfeData.dest.enderDest.xLgr"
                  type="text"
                  :required="true"
                />
              </div>
              <FiscalField
                label="Número"
                v-model="nfeData.dest.enderDest.nro"
                type="text"
                :required="true"
              />
              <FiscalField
                label="Bairro"
                v-model="nfeData.dest.enderDest.xBairro"
                type="text"
                :required="true"
              />
              <FiscalField
                label="Cidade"
                v-model="nfeData.dest.enderDest.xMun"
                type="text"
                :required="true"
              />
              <FiscalField
                label="UF"
                v-model="nfeData.dest.enderDest.UF"
                type="uf"
                :required="true"
              />
              <FiscalField
                label="CEP"
                v-model="nfeData.dest.enderDest.CEP"
                type="cep"
                :required="true"
              />
              <FiscalField
                label="Código do Município (IBGE)"
                v-model="nfeData.dest.enderDest.cMun"
                type="municipio"
                :required="true"
                :filters="{ uf: nfeData.dest.enderDest.UF }"
              />
              <FiscalField
                label="Código do País"
                v-model="nfeData.dest.enderDest.cPais"
                type="pais"
                :required="true"
              />
              <FiscalField
                label="Nome do País"
                v-model="nfeData.dest.enderDest.xPais"
                type="text"
                :required="true"
              />
              <FiscalField
                label="Telefone"
                v-model="nfeData.dest.enderDest.fone"
                type="telefone"
              />
            </div>
          </div>
        </div>

        <!-- Busca de Nota para Devolução/Crédito -->
        <div v-if="showNoteSearch" class="bg-white shadow rounded-lg p-6">
          <NoteSearch
            :operation-type="nfeData.ide.operationType"
            @note-selected="onNoteSelected"
          />
        </div>

        <!-- Produtos/Serviços -->
        <div class="bg-white shadow rounded-lg p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-lg font-semibold text-gray-900">Itens</h2>
            <button 
              type="button" 
              @click="adicionarItem"
              class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
            >
              Adicionar Item
            </button>
          </div>
          
          <div v-for="(item, index) in nfeData.det" :key="index" class="border border-gray-200 rounded-lg p-4 mb-4">
            <div class="flex justify-between items-center mb-3">
              <h3 class="font-medium text-gray-900">Item {{ index + 1 }}</h3>
              <button 
                v-if="nfeData.det.length > 1"
                type="button" 
                @click="removerItem(index)"
                class="text-red-600 hover:text-red-800"
              >
                Remover
              </button>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <FiscalField
                label="Código do Produto"
                v-model="item.prod.cProd"
                type="text"
                :required="true"
              />
              <FiscalField
                label="Descrição"
                v-model="item.prod.xProd"
                type="text"
                :required="true"
              />
              <FiscalField
                label="NCM"
                v-model="item.prod.NCM"
                type="ncm"
                :required="true"
              />
              <FiscalField
                label="CFOP"
                v-model="item.prod.CFOP"
                type="cfop"
                :required="true"
                :filters="{
                  operationType: nfeData.ide.operationType,
                  ufOrigin: nfeData.emit.enderEmit.UF,
                  ufDest: nfeData.dest.enderDest.UF
                }"
              />
              <FiscalField
                label="Unidade Comercial"
                v-model="item.prod.uCom"
                type="unidade"
                :required="true"
              />
              <FiscalField
                label="Quantidade"
                v-model="item.prod.qCom"
                input-type="number"
                :required="true"
              />
              <FiscalField
                label="Valor Unitário"
                v-model="item.prod.vUnCom"
                input-type="number"
                :required="true"
              />
              <FiscalField
                label="Valor Total"
                v-model="item.prod.vProd"
                input-type="number"
                :required="true"
                :disabled="true"
              />
              
              <!-- Impostos IBS -->
              <div class="md:col-span-3">
                <h4 class="text-sm font-medium text-gray-700 mb-2">Imposto sobre Bens e Serviços (IBS)</h4>
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <FiscalField
                    label="CST IBS"
                    v-model="item.imposto.IBS.CST"
                    type="cst"
                    :required="true"
                    :filters="{ operationType: 'produto' }"
                  />
                  <FiscalField
                    label="Alíquota IBS (%)"
                    v-model="item.imposto.IBS.pIBS"
                    input-type="number"
                    :required="true"
                  />
                  <FiscalField
                    label="Base de Cálculo IBS"
                    v-model="item.imposto.IBS.vBC"
                    input-type="number"
                    :required="true"
                    :disabled="true"
                  />
                  <FiscalField
                    label="Valor IBS"
                    v-model="item.imposto.IBS.vIBS"
                    input-type="number"
                    :required="true"
                    :disabled="true"
                  />
                </div>
              </div>
              
              <!-- Impostos CBS -->
              <div class="md:col-span-3">
                <h4 class="text-sm font-medium text-gray-700 mb-2">Contribuição sobre Bens e Serviços (CBS)</h4>
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <FiscalField
                    label="CST CBS"
                    v-model="item.imposto.CBS.CST"
                    type="cst"
                    :required="true"
                    :filters="{ operationType: 'produto' }"
                  />
                  <FiscalField
                    label="Alíquota CBS (%)"
                    v-model="item.imposto.CBS.pCBS"
                    input-type="number"
                    :required="true"
                  />
                  <FiscalField
                    label="Base de Cálculo CBS"
                    v-model="item.imposto.CBS.vBC"
                    input-type="number"
                    :required="true"
                    :disabled="true"
                  />
                  <FiscalField
                    label="Valor CBS"
                    v-model="item.imposto.CBS.vCBS"
                    input-type="number"
                    :required="true"
                    :disabled="true"
                  />
                </div>
              </div>
              
              <!-- Impostos IS (para serviços) -->
              <div v-if="item.tipo === 'servico'" class="md:col-span-3">
                <h4 class="text-sm font-medium text-gray-700 mb-2">Imposto sobre Serviços (IS)</h4>
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <FiscalField
                    label="CST IS"
                    v-model="item.imposto.IS.CST"
                    type="cst"
                    :required="true"
                    :filters="{ operationType: 'servico' }"
                  />
                  <FiscalField
                    label="Alíquota IS (%)"
                    v-model="item.imposto.IS.pIS"
                    input-type="number"
                    :required="true"
                  />
                  <FiscalField
                    label="Base de Cálculo IS"
                    v-model="item.imposto.IS.vBC"
                    input-type="number"
                    :required="true"
                    :disabled="true"
                  />
                  <FiscalField
                    label="Valor IS"
                    v-model="item.imposto.IS.vIS"
                    input-type="number"
                    :required="true"
                    :disabled="true"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Totais dos Impostos -->
        <TaxTotals
          :items="nfeData.det"
          :emitente-u-f="nfeData.emit.enderEmit.UF"
          :destinatario-u-f="nfeData.dest.enderDest.UF"
        />

        <!-- Botões de Ação -->
        <div class="flex justify-end space-x-4">
          <button 
            type="button" 
            @click="validarCampos"
            class="bg-yellow-600 text-white px-6 py-3 rounded-md hover:bg-yellow-700 focus:outline-none focus:ring-2 focus:ring-yellow-500"
          >
            Validar Campos
          </button>
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
import { reactive, ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import CnpjInput from '@/components/CnpjInput.vue'
import FiscalField from '@/components/FiscalField.vue'
import TaxTotals from '@/components/TaxTotals.vue'
import NoteSearch from '@/components/NoteSearch.vue'
import fiscalTablesService from '@/services/fiscalTablesService'

const loading = ref(false)
const resultado = ref(null)

// Tipos de operação disponíveis
const operationTypes = [
  {
    value: 'produto',
    label: 'Produto',
    icon: '📦',
    description: 'Venda de produtos'
  },
  {
    value: 'devolucao',
    label: 'Devolução',
    icon: '↩️',
    description: 'Devolução de produtos'
  },
  {
    value: 'credito',
    label: 'Crédito',
    icon: '💳',
    description: 'Operação de crédito'
  },
  {
    value: 'monofasia',
    label: 'Monofásica',
    icon: '⚡',
    description: 'Operação monofásica'
  }
]

// Dados da NF-e com estrutura 2026
const nfeData = reactive({
  ide: {
    operationType: 'produto',
    cNF: "",
    natOp: "",
    mod: "55",
    serie: "1",
    nNF: "",
    dhEmi: "",
    tpNF: "1",
    idDest: "1",
    cMunFG: "",
    tpImp: "1",
    tpEmis: "1",
    cDV: "",
    tpAmb: "2",
    finNFe: "1",
    indFinal: "0",
    indPres: "1",
    procEmi: "0",
    verProc: "1.0"
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
      cPais: "1058",
      xPais: "BRASIL"
    },
    IE: "",
    IEST: "",
    IM: "",
    CNAE: "",
    CRT: "3"
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
      cPais: "1058",
      xPais: "BRASIL",
      fone: ""
    },
    indIEDest: "1",
    IE: "",
    IM: "",
    email: ""
  },
  det: [
    {
      nItem: "1",
      prod: {
        cProd: "",
        cEAN: "",
        xProd: "",
        NCM: "",
        CFOP: "",
        uCom: "",
        qCom: 1,
        vUnCom: 0,
        vProd: 0,
        cEANTrib: "",
        uTrib: "",
        qTrib: 0,
        vUnTrib: 0,
        indTot: "1"
      },
      imposto: {
        IBS: {
          CST: "",
          vBC: 0,
          pIBS: 0,
          vIBS: 0
        },
        CBS: {
          CST: "",
          vBC: 0,
          pCBS: 0,
          vCBS: 0
        },
        IS: {
          CST: "",
          vBC: 0,
          pIS: 0,
          vIS: 0
        }
      }
    }
  ],
  total: {
    ICMSTot: {
      vBC: 0,
      vICMS: 0,
      vICMSDeson: 0,
      vFCP: 0,
      vBCST: 0,
      vST: 0,
      vFCPST: 0,
      vFCPSTRet: 0,
      vProd: 0,
      vFrete: 0,
      vSeg: 0,
      vDesc: 0,
      vII: 0,
      vIPI: 0,
      vIPIDevol: 0,
      vPIS: 0,
      vCOFINS: 0,
      vOutro: 0,
      vNF: 0
    },
    IBSTot: {
      vBC: 0,
      vIBS: 0
    },
    CBSTot: {
      vBC: 0,
      vCBS: 0
    },
    ISTot: {
      vBC: 0,
      vIS: 0
    }
  }
})

// Computed para mostrar busca de nota
const showNoteSearch = computed(() => {
  return ['devolucao', 'credito'].includes(nfeData.ide.operationType)
})

// Adicionar item
const adicionarItem = () => {
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
      qCom: 1,
      vUnCom: 0,
      vProd: 0,
      cEANTrib: "",
      uTrib: "",
      qTrib: 0,
      vUnTrib: 0,
      indTot: "1"
    },
    imposto: {
      IBS: {
        CST: "",
        vBC: 0,
        pIBS: 0,
        vIBS: 0
      },
      CBS: {
        CST: "",
        vBC: 0,
        pCBS: 0,
        vCBS: 0
      },
      IS: {
        CST: "",
        vBC: 0,
        pIS: 0,
        vIS: 0
      }
    }
  })
}

// Remover item
const removerItem = (index) => {
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
  let vIBS = 0
  let vCBS = 0
  let vIS = 0
  let vBCIBS = 0
  let vBCCBS = 0
  let vBCIS = 0

  nfeData.det.forEach(item => {
    // Calcular valor do produto
    item.prod.vProd = item.prod.qCom * item.prod.vUnCom
    
    // Calcular IBS
    item.imposto.IBS.vBC = item.prod.vProd
    item.imposto.IBS.vIBS = item.prod.vProd * (item.imposto.IBS.pIBS / 100)
    
    // Calcular CBS
    item.imposto.CBS.vBC = item.prod.vProd
    item.imposto.CBS.vCBS = item.prod.vProd * (item.imposto.CBS.pCBS / 100)
    
    // Calcular IS
    item.imposto.IS.vBC = item.prod.vProd
    item.imposto.IS.vIS = item.prod.vProd * (item.imposto.IS.pIS / 100)
    
    vProd += item.prod.vProd
    vIBS += item.imposto.IBS.vIBS
    vCBS += item.imposto.CBS.vCBS
    vIS += item.imposto.IS.vIS
    vBCIBS += item.imposto.IBS.vBC
    vBCCBS += item.imposto.CBS.vBC
    vBCIS += item.imposto.IS.vBC
  })

  // Atualizar totais
  nfeData.total.ICMSTot.vProd = vProd
  nfeData.total.ICMSTot.vNF = vProd
  nfeData.total.IBSTot.vBC = vBCIBS
  nfeData.total.IBSTot.vIBS = vIBS
  nfeData.total.CBSTot.vBC = vBCCBS
  nfeData.total.CBSTot.vCBS = vCBS
  nfeData.total.ISTot.vBC = vBCIS
  nfeData.total.ISTot.vIS = vIS
}

// Validar campos
const validarCampos = () => {
  const validation = fiscalTablesService.validateRequiredFields(nfeData, nfeData.ide.operationType)
  
  if (validation.errors.length > 0) {
    alert('Erros encontrados:\n' + validation.errors.join('\n'))
  } else if (validation.warnings.length > 0) {
    alert('Avisos:\n' + validation.warnings.join('\n'))
  } else {
    alert('Todos os campos obrigatórios estão preenchidos!')
  }
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
    
    const { data } = await api.post('/NFe/emitir-layout2026', nfeData)
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

// Preencher dados do emitente
const preencherDadosEmitente = (dadosEmpresa) => {
  nfeData.emit.xNome = dadosEmpresa.razaoSocial || ''
  nfeData.emit.xFant = dadosEmpresa.nomeFantasia || ''
  nfeData.emit.enderEmit.xLgr = dadosEmpresa.logradouro || ''
  nfeData.emit.enderEmit.nro = dadosEmpresa.numero || ''
  nfeData.emit.enderEmit.xBairro = dadosEmpresa.bairro || ''
  nfeData.emit.enderEmit.xMun = dadosEmpresa.municipio || ''
  nfeData.emit.enderEmit.UF = dadosEmpresa.uf || ''
  nfeData.emit.enderEmit.CEP = dadosEmpresa.cep || ''
  nfeData.emit.enderEmit.cMun = dadosEmpresa.codigoMunicipio || ''
  nfeData.emit.CNAE = dadosEmpresa.cnaeFiscal || ''
}

// Preencher dados do destinatário
const preencherDadosDestinatario = (dadosEmpresa) => {
  nfeData.dest.xNome = dadosEmpresa.razaoSocial || ''
  nfeData.dest.enderDest.xLgr = dadosEmpresa.logradouro || ''
  nfeData.dest.enderDest.nro = dadosEmpresa.numero || ''
  nfeData.dest.enderDest.xBairro = dadosEmpresa.bairro || ''
  nfeData.dest.enderDest.xMun = dadosEmpresa.municipio || ''
  nfeData.dest.enderDest.UF = dadosEmpresa.uf || ''
  nfeData.dest.enderDest.CEP = dadosEmpresa.cep || ''
  nfeData.dest.enderDest.cMun = dadosEmpresa.codigoMunicipio || ''
  nfeData.dest.email = dadosEmpresa.email || ''
}

// Nota selecionada para devolução/crédito
const onNoteSelected = (nota) => {
  console.log('Nota selecionada para devolução/crédito:', nota)
  // Implementar lógica de preenchimento baseada na nota selecionada
}

// Mudança no tipo de operação
const onOperationTypeChange = () => {
  // Limpar campos específicos quando mudar o tipo de operação
  if (nfeData.ide.operationType === 'devolucao' || nfeData.ide.operationType === 'credito') {
    // Mostrar busca de nota
  } else {
    // Ocultar busca de nota
  }
}

// Formatar data
const formatarData = (data) => {
  if (!data) return ''
  return new Date(data).toLocaleString('pt-BR')
}

// Carregar tabelas fiscais na inicialização
onMounted(async () => {
  try {
    await fiscalTablesService.loadFiscalTables()
  } catch (error) {
    console.error('Erro ao carregar tabelas fiscais:', error)
  }
})
</script>

<style scoped>
/* Estilos específicos da página */
</style>
