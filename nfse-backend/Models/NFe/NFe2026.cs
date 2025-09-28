using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nfse_backend.Models.NFe
{
    /// <summary>
    /// Modelos de dados para NF-e 2026 conforme NT 2025.002
    /// Implementa os novos grupos obrigatórios: W03 (IBS/CBS/IS), VB (Totais), VC (Referências)
    /// </summary>
    
    public class NFe2026 : NFe
    {
        public NFe2026()
        {
            Versao = "4.00"; // Manter compatibilidade com layout atual
            // Novos grupos obrigatórios para 2026
            GruposIBS = new List<GrupoIBS>();
            GruposCBS = new List<GrupoCBS>();
            GruposIS = new List<GrupoIS>();
            TotaisIBS = new TotalIBS();
            TotaisCBS = new TotalCBS();
            TotaisIS = new TotalIS();
            Referencias = new List<ReferenciaDocumento>();
            Rastreabilidade = new List<RastreabilidadeItem>();
            EventosFiscais = new List<EventoFiscal>();
        }

        // Grupo W03 - IBS (Imposto sobre Bens e Serviços)
        public List<GrupoIBS> GruposIBS { get; set; }
        public TotalIBS TotaisIBS { get; set; }

        // Grupo W03 - CBS (Contribuição sobre Bens e Serviços)
        public List<GrupoCBS> GruposCBS { get; set; }
        public TotalCBS TotaisCBS { get; set; }

        // Grupo W03 - IS (Imposto Seletivo)
        public List<GrupoIS> GruposIS { get; set; }
        public TotalIS TotaisIS { get; set; }

        // Grupo VB - Totais por UF/Município
        public List<TotalPorUF> TotaisPorUF { get; set; } = new List<TotalPorUF>();
        public List<TotalPorMunicipio> TotaisPorMunicipio { get; set; } = new List<TotalPorMunicipio>();

        // Grupo VC - Referências de Documentos
        public List<ReferenciaDocumento> Referencias { get; set; }

        // Rastreabilidade (GTIN, medicamentos, bebidas)
        public List<RastreabilidadeItem> Rastreabilidade { get; set; }

        // Eventos Fiscais
        public List<EventoFiscal> EventosFiscais { get; set; }

        // Auditoria e Versionamento
        public string VersaoLayout { get; set; } = "2026.001";
        public DateTime DataAtualizacaoLayout { get; set; } = DateTime.UtcNow;
        public string? HashValidacao { get; set; }
    }

    #region Grupos IBS/CBS/IS (W03)

    public class GrupoIBS
    {
        public int nItem { get; set; }
        public string UF { get; set; } = string.Empty;
        public int CodigoMunicipio { get; set; }
        public string NomeMunicipio { get; set; } = string.Empty;
        
        // Campos obrigatórios IBS
        public decimal vBCIBS { get; set; } // Base de cálculo IBS
        public decimal pIBS { get; set; } // Alíquota IBS (%)
        public decimal vIBS { get; set; } // Valor IBS
        
        // Campos condicionais
        public decimal? pDif { get; set; } // Percentual diferencial
        public decimal? vDevTrib { get; set; } // Valor devolvido tributário
        public bool gIBSCBSMono { get; set; } // Indicador monofásico
        public bool gCBSCredPres { get; set; } // Crédito presumido
        
        // Classificação tributária
        public string ClassificacaoTributaria { get; set; } = string.Empty; // IBS/CBS/IS
        public string CodigoBeneficio { get; set; } = string.Empty;
        public string TipoOperacao { get; set; } = string.Empty; // VENDA, DEVOLUCAO, CREDITO, etc.
    }

    public class GrupoCBS
    {
        public int nItem { get; set; }
        public string UF { get; set; } = string.Empty;
        public int CodigoMunicipio { get; set; }
        public string NomeMunicipio { get; set; } = string.Empty;
        
        // Campos obrigatórios CBS
        public decimal vBCCBS { get; set; } // Base de cálculo CBS
        public decimal pCBS { get; set; } // Alíquota CBS (%)
        public decimal vCBS { get; set; } // Valor CBS
        
        // Campos condicionais
        public decimal? pDif { get; set; }
        public decimal? vDevTrib { get; set; }
        public bool gIBSCBSMono { get; set; }
        public bool gCBSCredPres { get; set; }
        
        public string ClassificacaoTributaria { get; set; } = string.Empty;
        public string CodigoBeneficio { get; set; } = string.Empty;
        public string TipoOperacao { get; set; } = string.Empty;
    }

    public class GrupoIS
    {
        public int nItem { get; set; }
        public string UF { get; set; } = string.Empty;
        public int CodigoMunicipio { get; set; }
        public string NomeMunicipio { get; set; } = string.Empty;
        
        // Campos obrigatórios IS
        public decimal vBCIS { get; set; } // Base de cálculo IS
        public decimal pIS { get; set; } // Alíquota IS (%)
        public decimal vIS { get; set; } // Valor IS
        
        // Campos condicionais
        public decimal? pDif { get; set; }
        public decimal? vDevTrib { get; set; }
        public bool gIBSCBSMono { get; set; }
        public bool gCBSCredPres { get; set; }
        
        public string ClassificacaoTributaria { get; set; } = string.Empty;
        public string CodigoBeneficio { get; set; } = string.Empty;
        public string TipoOperacao { get; set; } = string.Empty;
    }

    #endregion

    #region Totais (VB)

    public class TotalIBS
    {
        public decimal vBCIBS { get; set; }
        public decimal vIBS { get; set; }
        public decimal vIBSST { get; set; } // Substituição tributária
        public decimal vFCPIBS { get; set; } // Fundo de combate à pobreza
        public decimal vIBSDevolvido { get; set; } // IBS devolvido
        public decimal vIBSRetido { get; set; } // IBS retido
    }

    public class TotalCBS
    {
        public decimal vBCCBS { get; set; }
        public decimal vCBS { get; set; }
        public decimal vCBSST { get; set; }
        public decimal vFCPCBS { get; set; }
        public decimal vCBSDevolvido { get; set; }
        public decimal vCBSRetido { get; set; }
    }

    public class TotalIS
    {
        public decimal vBCIS { get; set; }
        public decimal vIS { get; set; }
        public decimal vISST { get; set; }
        public decimal vFCPIS { get; set; }
        public decimal vISDevolvido { get; set; }
        public decimal vISRetido { get; set; }
    }

    public class TotalPorUF
    {
        public string UF { get; set; } = string.Empty;
        public decimal vBCIBS { get; set; }
        public decimal vIBS { get; set; }
        public decimal vBCCBS { get; set; }
        public decimal vCBS { get; set; }
        public decimal vBCIS { get; set; }
        public decimal vIS { get; set; }
        public int QuantidadeItens { get; set; }
    }

    public class TotalPorMunicipio
    {
        public string UF { get; set; } = string.Empty;
        public int CodigoMunicipio { get; set; }
        public string NomeMunicipio { get; set; } = string.Empty;
        public decimal vBCIBS { get; set; }
        public decimal vIBS { get; set; }
        public decimal vBCCBS { get; set; }
        public decimal vCBS { get; set; }
        public decimal vBCIS { get; set; }
        public decimal vIS { get; set; }
        public int QuantidadeItens { get; set; }
    }

    #endregion

    #region Referências de Documentos (VC)

    public class ReferenciaDocumento
    {
        public int nItem { get; set; }
        public string ChaveAcessoReferenciada { get; set; } = string.Empty;
        public int nItemReferenciado { get; set; }
        public string TipoDocumento { get; set; } = string.Empty; // NF-e, NFC-e, NFS-e, etc.
        public DateTime DataEmissaoReferenciada { get; set; }
        public string UFReferenciada { get; set; } = string.Empty;
        public decimal ValorReferenciado { get; set; }
        public string MotivoReferencia { get; set; } = string.Empty; // DEVOLUCAO, CREDITO, etc.
    }

    #endregion

    #region Rastreabilidade

    public class RastreabilidadeItem
    {
        public int nItem { get; set; }
        public string GTIN { get; set; } = string.Empty; // Global Trade Item Number
        public string TipoRastreabilidade { get; set; } = string.Empty; // MEDICAMENTO, BEBIDA, COMBUSTIVEL, etc.
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime? DataFabricacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string CodigoRastreamento { get; set; } = string.Empty;
        public string InformacoesAdicionais { get; set; } = string.Empty;
    }

    #endregion

    #region Eventos Fiscais

    public class EventoFiscal
    {
        public string TipoEvento { get; set; } = string.Empty; // CREDITO_PRESUMIDO, PERDA_ROUBO, CANCELAMENTO, TRANSFERENCIA_CREDITO
        public DateTime DataEvento { get; set; }
        public string DescricaoEvento { get; set; } = string.Empty;
        public decimal? ValorEvento { get; set; }
        public string DocumentoReferencia { get; set; } = string.Empty;
        public string Justificativa { get; set; } = string.Empty;
        public string StatusEvento { get; set; } = "PENDENTE";
        public string ProtocoloEvento { get; set; } = string.Empty;
    }

    #endregion

    #region Validação e Conformidade

    public class ValidacaoNFE2026
    {
        public bool SchemaValido { get; set; }
        public bool RegrasNegocioValidas { get; set; }
        public bool CodigosTributariosValidos { get; set; }
        public bool RastreabilidadeValida { get; set; }
        public bool TotaisCalculadosCorretamente { get; set; }
        public List<string> ErrosValidacao { get; set; } = new List<string>();
        public List<string> AvisosValidacao { get; set; } = new List<string>();
        public DateTime DataValidacao { get; set; } = DateTime.UtcNow;
    }

    public class ConformidadeLayout
    {
        public string VersaoLayout { get; set; } = string.Empty;
        public string NumeroNotaTecnica { get; set; } = string.Empty;
        public DateTime DataVigencia { get; set; }
        public bool Conforme { get; set; }
        public List<string> Inconformidades { get; set; } = new List<string>();
        public List<string> Observacoes { get; set; } = new List<string>();
        public DateTime DataVerificacao { get; set; } = DateTime.UtcNow;
    }

    #endregion
}
