<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">🎯 Seletor de Versão da DANFE</h1>
        <p class="text-gray-600 mt-2">Compare as versões da DANFE: tradicional vs reforma tributária 2026</p>
      </div>

      <!-- Informações sobre as versões -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- Versão Atual -->
        <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-blue-500">
          <div class="flex items-center mb-4">
            <div class="bg-blue-100 p-2 rounded-lg mr-3">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-900">DANFE Atual (4.00)</h3>
              <p class="text-sm text-gray-600">Layout tradicional</p>
            </div>
          </div>
          <ul class="space-y-2 text-sm text-gray-700">
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              ICMS tradicional
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              IPI, PIS, COFINS
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              Totais básicos
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              Dados do emitente/destinatário
            </li>
          </ul>
        </div>

        <!-- Versão 2026 -->
        <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-green-500">
          <div class="flex items-center mb-4">
            <div class="bg-green-100 p-2 rounded-lg mr-3">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path>
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-900">DANFE 2026 (Reforma Tributária)</h3>
              <p class="text-sm text-gray-600">Layout 2026.001</p>
            </div>
          </div>
          <ul class="space-y-2 text-sm text-gray-700">
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              <span class="font-medium text-green-700">IBS</span> (Imposto sobre Bens e Serviços)
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              <span class="font-medium text-green-700">CBS</span> (Contribuição sobre Bens e Serviços)
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              <span class="font-medium text-green-700">IS</span> (Imposto Seletivo)
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              Rastreabilidade (GTIN)
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              Totais por UF/Município
            </li>
            <li class="flex items-center">
              <svg class="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
              </svg>
              Referências de documentos
            </li>
          </ul>
        </div>
      </div>

      <!-- Seletor de Versão -->
      <div class="mb-8">
        <DanfeVersionSelector 
          :nfe-data="nfeExemplo"
          @danfe-gerada="onDanfeGerada"
        />
      </div>

      <!-- Resultados -->
      <div v-if="resultados.length > 0" class="space-y-6">
        <h2 class="text-2xl font-bold text-gray-900">📄 DANFEs Geradas</h2>
        
        <div v-for="(resultado, index) in resultados" :key="index" class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-900">
              Teste {{ index + 1 }} - {{ resultado.versaoSolicitada.toUpperCase() }}
            </h3>
            <span class="text-sm text-gray-500">
              {{ new Date().toLocaleString('pt-BR') }}
            </span>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="danfe in resultado.danfes" :key="danfe.versao" class="border rounded-lg p-4">
              <div class="flex items-center justify-between mb-2">
                <h4 class="font-medium text-gray-800">
                  📄 {{ danfe.versao === 'atual' ? 'DANFE Atual' : 'DANFE 2026' }}
                </h4>
                <span class="text-xs px-2 py-1 rounded-full"
                      :class="danfe.versao === 'atual' ? 'bg-blue-100 text-blue-800' : 'bg-green-100 text-green-800'">
                  {{ danfe.layout }}
                </span>
              </div>
              
              <div class="space-y-2 text-sm">
                <div>
                  <span class="text-gray-600">Arquivo:</span>
                  <span class="font-medium">{{ danfe.nomeArquivo }}</span>
                </div>
                <div>
                  <span class="text-gray-600">Tamanho:</span>
                  <span class="font-medium">{{ formatBytes(danfe.tamanhoBytes) }}</span>
                </div>
              </div>
              
              <div class="mt-3">
                <button 
                  @click="downloadDanfe(danfe)"
                  class="w-full bg-green-600 text-white px-3 py-2 rounded-lg text-sm hover:bg-green-700 transition-colors duration-200"
                >
                  📥 Baixar PDF
                </button>
              </div>
            </div>
          </div>

          <!-- Comparação -->
          <div v-if="resultado.comparacao" class="mt-6 border rounded-lg p-4 bg-purple-50">
            <h5 class="font-medium text-gray-800 mb-3">📊 Comparação das Versões</h5>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
              <div>
                <h6 class="font-medium text-gray-700 mb-2">🆕 Novos Campos (2026)</h6>
                <ul class="space-y-1">
                  <li v-for="campo in resultado.comparacao.camposNovos" :key="campo" class="text-green-700">
                    + {{ campo }}
                  </li>
                </ul>
              </div>
              
              <div>
                <h6 class="font-medium text-gray-700 mb-2">🔄 Campos Mantidos</h6>
                <ul class="space-y-1">
                  <li v-for="campo in resultado.comparacao.camposMantidos" :key="campo" class="text-blue-700">
                    = {{ campo }}
                  </li>
                </ul>
              </div>
              
              <div>
                <h6 class="font-medium text-gray-700 mb-2">⚡ Principais Diferenças</h6>
                <ul class="space-y-1">
                  <li v-for="diff in resultado.comparacao.diferencasPrincipais" :key="diff" class="text-purple-700">
                    • {{ diff }}
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Instruções -->
      <div class="mt-8 bg-blue-50 border border-blue-200 rounded-lg p-6">
        <h3 class="text-lg font-semibold text-blue-900 mb-3">💡 Como Usar</h3>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-blue-800">
          <div>
            <h4 class="font-medium mb-2">1. Selecione a Versão</h4>
            <p>Escolha entre "Atual", "2026" ou "Ambas" para comparar</p>
          </div>
          <div>
            <h4 class="font-medium mb-2">2. Configure Opções</h4>
            <p>Marque "Modo Contingência" se necessário</p>
          </div>
          <div>
            <h4 class="font-medium mb-2">3. Gere e Baixe</h4>
            <p>Clique em "Gerar DANFE" e baixe os PDFs automaticamente</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import DanfeVersionSelector from '@/components/DanfeVersionSelector.vue'

const resultados = ref([])

// Dados de exemplo para demonstração
const nfeExemplo = computed(() => ({
  versao: "4.00",
  chaveAcesso: "35200114200166000187550010000000015123456789",
  ide: {
    cUF: 35,
    cNF: 12345678,
    natOp: "VENDA",
    mod: 55,
    serie: 1,
    nNF: 1,
    dhEmi: "2024-01-15T10:00:00-03:00",
    tpNF: 1,
    idDest: 1,
    cMunFG: 3550308,
    tpImp: 1,
    tpEmis: 1,
    cDV: 9,
    tpAmb: 2,
    finNFe: 1,
    indFinal: 1,
    indPres: 1,
    procEmi: 0,
    verProc: "1.0"
  },
  emit: {
    CNPJ: "14200166000187",
    xNome: "Empresa Exemplo Ltda",
    xFant: "Exemplo",
    enderEmit: {
      xLgr: "Rua das Flores",
      nro: "123",
      xBairro: "Centro",
      cMun: 3550308,
      xMun: "São Paulo",
      UF: "SP",
      CEP: "01234567",
      cPais: 1058,
      xPais: "Brasil"
    },
    IE: "123456789",
    CRT: 1
  },
  dest: {
    CNPJ: "12345678000195",
    xNome: "Cliente Exemplo Ltda",
    enderDest: {
      xLgr: "Av. Paulista",
      nro: "1000",
      xBairro: "Bela Vista",
      cMun: 3550308,
      xMun: "São Paulo",
      UF: "SP",
      CEP: "01310100",
      cPais: 1058,
      xPais: "Brasil"
    },
    indIEDest: 1,
    IE: "987654321"
  },
  det: [
    {
      nItem: 1,
      prod: {
        cProd: "001",
        cEAN: "7891234567890",
        xProd: "Produto Exemplo",
        NCM: "61091000",
        CFOP: "5102",
        uCom: "UN",
        qCom: 1.0000,
        vUnCom: 100.00,
        vProd: 100.00,
        cEANTrib: "7891234567890",
        uTrib: "UN",
        qTrib: 1.0000,
        vUnTrib: 100.00,
        indTot: 1
      },
      imposto: {
        ICMS: {
          ICMS00: {
            orig: 0,
            CST: "00",
            modBC: 3,
            vBC: 100.00,
            pICMS: 18.00,
            vICMS: 18.00
          }
        },
        IPI: {
          cEnq: "999",
          IPITrib: {
            CST: "50",
            vBC: 100.00,
            pIPI: 10.00,
            vIPI: 10.00
          }
        },
        PIS: {
          PISAliq: {
            CST: "01",
            vBC: 100.00,
            pPIS: 1.65,
            vPIS: 1.65
          }
        },
        COFINS: {
          COFINSAliq: {
            CST: "01",
            vBC: 100.00,
            pCOFINS: 7.60,
            vCOFINS: 7.60
          }
        }
      }
    }
  ],
  total: {
    ICMSTot: {
      vBC: 100.00,
      vICMS: 18.00,
      vICMSDeson: 0.00,
      vFCP: 0.00,
      vBCST: 0.00,
      vST: 0.00,
      vFCPST: 0.00,
      vFCPSTRet: 0.00,
      vProd: 100.00,
      vFrete: 0.00,
      vSeg: 0.00,
      vDesc: 0.00,
      vII: 0.00,
      vIPI: 10.00,
      vIPIDevol: 0.00,
      vPIS: 1.65,
      vCOFINS: 7.60,
      vOutro: 0.00,
      vNF: 137.25
    }
  },
  transp: {
    modFrete: 0
  },
  cobr: null,
  pag: {
    detPag: [
      {
        indPag: 0,
        tPag: "01",
        vPag: 137.25
      }
    ]
  },
  infAdic: {
    infCpl: "Informações complementares da NF-e"
  }
}))

// Método para lidar com DANFE gerada
const onDanfeGerada = (resultado) => {
  console.log('DANFE(s) gerada(s):', resultado)
  resultados.value.unshift(resultado)
}

// Método para baixar DANFE
const downloadDanfe = (danfe) => {
  try {
    const pdfBytes = base64ToArrayBuffer(danfe.danfeBase64)
    const blob = new Blob([pdfBytes], { type: 'application/pdf' })
    const url = window.URL.createObjectURL(blob)
    
    const link = document.createElement('a')
    link.href = url
    link.download = danfe.nomeArquivo
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Erro ao baixar arquivo:', error)
    alert('Erro ao baixar arquivo PDF')
  }
}

// Converter base64 para ArrayBuffer
const base64ToArrayBuffer = (base64) => {
  const binaryString = window.atob(base64)
  const bytes = new Uint8Array(binaryString.length)
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i)
  }
  return bytes.buffer
}

// Formatar bytes
const formatBytes = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script>

<style scoped>
/* Animações suaves */
.transition-colors {
  transition: background-color 0.2s ease-in-out, color 0.2s ease-in-out;
}
</style>
