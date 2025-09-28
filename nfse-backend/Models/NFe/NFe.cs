using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nfse_backend.Models.NFe
{
    public class NFe
    {
        [Key]
        public Guid Id { get; set; }
        
        // infNFe - Informações da NF-e
        public string Versao { get; set; } = "4.00";
        public string ChaveAcesso { get; set; }
        
        // ide - Identificação da NF-e
        public IdentificacaoNFe Ide { get; set; } = new IdentificacaoNFe();
        
        // emit - Emitente
        public Emitente Emit { get; set; } = new Emitente();
        
        // dest - Destinatário
        public Destinatario Dest { get; set; } = new Destinatario();
        
        // det - Detalhamento de Produtos e Serviços
        public List<DetalheNFe> Det { get; set; } = new List<DetalheNFe>();
        
        // total - Totais da NF-e
        public TotalNFe Total { get; set; } = new TotalNFe();
        
        // transp - Transporte
        public Transporte Transp { get; set; } = new Transporte();
        
        // cobr - Cobrança
        public Cobranca Cobr { get; set; } = new Cobranca();
        
        // pag - Pagamento
        public Pagamento Pag { get; set; } = new Pagamento();
        
        // infAdic - Informações Adicionais
        public InformacaoAdicional InfAdic { get; set; } = new InformacaoAdicional();
        
        // Signature - Assinatura Digital
        public string Signature { get; set; }
        
        // Protocol info
        public string Protocolo { get; set; }
        public DateTime? DataAutorizacao { get; set; }
        public string Status { get; set; } = "Pendente";
        public string MotivoRejeicao { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class IdentificacaoNFe
    {
        public int cUF { get; set; } // Código da UF do emitente
        public string cNF { get; set; } // Código numérico da NF-e
        public string natOp { get; set; } // Natureza da Operação
        public int mod { get; set; } = 55; // Modelo (55 para NF-e)
        public int serie { get; set; } = 1; // Série da NF-e
        public int nNF { get; set; } // Número da NF-e
        public DateTime? dhEmi { get; set; } = DateTime.Now; // Data e hora de emissão
        public DateTime? dhSaiEnt { get; set; } // Data e hora de saída/entrada
        public int tpNF { get; set; } = 1; // Tipo (0=Entrada, 1=Saída)
        public int idDest { get; set; } = 1; // Identificação do destino (1=Operação interna)
        public int cMunFG { get; set; } // Código do município de ocorrência do fato gerador
        public int tpImp { get; set; } = 1; // Formato de impressão do DANFE
        public int tpEmis { get; set; } = 1; // Tipo de emissão (1=Normal, 2=FS-DA, 8=EPEC)
        public int cDV { get; set; } // Dígito verificador
        public int tpAmb { get; set; } = 2; // Ambiente (1=Produção, 2=Homologação)
        public int finNFe { get; set; } = 1; // Finalidade (1=Normal)
        public int indFinal { get; set; } = 1; // Consumidor final (0=Não, 1=Sim)
        public int indPres { get; set; } = 1; // Indicador de presença
        public int procEmi { get; set; } = 0; // Processo de emissão (0=Aplicativo contribuinte)
        public string verProc { get; set; } = "1.0.0"; // Versão do processo
        
        // Campos para contingência
        public DateTime? dhCont { get; set; } // Data e hora da contingência
        public string xJust { get; set; } = ""; // Justificativa da contingência
    }

    public class Emitente
    {
        public string CNPJ { get; set; }
        public string CPF { get; set; }
        public string xNome { get; set; } // Razão Social
        public string xFant { get; set; } // Nome Fantasia
        public EnderecoNFe EnderEmit { get; set; } = new EnderecoNFe();
        public string IE { get; set; } // Inscrição Estadual
        public string IEST { get; set; } // IE do Substituto Tributário
        public string IM { get; set; } // Inscrição Municipal
        public string CNAE { get; set; } // CNAE fiscal
        public int CRT { get; set; } // Código de Regime Tributário
    }

    public class Destinatario
    {
        public string CNPJ { get; set; }
        public string CPF { get; set; }
        public string idEstrangeiro { get; set; }
        public string xNome { get; set; } // Razão Social ou Nome
        public EnderecoNFe EnderDest { get; set; } = new EnderecoNFe();
        public int indIEDest { get; set; } = 9; // Indicador da IE (9=Não Contribuinte)
        public string IE { get; set; } // Inscrição Estadual
        public string ISUF { get; set; } // Inscrição SUFRAMA
        public string IM { get; set; } // Inscrição Municipal
        public string email { get; set; }
    }

    public class EnderecoNFe
    {
        public string xLgr { get; set; } // Logradouro
        public string nro { get; set; } // Número
        public string xCpl { get; set; } // Complemento
        public string xBairro { get; set; } // Bairro
        public int cMun { get; set; } // Código do município (IBGE)
        public string xMun { get; set; } // Nome do município
        public string UF { get; set; } // Sigla da UF
        public string CEP { get; set; } // CEP
        public int? cPais { get; set; } = 1058; // Código do país (1058=Brasil)
        public string xPais { get; set; } = "BRASIL"; // Nome do país
        public string fone { get; set; } // Telefone
    }

    public class DetalheNFe
    {
        public int nItem { get; set; } // Número do item
        public ProdutoNFe Prod { get; set; } = new ProdutoNFe();
        public ImpostoNFe Imposto { get; set; } = new ImpostoNFe();
        public string infAdProd { get; set; } // Informações adicionais do produto
    }

    public class ProdutoNFe
    {
        public string cProd { get; set; } // Código do produto
        public string cEAN { get; set; } // Código de barras
        public string xProd { get; set; } // Descrição do produto
        public string NCM { get; set; } // Código NCM
        public string NVE { get; set; } // Nomenclatura de Valor Aduaneiro e Estatístico
        public string CEST { get; set; } // Código Especificador da Substituição Tributária
        public string indEscala { get; set; } // Indicador de escala
        public string CNPJFab { get; set; } // CNPJ do fabricante
        public string cBenef { get; set; } // Código de benefício fiscal
        public string EXTIPI { get; set; } // EX TIPI
        public int CFOP { get; set; } // Código Fiscal de Operações e Prestações
        public string uCom { get; set; } // Unidade comercial
        public decimal qCom { get; set; } // Quantidade comercial
        public decimal vUnCom { get; set; } // Valor unitário
        public decimal vProd { get; set; } // Valor total bruto
        public string cEANTrib { get; set; } // Código de barras tributável
        public string uTrib { get; set; } // Unidade tributável
        public decimal qTrib { get; set; } // Quantidade tributável
        public decimal vUnTrib { get; set; } // Valor unitário tributável
        public decimal? vFrete { get; set; } // Valor do frete
        public decimal? vSeg { get; set; } // Valor do seguro
        public decimal? vDesc { get; set; } // Valor do desconto
        public decimal? vOutro { get; set; } // Outras despesas
        public int indTot { get; set; } = 1; // Compõe valor total (0=Não, 1=Sim)
    }

    public class ImpostoNFe
    {
        public decimal vTotTrib { get; set; } // Valor total dos tributos
        public ICMS ICMS { get; set; } = new ICMS();
        public IPI IPI { get; set; } = new IPI();
        public PIS PIS { get; set; } = new PIS();
        public COFINS COFINS { get; set; } = new COFINS();
    }

    public class ICMS
    {
        public string orig { get; set; } // Origem da mercadoria
        public string CST { get; set; } // Código de Situação Tributária
        public int modBC { get; set; } // Modalidade de determinação da BC
        public decimal vBC { get; set; } // Valor da BC do ICMS
        public decimal pICMS { get; set; } // Alíquota do ICMS
        public decimal vICMS { get; set; } // Valor do ICMS
        public decimal pFCP { get; set; } // Percentual do Fundo de Combate à Pobreza
        public decimal vFCP { get; set; } // Valor do FCP
        public decimal? vBCST { get; set; } // Valor da BC do ICMS ST
        public decimal? pICMSST { get; set; } // Alíquota do ICMS ST
        public decimal? vICMSST { get; set; } // Valor do ICMS ST
        public decimal? pFCPST { get; set; } // Percentual do FCP ST
        public decimal? vFCPST { get; set; } // Valor do FCP ST
        
        // DIFAL - Diferencial de Alíquotas (EC 87/2015)
        public decimal? vICMSUFDest { get; set; } // ICMS para UF de destino
        public decimal? vICMSUFRemet { get; set; } // ICMS para UF do remetente
        public decimal? pICMSUFDest { get; set; } // Percentual ICMS UF destino
        
        // Redução de Base de Cálculo
        public decimal? pRedBC { get; set; } // Percentual de redução da BC
        public decimal? pRedBCST { get; set; } // Percentual de redução da BC ST
        
        // Margem de Valor Agregado
        public decimal? pMVAST { get; set; } // Percentual da MVA da ST
        
        // Campos para regime de estimativa
        public decimal? vBCSTRet { get; set; } // BC do ICMS ST retido
        public decimal? pST { get; set; } // Alíquota suportada pelo consumidor final
        public decimal? vICMSSTRet { get; set; } // Valor do ICMS ST retido
        public decimal? vBCFCPSTRet { get; set; } // BC do FCP ST retido
        public decimal? pFCPSTRet { get; set; } // Percentual do FCP ST retido
        public decimal? vFCPSTRet { get; set; } // Valor do FCP ST retido
        
        // Campos para diferimento
        public decimal? pDif { get; set; } // Percentual do diferimento
        public decimal? vICMSDif { get; set; } // Valor do ICMS diferido
        public decimal? vICMSOp { get; set; } // Valor do ICMS da operação
    }

    public class IPI
    {
        public string clEnq { get; set; } // Classe de enquadramento
        public string CNPJProd { get; set; } // CNPJ do produtor
        public string cSelo { get; set; } // Código do selo
        public int? qSelo { get; set; } // Quantidade de selo
        public string cEnq { get; set; } // Código de enquadramento
        public string CST { get; set; } // Código de Situação Tributária
        public decimal? vBC { get; set; } // Valor da BC do IPI
        public decimal? pIPI { get; set; } // Alíquota do IPI
        public decimal? vIPI { get; set; } // Valor do IPI
    }

    public class PIS
    {
        public string CST { get; set; } // Código de Situação Tributária
        public decimal? vBC { get; set; } // Valor da BC do PIS
        public decimal? pPIS { get; set; } // Alíquota do PIS
        public decimal? vPIS { get; set; } // Valor do PIS
    }

    public class COFINS
    {
        public string CST { get; set; } // Código de Situação Tributária
        public decimal? vBC { get; set; } // Valor da BC da COFINS
        public decimal? pCOFINS { get; set; } // Alíquota da COFINS
        public decimal? vCOFINS { get; set; } // Valor da COFINS
    }

    public class TotalNFe
    {
        public ICMSTotal ICMSTot { get; set; } = new ICMSTotal();
        public ISSQNTotal ISSQNtot { get; set; } = new ISSQNTotal();
        public RetencaoTributos retTrib { get; set; } = new RetencaoTributos();
    }

    public class ICMSTotal
    {
        public decimal vBC { get; set; } // BC do ICMS
        public decimal vICMS { get; set; } // Valor do ICMS
        public decimal vICMSDeson { get; set; } // Valor do ICMS desonerado
        public decimal vFCP { get; set; } // Valor do FCP
        public decimal vBCST { get; set; } // BC do ICMS ST
        public decimal vST { get; set; } // Valor do ICMS ST
        public decimal vFCPST { get; set; } // Valor do FCP ST
        public decimal vFCPSTRet { get; set; } // Valor do FCP ST retido
        public decimal vProd { get; set; } // Valor dos produtos
        public decimal vFrete { get; set; } // Valor do frete
        public decimal vSeg { get; set; } // Valor do seguro
        public decimal vDesc { get; set; } // Valor do desconto
        public decimal vII { get; set; } // Valor do II
        public decimal vIPI { get; set; } // Valor do IPI
        public decimal vIPIDevol { get; set; } // Valor do IPI devolvido
        public decimal vPIS { get; set; } // Valor do PIS
        public decimal vCOFINS { get; set; } // Valor da COFINS
        public decimal vOutro { get; set; } // Outras despesas
        public decimal vNF { get; set; } // Valor total da NF-e
        public decimal vTotTrib { get; set; } // Valor total dos tributos
    }

    public class ISSQNTotal
    {
        public decimal? vServ { get; set; } // Valor dos serviços
        public decimal? vBC { get; set; } // BC do ISSQN
        public decimal? vISS { get; set; } // Valor do ISS
        public decimal? vPIS { get; set; } // Valor do PIS sobre serviços
        public decimal? vCOFINS { get; set; } // Valor da COFINS sobre serviços
        public DateTime? dCompet { get; set; } // Data da prestação do serviço
        public decimal? vDeducao { get; set; } // Valor dedução
        public decimal? vOutro { get; set; } // Valor outras retenções
        public decimal? vDescIncond { get; set; } // Valor desconto incondicionado
        public decimal? vDescCond { get; set; } // Valor desconto condicionado
        public decimal? vISSRet { get; set; } // Valor ISS retido
        public int? cRegTrib { get; set; } // Código do regime especial de tributação
    }

    public class RetencaoTributos
    {
        public decimal? vRetPIS { get; set; } // Valor PIS retido
        public decimal? vRetCOFINS { get; set; } // Valor COFINS retido
        public decimal? vRetCSLL { get; set; } // Valor CSLL retido
        public decimal? vBCIRRF { get; set; } // BC do IRRF
        public decimal? vIRRF { get; set; } // Valor do IRRF
        public decimal? vBCRetPrev { get; set; } // BC da retenção da previdência social
        public decimal? vRetPrev { get; set; } // Valor da retenção da previdência social
    }

    public class Transporte
    {
        public int modFrete { get; set; } // Modalidade do frete
        public Transportador Transporta { get; set; } = new Transportador();
        public Veiculo VeicTransp { get; set; } = new Veiculo();
        public List<Volume> Vol { get; set; } = new List<Volume>();
    }

    public class Transportador
    {
        public string CNPJ { get; set; }
        public string CPF { get; set; }
        public string xNome { get; set; } // Nome/Razão Social
        public string IE { get; set; } // Inscrição Estadual
        public string xEnder { get; set; } // Endereço completo
        public string xMun { get; set; } // Nome do município
        public string UF { get; set; } // Sigla da UF
    }

    public class Veiculo
    {
        public string placa { get; set; } // Placa do veículo
        public string UF { get; set; } // UF de registro
        public string RNTC { get; set; } // Registro Nacional de Transportador de Carga
    }

    public class Volume
    {
        public int? qVol { get; set; } // Quantidade de volumes
        public string esp { get; set; } // Espécie dos volumes
        public string marca { get; set; } // Marca dos volumes
        public string nVol { get; set; } // Numeração dos volumes
        public decimal? pesoL { get; set; } // Peso líquido
        public decimal? pesoB { get; set; } // Peso bruto
    }

    public class Cobranca
    {
        public Fatura Fat { get; set; } = new Fatura();
        public List<Duplicata> Dup { get; set; } = new List<Duplicata>();
    }

    public class Fatura
    {
        public string nFat { get; set; } // Número da fatura
        public decimal? vOrig { get; set; } // Valor original
        public decimal? vDesc { get; set; } // Valor do desconto
        public decimal? vLiq { get; set; } // Valor líquido
    }

    public class Duplicata
    {
        public string nDup { get; set; } // Número da duplicata
        public DateTime? dVenc { get; set; } // Data de vencimento
        public decimal vDup { get; set; } // Valor da duplicata
    }

    public class Pagamento
    {
        public List<DetalhePagamento> DetPag { get; set; } = new List<DetalhePagamento>();
        public decimal? vTroco { get; set; } // Valor do troco
    }

    public class DetalhePagamento
    {
        public int? indPag { get; set; } // Indicador forma pagamento
        public int tPag { get; set; } // Tipo de pagamento
        public decimal vPag { get; set; } // Valor do pagamento
        public CartaoPagamento Card { get; set; } = new CartaoPagamento();
    }

    public class CartaoPagamento
    {
        public int? tpIntegra { get; set; } // Tipo de integração
        public string CNPJ { get; set; } // CNPJ da credenciadora
        public int? tBand { get; set; } // Bandeira do cartão
        public string cAut { get; set; } // Código de autorização
    }

    public class InformacaoAdicional
    {
        public string infAdFisco { get; set; } // Informações adicionais de interesse do fisco
        public string infCpl { get; set; } // Informações complementares
        public List<ObservacaoContribuinte> ObsCont { get; set; } = new List<ObservacaoContribuinte>();
        public List<ObservacaoFisco> ObsFisco { get; set; } = new List<ObservacaoFisco>();
    }

    public class ObservacaoContribuinte
    {
        public string xCampo { get; set; } // Identificação do campo
        public string xTexto { get; set; } // Conteúdo do campo
    }

    public class ObservacaoFisco
    {
        public string xCampo { get; set; } // Identificação do campo
        public string xTexto { get; set; } // Conteúdo do campo
    }
}
