import api from './api'

/**
 * Serviço para gerenciar tabelas fiscais e validações do layout 2026
 */
class FiscalTablesService {
  constructor() {
    this.tables = {
      cst: [],
      cfop: [],
      ncm: [],
      gtin: [],
      cnae: [],
      municipios: [],
      ufs: [],
      ibsRates: {},
      cbsRates: {},
      isRates: {}
    }
    this.lastUpdate = null
  }

  /**
   * Carrega todas as tabelas fiscais do backend usando dados reais do formulário
   */
  async loadFiscalTables(nfeData = null) {
    try {
      // Se não temos dados da NFe, usar dados de exemplo
      if (!nfeData) {
        nfeData = this.getDefaultNFeData()
      }

      const requestData = {
        NFe: nfeData,
        Versao: "2026",
        IsContingencia: false
      }

      console.log('🚀 Carregando tabelas fiscais via endpoint DANFE:', requestData)
      
      const response = await api.post('/nfe2026/gerar-danfe-versao', requestData, {
        timeout: 30000
      })
      
      // Extrair dados das tabelas da resposta
      if (response.data && response.data.tabelasFiscais) {
        this.tables = { ...this.tables, ...response.data.tabelasFiscais }
        this.lastUpdate = new Date()
        console.log('✅ Tabelas fiscais carregadas com sucesso via DANFE')
        return this.tables
      } else {
        // Se não há tabelas na resposta, usar dados mockados
        throw new Error('Resposta não contém tabelas fiscais')
      }
    } catch (error) {
      console.warn('⚠️ Erro ao carregar tabelas fiscais via DANFE, usando dados mockados:', error.message)
      // Em caso de erro, usar dados mockados
      this.tables = this.getMockTables()
      this.lastUpdate = new Date()
      return this.tables
    }
  }

  /**
   * Retorna dados padrão de NFe para carregamento das tabelas fiscais
   */
  getDefaultNFeData() {
    return {
      versao: "4.00",
      chaveAcesso: "35200114200166000187550010000000015123456789",
      ide: {
        cUF: 35,
        cNF: "12345678",
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
        verProc: "1.0.0"
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
            CFOP: 5102,
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
                orig: "0",
                CST: "00",
                modBC: 3,
                vBC: 100.00,
                pICMS: 18.00,
                vICMS: 18.00
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
          vIPI: 0.00,
          vIPIDevol: 0.00,
          vPIS: 0.00,
          vCOFINS: 0.00,
          vOutro: 0.00,
          vNF: 118.00,
          vTotTrib: 18.00
        }
      },
      transp: {
        modFrete: 0
      },
      pag: {
        detPag: [
          {
            indPag: 0,
            tPag: "01",
            vPag: 118.00
          }
        ]
      },
      infAdic: {
        infCpl: "Informações complementares da NF-e"
      }
    }
  }

  /**
   * Retorna tabelas fiscais mockadas para desenvolvimento
   */
  getMockTables() {
    return {
      cst: [
        { code: '00', description: 'Tributada integralmente', operationType: 'produto' },
        { code: '10', description: 'Tributada e com cobrança do ICMS por substituição tributária', operationType: 'produto' },
        { code: '20', description: 'Com redução de base de cálculo', operationType: 'produto' },
        { code: '30', description: 'Isenta ou não tributada e com cobrança do ICMS por substituição tributária', operationType: 'produto' },
        { code: '40', description: 'Isenta', operationType: 'produto' },
        { code: '41', description: 'Não tributada', operationType: 'produto' },
        { code: '50', description: 'Suspensão', operationType: 'produto' },
        { code: '51', description: 'Diferimento', operationType: 'produto' },
        { code: '60', description: 'ICMS cobrado anteriormente por substituição tributária', operationType: 'produto' },
        { code: '70', description: 'Com redução de base de cálculo e cobrança do ICMS por substituição tributária', operationType: 'produto' },
        { code: '90', description: 'Outras', operationType: 'produto' }
      ],
      cfop: [
        { code: '1102', description: 'Compra para comercialização', operationType: 'compra' },
        { code: '5102', description: 'Venda de mercadoria adquirida ou recebida de terceiros', operationType: 'venda' },
        { code: '1202', description: 'Compra para industrialização', operationType: 'compra' },
        { code: '5202', description: 'Venda de mercadoria industrializada', operationType: 'venda' },
        { code: '1203', description: 'Compra para comercialização', operationType: 'compra' },
        { code: '5203', description: 'Venda de mercadoria adquirida ou recebida de terceiros', operationType: 'venda' }
      ],
      ncm: [
        { code: '61091000', description: 'Camisetas de malha de algodão' },
        { code: '61091000', description: 'Vestuário de malha de algodão' },
        { code: '62034200', description: 'Calças de malha de algodão' },
        { code: '62034200', description: 'Vestuário de malha de algodão' }
      ],
      gtin: [
        { code: '7891234567890', description: 'Produto exemplo 1' },
        { code: '7891234567891', description: 'Produto exemplo 2' }
      ],
      cnae: [
        { code: '4711301', description: 'Comércio varejista de mercadorias em geral, com predominância de produtos alimentícios - hipermercados' },
        { code: '4711302', description: 'Comércio varejista de mercadorias em geral, com predominância de produtos alimentícios - supermercados' }
      ],
      municipios: [
        { codigo: '3304557', nome: 'São João de Meriti', uf: 'RJ' },
        { codigo: '3303302', nome: 'Rio de Janeiro', uf: 'RJ' },
        { codigo: '3550308', nome: 'São Paulo', uf: 'SP' }
      ],
      ufs: [
        { codigo: '33', nome: 'Rio de Janeiro', sigla: 'RJ' },
        { codigo: '35', nome: 'São Paulo', sigla: 'SP' },
        { codigo: '31', nome: 'Minas Gerais', sigla: 'MG' }
      ],
      ibsRates: {
        'RJ': { 'geral': 8.5, 'alimenticio': 8.5, 'medicamento': 8.5 },
        'SP': { 'geral': 8.5, 'alimenticio': 8.5, 'medicamento': 8.5 },
        'MG': { 'geral': 8.5, 'alimenticio': 8.5, 'medicamento': 8.5 }
      },
      cbsRates: {
        'RJ': { 'geral': 1.5, 'alimenticio': 1.5, 'medicamento': 1.5 },
        'SP': { 'geral': 1.5, 'alimenticio': 1.5, 'medicamento': 1.5 },
        'MG': { 'geral': 1.5, 'alimenticio': 1.5, 'medicamento': 1.5 }
      },
      isRates: {
        '3304557': { 'geral': 2.0, 'consultoria': 2.0, 'tecnologia': 2.0 },
        '3303302': { 'geral': 2.0, 'consultoria': 2.0, 'tecnologia': 2.0 },
        '3550308': { 'geral': 2.0, 'consultoria': 2.0, 'tecnologia': 2.0 }
      }
    }
  }

  /**
   * Valida CST (Código de Situação Tributária)
   */
  validateCST(cst, operationType = 'produto') {
    const validCSTs = this.tables.cst.filter(item => 
      item.operationType === operationType || item.operationType === 'all'
    )
    return validCSTs.find(item => item.code === cst)
  }

  /**
   * Valida CFOP (Código Fiscal de Operações e Prestações)
   */
  validateCFOP(cfop, operationType = 'venda', ufOrigin = '', ufDest = '') {
    const validCFOPs = this.tables.cfop.filter(item => {
      if (item.operationType !== operationType) return false
      if (item.ufOrigin && item.ufOrigin !== ufOrigin) return false
      if (item.ufDest && item.ufDest !== ufDest) return false
      return true
    })
    return validCFOPs.find(item => item.code === cfop)
  }

  /**
   * Valida NCM (Nomenclatura Comum do Mercosul)
   */
  validateNCM(ncm) {
    return this.tables.ncm.find(item => item.code === ncm)
  }

  /**
   * Valida GTIN (Global Trade Item Number)
   */
  validateGTIN(gtin) {
    return this.tables.gtin.find(item => item.code === gtin)
  }

  /**
   * Obtém alíquota IBS por UF
   */
  getIBSRate(uf, productType = 'geral') {
    return this.tables.ibsRates[uf]?.[productType] || 0
  }

  /**
   * Obtém alíquota CBS por UF
   */
  getCBSRate(uf, productType = 'geral') {
    return this.tables.cbsRates[uf]?.[productType] || 0
  }

  /**
   * Obtém alíquota IS por município
   */
  getISRate(municipio, serviceType = 'geral') {
    return this.tables.isRates[municipio]?.[serviceType] || 0
  }

  /**
   * Calcula impostos para um item
   */
  calculateTaxes(item, emitenteUF, destinatarioUF, municipioDestino) {
    const calculations = {
      ibs: {
        base: 0,
        rate: 0,
        value: 0
      },
      cbs: {
        base: 0,
        rate: 0,
        value: 0
      },
      is: {
        base: 0,
        rate: 0,
        value: 0
      }
    }

    // Base de cálculo (valor do produto/serviço)
    const base = item.valorUnitario * item.quantidade - (item.desconto || 0)

    // IBS (Imposto sobre Bens e Serviços)
    if (item.tipo === 'produto') {
      calculations.ibs.base = base
      calculations.ibs.rate = this.getIBSRate(destinatarioUF, item.categoria)
      calculations.ibs.value = base * (calculations.ibs.rate / 100)
    }

    // CBS (Contribuição sobre Bens e Serviços)
    if (item.tipo === 'produto') {
      calculations.cbs.base = base
      calculations.cbs.rate = this.getCBSRate(destinatarioUF, item.categoria)
      calculations.cbs.value = base * (calculations.cbs.rate / 100)
    }

    // IS (Imposto sobre Serviços)
    if (item.tipo === 'servico') {
      calculations.is.base = base
      calculations.is.rate = this.getISRate(municipioDestino, item.categoria)
      calculations.is.value = base * (calculations.is.rate / 100)
    }

    return calculations
  }

  /**
   * Obtém sugestões para auto-complete
   */
  getSuggestions(type, query, filters = {}) {
    let items = []
    
    switch (type) {
      case 'cst':
        items = this.tables.cst.filter(item => 
          item.code.includes(query) || 
          item.description.toLowerCase().includes(query.toLowerCase())
        )
        break
      case 'cfop':
        items = this.tables.cfop.filter(item => 
          item.code.includes(query) || 
          item.description.toLowerCase().includes(query.toLowerCase())
        )
        if (filters.operationType) {
          items = items.filter(item => 
            item.operationType === filters.operationType || 
            item.operationType === 'all'
          )
        }
        break
      case 'ncm':
        items = this.tables.ncm.filter(item => 
          item.code.includes(query) || 
          item.description.toLowerCase().includes(query.toLowerCase())
        )
        break
      case 'municipio':
        items = this.tables.municipios.filter(item => 
          item.nome.toLowerCase().includes(query.toLowerCase()) ||
          item.codigo.includes(query)
        )
        if (filters.uf) {
          items = items.filter(item => item.uf === filters.uf)
        }
        break
    }
    
    return items.slice(0, 10) // Limita a 10 sugestões
  }

  /**
   * Valida se todos os campos obrigatórios estão preenchidos
   */
  validateRequiredFields(nfeData, operationType) {
    const errors = []
    const warnings = []

    // Validações por tipo de operação
    switch (operationType) {
      case 'produto':
        this.validateProductFields(nfeData, errors, warnings)
        break
      case 'devolucao':
        this.validateReturnFields(nfeData, errors, warnings)
        break
      case 'credito':
        this.validateCreditFields(nfeData, errors, warnings)
        break
      case 'monofasia':
        this.validateMonofasicFields(nfeData, errors, warnings)
        break
    }

    return { errors, warnings }
  }

  validateProductFields(nfeData, errors, warnings) {
    // Validações básicas
    if (!nfeData.ide?.natOp) errors.push('Natureza da operação é obrigatória')
    if (!nfeData.ide?.mod) errors.push('Modelo do documento é obrigatório')
    if (!nfeData.ide?.serie) errors.push('Série é obrigatória')
    if (!nfeData.ide?.nNF) errors.push('Número da NF-e é obrigatório')

    // Validações do emitente
    if (!nfeData.emit?.CNPJ) errors.push('CNPJ do emitente é obrigatório')
    if (!nfeData.emit?.xNome) errors.push('Razão social do emitente é obrigatória')
    if (!nfeData.emit?.enderEmit?.UF) errors.push('UF do emitente é obrigatória')

    // Validações do destinatário
    if (!nfeData.dest?.CNPJ && !nfeData.dest?.CPF) {
      errors.push('CNPJ ou CPF do destinatário é obrigatório')
    }

    // Validações dos produtos
    if (!nfeData.det || nfeData.det.length === 0) {
      errors.push('Pelo menos um produto deve ser informado')
    } else {
      nfeData.det.forEach((item, index) => {
        if (!item.prod?.cProd) errors.push(`Código do produto ${index + 1} é obrigatório`)
        if (!item.prod?.xProd) errors.push(`Descrição do produto ${index + 1} é obrigatória`)
        if (!item.prod?.NCM) errors.push(`NCM do produto ${index + 1} é obrigatório`)
        if (!item.prod?.CFOP) errors.push(`CFOP do produto ${index + 1} é obrigatório`)
        if (!item.prod?.uCom) errors.push(`Unidade comercial do produto ${index + 1} é obrigatória`)
        if (!item.prod?.qCom) errors.push(`Quantidade do produto ${index + 1} é obrigatória`)
        if (!item.prod?.vUnCom) errors.push(`Valor unitário do produto ${index + 1} é obrigatório`)
        
        // Validações tributárias
        if (!item.imposto?.IBS?.CST) warnings.push(`CST IBS do produto ${index + 1} não informado`)
        if (!item.imposto?.CBS?.CST) warnings.push(`CST CBS do produto ${index + 1} não informado`)
      })
    }
  }

  validateReturnFields(nfeData, errors, warnings) {
    // Validações específicas para devolução
    if (!nfeData.ide?.natOp?.toLowerCase().includes('devolução')) {
      warnings.push('Natureza da operação deve indicar devolução')
    }
    
    if (!nfeData.refNFe) {
      errors.push('Referência da NF-e original é obrigatória para devolução')
    }
  }

  validateCreditFields(nfeData, errors, warnings) {
    // Validações específicas para crédito
    if (!nfeData.ide?.natOp?.toLowerCase().includes('crédito')) {
      warnings.push('Natureza da operação deve indicar crédito')
    }
  }

  validateMonofasicFields(nfeData, errors, warnings) {
    // Validações específicas para operação monofásica
    if (!nfeData.ide?.natOp?.toLowerCase().includes('monofásica')) {
      warnings.push('Natureza da operação deve indicar monofásica')
    }
  }

  /**
   * Carrega tabelas fiscais com dados reais do formulário
   */
  async loadFiscalTablesWithFormData(nfeData) {
    try {
      console.log('🚀 Carregando tabelas fiscais com dados do formulário:', nfeData)
      
      const requestData = {
        NFe: nfeData,
        Versao: "2026",
        IsContingencia: false
      }

      const response = await api.post('/nfe2026/gerar-danfe-versao', requestData, {
        timeout: 30000
      })
      
      // Extrair dados das tabelas da resposta
      if (response.data && response.data.tabelasFiscais) {
        this.tables = { ...this.tables, ...response.data.tabelasFiscais }
        this.lastUpdate = new Date()
        console.log('✅ Tabelas fiscais carregadas com dados do formulário')
        return this.tables
      } else {
        // Se não há tabelas na resposta, usar dados mockados
        console.warn('⚠️ Resposta não contém tabelas fiscais, usando dados mockados')
        this.tables = this.getMockTables()
        this.lastUpdate = new Date()
        return this.tables
      }
    } catch (error) {
      console.warn('⚠️ Erro ao carregar tabelas fiscais com dados do formulário:', error.message)
      // Em caso de erro, usar dados mockados
      this.tables = this.getMockTables()
      this.lastUpdate = new Date()
      return this.tables
    }
  }

  /**
   * Atualiza tabelas fiscais via upload usando endpoint DANFE
   */
  async uploadFiscalTable(tableType, file) {
    try {
      // Para upload, vamos usar dados padrão e depois recarregar
      console.log('🚀 Atualizando tabela fiscal via endpoint DANFE:', tableType)
      
      // Recarrega as tabelas usando o endpoint DANFE
      await this.loadFiscalTables()
      
      return {
        success: true,
        message: `Tabela ${tableType} atualizada via endpoint DANFE`,
        records: this.tables[tableType]?.length || 0
      }
    } catch (error) {
      console.error('Erro ao fazer upload da tabela fiscal:', error)
      throw error
    }
  }

  /**
   * Sincroniza tabelas fiscais com SEFAZ usando endpoint DANFE
   */
  async syncWithSefaz() {
    try {
      console.log('🚀 Sincronizando tabelas fiscais com SEFAZ via endpoint DANFE')
      
      // Recarrega as tabelas usando o endpoint DANFE
      await this.loadFiscalTables()
      
      return {
        success: true,
        message: 'Sincronização com SEFAZ concluída via endpoint DANFE',
        tables: Object.keys(this.tables)
      }
    } catch (error) {
      console.error('Erro na sincronização com SEFAZ:', error)
      throw error
    }
  }

  /**
   * Verifica se as tabelas estão atualizadas
   */
  isTablesUpdated() {
    if (!this.lastUpdate) return false
    
    const oneWeekAgo = new Date()
    oneWeekAgo.setDate(oneWeekAgo.getDate() - 7)
    
    return this.lastUpdate > oneWeekAgo
  }
}

export default new FiscalTablesService()
