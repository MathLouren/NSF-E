import axios from 'axios'

const CNPJ_API_BASE_URL = 'https://brasilapi.com.br/api/cnpj/v1'

/**
 * Serviço para consulta de dados de empresa via CNPJ
 */
class CnpjService {
  /**
   * Consulta dados da empresa pelo CNPJ
   * @param {string} cnpj - CNPJ sem formatação (apenas números)
   * @returns {Promise<Object>} Dados da empresa
   */
  async consultarCnpj(cnpj) {
    try {
      // Remove formatação do CNPJ
      const cnpjLimpo = cnpj.replace(/\D/g, '')
      
      // Valida se o CNPJ tem 14 dígitos
      if (cnpjLimpo.length !== 14) {
        throw new Error('CNPJ deve ter 14 dígitos')
      }

      const response = await axios.get(`${CNPJ_API_BASE_URL}/${cnpjLimpo}`, {
        timeout: 10000 // 10 segundos de timeout
      })

      if (response.data.status === 'ERROR') {
        throw new Error(response.data.message || 'Erro ao consultar CNPJ')
      }

      return this.mapearDadosEmpresa(response.data)
    } catch (error) {
      console.error('Erro ao consultar CNPJ:', error)
      throw error
    }
  }

  /**
   * Mapeia os dados da API para o formato usado no sistema
   * @param {Object} dadosApi - Dados retornados pela API
   * @returns {Object} Dados mapeados
   */
  mapearDadosEmpresa(dadosApi) {
    return {
      cnpj: dadosApi.cnpj,
      razaoSocial: dadosApi.razao_social,
      nomeFantasia: dadosApi.nome_fantasia || '',
      logradouro: dadosApi.logradouro || '',
      numero: dadosApi.numero || '',
      complemento: dadosApi.complemento || '',
      bairro: dadosApi.bairro || '',
      municipio: dadosApi.municipio || '',
      uf: dadosApi.uf || '',
      cep: dadosApi.cep || '',
      telefone: this.formatarTelefone(dadosApi.ddd_telefone_1),
      email: dadosApi.email || '',
      situacaoCadastral: dadosApi.descricao_situacao_cadastral || '',
      dataInicioAtividade: dadosApi.data_inicio_atividade || '',
      capitalSocial: dadosApi.capital_social || 0,
      porte: dadosApi.porte || '',
      naturezaJuridica: dadosApi.natureza_juridica || '',
      cnaeFiscal: dadosApi.cnae_fiscal || '',
      cnaeFiscalDescricao: dadosApi.cnae_fiscal_descricao || '',
      codigoMunicipio: dadosApi.codigo_municipio_ibge || '',
      regimeTributario: dadosApi.regime_tributario || [],
      socios: dadosApi.qsa || []
    }
  }

  /**
   * Formata o telefone removendo caracteres especiais
   * @param {string} telefone - Telefone com DDD
   * @returns {string} Telefone formatado
   */
  formatarTelefone(telefone) {
    if (!telefone) return ''
    return telefone.replace(/\D/g, '')
  }

  /**
   * Valida se o CNPJ é válido
   * @param {string} cnpj - CNPJ para validação
   * @returns {boolean} True se válido
   */
  validarCnpj(cnpj) {
    const cnpjLimpo = cnpj.replace(/\D/g, '')
    
    if (cnpjLimpo.length !== 14) return false
    
    // Verifica se todos os dígitos são iguais
    if (/^(\d)\1+$/.test(cnpjLimpo)) return false
    
    // Validação dos dígitos verificadores
    let tamanho = cnpjLimpo.length - 2
    let numeros = cnpjLimpo.substring(0, tamanho)
    let digitos = cnpjLimpo.substring(tamanho)
    let soma = 0
    let pos = tamanho - 7
    
    for (let i = tamanho; i >= 1; i--) {
      soma += numeros.charAt(tamanho - i) * pos--
      if (pos < 2) pos = 9
    }
    
    let resultado = soma % 11 < 2 ? 0 : 11 - soma % 11
    if (resultado != digitos.charAt(0)) return false
    
    tamanho = tamanho + 1
    numeros = cnpjLimpo.substring(0, tamanho)
    soma = 0
    pos = tamanho - 7
    
    for (let i = tamanho; i >= 1; i--) {
      soma += numeros.charAt(tamanho - i) * pos--
      if (pos < 2) pos = 9
    }
    
    resultado = soma % 11 < 2 ? 0 : 11 - soma % 11
    return resultado == digitos.charAt(1)
  }

  /**
   * Formata CNPJ com máscara
   * @param {string} cnpj - CNPJ sem formatação
   * @returns {string} CNPJ formatado
   */
  formatarCnpj(cnpj) {
    const cnpjLimpo = cnpj.replace(/\D/g, '')
    return cnpjLimpo.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
  }
}

export default new CnpjService()
