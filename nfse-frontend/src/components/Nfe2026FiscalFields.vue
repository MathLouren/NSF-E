<template>
  <div class="bg-white rounded-lg shadow-md p-6">
    <h3 class="text-lg font-semibold text-gray-800 mb-4">Reforma Tributária 2026 - Campos Fiscais</h3>

    <!-- IBS -->
    <section class="mb-6">
      <div class="flex items-center justify-between mb-2">
        <h4 class="text-sm font-semibold text-blue-800">Grupos IBS por Item</h4>
        <button @click="addIBS" class="text-xs bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700">+ Adicionar IBS</button>
      </div>
      <div v-if="(model.GruposIBS || []).length === 0" class="text-xs text-gray-500 mb-2">Nenhum grupo IBS adicionado.</div>
      <div v-for="(g, idx) in model.GruposIBS" :key="idx" class="border rounded p-3 mb-2">
        <div class="grid grid-cols-2 md:grid-cols-6 gap-2 text-xs">
          <div>
            <label class="block text-gray-600 mb-1">Item</label>
            <input v-model.number="g.nItem" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">UF</label>
            <input v-model="g.UF" type="text" maxlength="2" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Município (cMun)</label>
            <input v-model.number="g.CodigoMunicipio" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Nome Município</label>
            <input v-model="g.NomeMunicipio" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vBCIBS</label>
            <input v-model.number="g.vBCIBS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">pIBS (%)</label>
            <input v-model.number="g.pIBS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vIBS</label>
            <input v-model.number="g.vIBS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">pDif (%)</label>
            <input v-model.number="g.pDif" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vDevTrib</label>
            <input v-model.number="g.vDevTrib" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">Monofásico</label>
            <input v-model="g.gIBSCBSMono" type="checkbox" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Benefício</label>
            <input v-model="g.CodigoBeneficio" type="text" class="w-full border rounded px-2 py-1" />
          </div>
        </div>
        <div class="mt-2 text-right">
          <button @click="removeIBS(idx)" class="text-xs text-red-600 hover:underline">Remover</button>
        </div>
      </div>
    </section>

    <!-- CBS -->
    <section class="mb-6">
      <div class="flex items-center justify-between mb-2">
        <h4 class="text-sm font-semibold text-green-800">Grupos CBS por Item</h4>
        <button @click="addCBS" class="text-xs bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700">+ Adicionar CBS</button>
      </div>
      <div v-if="(model.GruposCBS || []).length === 0" class="text-xs text-gray-500 mb-2">Nenhum grupo CBS adicionado.</div>
      <div v-for="(g, idx) in model.GruposCBS" :key="idx" class="border rounded p-3 mb-2">
        <div class="grid grid-cols-2 md:grid-cols-6 gap-2 text-xs">
          <div>
            <label class="block text-gray-600 mb-1">Item</label>
            <input v-model.number="g.nItem" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">UF</label>
            <input v-model="g.UF" type="text" maxlength="2" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Município (cMun)</label>
            <input v-model.number="g.CodigoMunicipio" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Nome Município</label>
            <input v-model="g.NomeMunicipio" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vBCCBS</label>
            <input v-model.number="g.vBCCBS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">pCBS (%)</label>
            <input v-model.number="g.pCBS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vCBS</label>
            <input v-model.number="g.vCBS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">pDif (%)</label>
            <input v-model.number="g.pDif" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vDevTrib</label>
            <input v-model.number="g.vDevTrib" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">Monofásico</label>
            <input v-model="g.gIBSCBSMono" type="checkbox" />
          </div>
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">Crédito Presumido</label>
            <input v-model="g.gCBSCredPres" type="checkbox" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Benefício</label>
            <input v-model="g.CodigoBeneficio" type="text" class="w-full border rounded px-2 py-1" />
          </div>
        </div>
        <div class="mt-2 text-right">
          <button @click="removeCBS(idx)" class="text-xs text-red-600 hover:underline">Remover</button>
        </div>
      </div>
    </section>

    <!-- IS -->
    <section class="mb-6">
      <div class="flex items-center justify-between mb-2">
        <h4 class="text-sm font-semibold text-orange-800">Grupos IS por Item (quando aplicável)</h4>
        <button @click="addIS" class="text-xs bg-orange-600 text-white px-3 py-1 rounded hover:bg-orange-700">+ Adicionar IS</button>
      </div>
      <div v-if="(model.GruposIS || []).length === 0" class="text-xs text-gray-500 mb-2">Nenhum grupo IS adicionado.</div>
      <div v-for="(g, idx) in model.GruposIS" :key="idx" class="border rounded p-3 mb-2">
        <div class="grid grid-cols-2 md:grid-cols-6 gap-2 text-xs">
          <div>
            <label class="block text-gray-600 mb-1">Item</label>
            <input v-model.number="g.nItem" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">UF</label>
            <input v-model="g.UF" type="text" maxlength="2" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Município (cMun)</label>
            <input v-model.number="g.CodigoMunicipio" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Nome Município</label>
            <input v-model="g.NomeMunicipio" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vBCIS</label>
            <input v-model.number="g.vBCIS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">pIS (%)</label>
            <input v-model.number="g.pIS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">vIS</label>
            <input v-model.number="g.vIS" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">Monofásico</label>
            <input v-model="g.gIBSCBSMono" type="checkbox" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Benefício</label>
            <input v-model="g.CodigoBeneficio" type="text" class="w-full border rounded px-2 py-1" />
          </div>
        </div>
        <div class="mt-2 text-right">
          <button @click="removeIS(idx)" class="text-xs text-red-600 hover:underline">Remover</button>
        </div>
      </div>
    </section>

    <!-- Referências de Documentos -->
    <section class="mb-6">
      <div class="flex items-center justify-between mb-2">
        <h4 class="text-sm font-semibold text-purple-800">Referências de Documentos</h4>
        <button @click="addRef" class="text-xs bg-purple-600 text-white px-3 py-1 rounded hover:bg-purple-700">+ Adicionar Referência</button>
      </div>
      <div v-if="(model.Referencias || []).length === 0" class="text-xs text-gray-500 mb-2">Nenhuma referência adicionada.</div>
      <div v-for="(r, idx) in model.Referencias" :key="idx" class="border rounded p-3 mb-2">
        <div class="grid grid-cols-2 md:grid-cols-6 gap-2 text-xs">
          <div>
            <label class="block text-gray-600 mb-1">Chave Acesso</label>
            <input v-model="r.ChaveAcessoReferenciada" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">nItem</label>
            <input v-model.number="r.nItem" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">nItem Ref.</label>
            <input v-model.number="r.nItemReferenciado" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Tipo Doc.</label>
            <input v-model="r.TipoDocumento" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Data Emissão</label>
            <input v-model="r.DataEmissaoReferenciada" type="date" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">UF</label>
            <input v-model="r.UFReferenciada" type="text" maxlength="2" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Valor</label>
            <input v-model.number="r.ValorReferenciado" type="number" step="0.01" class="w-full border rounded px-2 py-1" />
          </div>
          <div class="md:col-span-2">
            <label class="block text-gray-600 mb-1">Motivo</label>
            <input v-model="r.MotivoReferencia" type="text" class="w-full border rounded px-2 py-1" />
          </div>
        </div>
        <div class="mt-2 text-right">
          <button @click="removeRef(idx)" class="text-xs text-red-600 hover:underline">Remover</button>
        </div>
      </div>
    </section>

    <!-- Rastreabilidade -->
    <section>
      <div class="flex items-center justify-between mb-2">
        <h4 class="text-sm font-semibold text-gray-800">Rastreabilidade (GTIN/Lote)</h4>
        <button @click="addRast" class="text-xs bg-gray-700 text-white px-3 py-1 rounded hover:bg-black">+ Adicionar Rastreio</button>
      </div>
      <div v-if="(model.Rastreabilidade || []).length === 0" class="text-xs text-gray-500 mb-2">Nenhum registro de rastreabilidade.</div>
      <div v-for="(r, idx) in model.Rastreabilidade" :key="idx" class="border rounded p-3 mb-2">
        <div class="grid grid-cols-2 md:grid-cols-6 gap-2 text-xs">
          <div>
            <label class="block text-gray-600 mb-1">Item</label>
            <input v-model.number="r.nItem" type="number" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">GTIN</label>
            <input v-model="r.GTIN" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Tipo</label>
            <input v-model="r.TipoRastreabilidade" type="text" class="w-full border rounded px-2 py-1" placeholder="MEDICAMENTO, BEBIDA..." />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Lote</label>
            <input v-model="r.NumeroLote" type="text" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Fabricação</label>
            <input v-model="r.DataFabricacao" type="date" class="w-full border rounded px-2 py-1" />
          </div>
          <div>
            <label class="block text-gray-600 mb-1">Validade</label>
            <input v-model="r.DataValidade" type="date" class="w-full border rounded px-2 py-1" />
          </div>
          <div class="md:col-span-2">
            <label class="block text-gray-600 mb-1">Código Rastreamento</label>
            <input v-model="r.CodigoRastreamento" type="text" class="w-full border rounded px-2 py-1" />
          </div>
        </div>
        <div class="mt-2 text-right">
          <button @click="removeRast(idx)" class="text-xs text-red-600 hover:underline">Remover</button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { reactive, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: Object,
    required: true
  }
})
const emit = defineEmits(['update:modelValue'])

const model = reactive(props.modelValue)
// Inicializar coleções se ausentes
if (!model.GruposIBS) model.GruposIBS = []
if (!model.GruposCBS) model.GruposCBS = []
if (!model.GruposIS) model.GruposIS = []
if (!model.Referencias) model.Referencias = []
if (!model.Rastreabilidade) model.Rastreabilidade = []
if (!model.TotaisIBS) model.TotaisIBS = { vBCIBS: 0, vIBS: 0, vIBSST: 0, vFCPIBS: 0, vIBSDevolvido: 0, vIBSRetido: 0 }
if (!model.TotaisCBS) model.TotaisCBS = { vBCCBS: 0, vCBS: 0, vCBSST: 0, vFCPCBS: 0, vCBSDevolvido: 0, vCBSRetido: 0 }
if (!model.TotaisIS) model.TotaisIS = { vBCIS: 0, vIS: 0, vISST: 0, vFCPIS: 0, vISDevolvido: 0, vISRetido: 0 }
watch(model, () => emit('update:modelValue', model), { deep: true })

function addIBS() {
  model.GruposIBS.push({ nItem: 1, UF: '', CodigoMunicipio: 0, NomeMunicipio: '', vBCIBS: 0, pIBS: 0, vIBS: 0, pDif: null, vDevTrib: null, gIBSCBSMono: false, CodigoBeneficio: '' })
}
function removeIBS(i) { model.GruposIBS.splice(i, 1) }

function addCBS() {
  model.GruposCBS.push({ nItem: 1, UF: '', CodigoMunicipio: 0, NomeMunicipio: '', vBCCBS: 0, pCBS: 0, vCBS: 0, pDif: null, vDevTrib: null, gIBSCBSMono: false, gCBSCredPres: false, CodigoBeneficio: '' })
}
function removeCBS(i) { model.GruposCBS.splice(i, 1) }

function addIS() {
  model.GruposIS.push({ nItem: 1, UF: '', CodigoMunicipio: 0, NomeMunicipio: '', vBCIS: 0, pIS: 0, vIS: 0, gIBSCBSMono: false, CodigoBeneficio: '' })
}
function removeIS(i) { model.GruposIS.splice(i, 1) }

function addRef() {
  model.Referencias.push({ nItem: 1, ChaveAcessoReferenciada: '', nItemReferenciado: 0, TipoDocumento: 'NF-e', DataEmissaoReferenciada: '', UFReferenciada: '', ValorReferenciado: 0, MotivoReferencia: '' })
}
function removeRef(i) { model.Referencias.splice(i, 1) }

function addRast() {
  model.Rastreabilidade.push({ nItem: 1, GTIN: '', TipoRastreabilidade: '', NumeroLote: '', DataFabricacao: '', DataValidade: '', CodigoRastreamento: '' })
}
function removeRast(i) { model.Rastreabilidade.splice(i, 1) }
</script>
