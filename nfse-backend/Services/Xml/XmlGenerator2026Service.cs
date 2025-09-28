using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using nfse_backend.Models.NFe;

namespace nfse_backend.Services.Xml
{
    /// <summary>
    /// Gerador de XML para NF-e 2026 conforme NT 2025.002
    /// Implementa os novos grupos obrigatórios: W03 (IBS/CBS/IS), VB (Totais), VC (Referências)
    /// </summary>
    public class XmlGenerator2026Service : XmlGeneratorService
    {
        private readonly XNamespace nfe = "http://www.portalfiscal.inf.br/nfe";

        public string GenerateNFe2026Xml(NFe2026 nfe2026)
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(nfe + "NFe",
                    new XAttribute("xmlns", nfe),
                    GenerateInfNFe2026(nfe2026)
                )
            );

            return xml.ToString();
        }

        private XElement GenerateInfNFe2026(NFe2026 nfe2026)
        {
            var infNFe = new XElement(nfe + "infNFe",
                new XAttribute("versao", nfe2026.Versao),
                new XAttribute("Id", GenerateNFeId(nfe2026))
            );

            // Grupos existentes (herdados)
            infNFe.Add(GenerateIde(nfe2026.Ide));
            infNFe.Add(GenerateEmit(nfe2026.Emit));
            infNFe.Add(GenerateDest(nfe2026.Dest));

            // det - Detalhamento com novos grupos 2026
            foreach (var det in nfe2026.Det)
            {
                infNFe.Add(GenerateDet2026(det, nfe2026));
            }

            // total - Totais com novos campos 2026
            infNFe.Add(GenerateTotal2026(nfe2026));

            // transp - Transporte
            infNFe.Add(GenerateTransp(nfe2026.Transp));

            // cobr - Cobrança (opcional)
            if (nfe2026.Cobr != null && nfe2026.Cobr.Fat != null)
            {
                infNFe.Add(GenerateCobr(nfe2026.Cobr));
            }

            // pag - Pagamento
            infNFe.Add(GeneratePag(nfe2026.Pag));

            // infAdic - Informações Adicionais (opcional)
            if (nfe2026.InfAdic != null && (!string.IsNullOrEmpty(nfe2026.InfAdic.infCpl) || !string.IsNullOrEmpty(nfe2026.InfAdic.infAdFisco)))
            {
                infNFe.Add(GenerateInfAdic(nfe2026.InfAdic));
            }

            return infNFe;
        }

        private XElement GenerateDet2026(DetalheNFe det, NFe2026 nfe2026)
        {
            var detElement = new XElement(nfe + "det",
                new XAttribute("nItem", det.nItem),
                GenerateProd(det.Prod),
                GenerateImposto(det.Imposto)
            );

            // Grupo W03 - IBS/CBS/IS
            detElement.Add(GenerateGrupoW03(det.nItem, nfe2026));

            // Grupo VC - Referências de Documentos
            var referencias = nfe2026.Referencias.Where(r => r.nItem == det.nItem);
            if (referencias.Any())
            {
                detElement.Add(GenerateGrupoVC(referencias));
            }

            // Rastreabilidade
            var rastreabilidade = nfe2026.Rastreabilidade.Where(r => r.nItem == det.nItem);
            if (rastreabilidade.Any())
            {
                detElement.Add(GenerateRastreabilidade(rastreabilidade));
            }

            if (!string.IsNullOrEmpty(det.infAdProd))
                detElement.Add(new XElement(nfe + "infAdProd", det.infAdProd));

            return detElement;
        }

        private XElement GenerateGrupoW03(int nItem, NFe2026 nfe2026)
        {
            var grupoW03 = new XElement(nfe + "W03");

            // IBS
            var gruposIBS = nfe2026.GruposIBS.Where(g => g.nItem == nItem);
            foreach (var grupo in gruposIBS)
            {
                grupoW03.Add(GenerateGrupoIBS(grupo));
            }

            // CBS
            var gruposCBS = nfe2026.GruposCBS.Where(g => g.nItem == nItem);
            foreach (var grupo in gruposCBS)
            {
                grupoW03.Add(GenerateGrupoCBS(grupo));
            }

            // IS
            var gruposIS = nfe2026.GruposIS.Where(g => g.nItem == nItem);
            foreach (var grupo in gruposIS)
            {
                grupoW03.Add(GenerateGrupoIS(grupo));
            }

            return grupoW03;
        }

        private XElement GenerateGrupoIBS(GrupoIBS grupo)
        {
            var grupoIBS = new XElement(nfe + "IBS",
                new XElement(nfe + "UF", grupo.UF),
                new XElement(nfe + "cMun", grupo.CodigoMunicipio),
                new XElement(nfe + "vBCIBS", grupo.vBCIBS.ToString("F2")),
                new XElement(nfe + "pIBS", grupo.pIBS.ToString("F2")),
                new XElement(nfe + "vIBS", grupo.vIBS.ToString("F2"))
            );

            // Campos condicionais
            if (grupo.pDif.HasValue)
                grupoIBS.Add(new XElement(nfe + "pDif", grupo.pDif.Value.ToString("F2")));

            if (grupo.vDevTrib.HasValue)
                grupoIBS.Add(new XElement(nfe + "vDevTrib", grupo.vDevTrib.Value.ToString("F2")));

            if (grupo.gIBSCBSMono)
                grupoIBS.Add(new XElement(nfe + "gIBSCBSMono", "1"));

            if (grupo.gCBSCredPres)
                grupoIBS.Add(new XElement(nfe + "gCBSCredPres", "1"));

            if (!string.IsNullOrEmpty(grupo.ClassificacaoTributaria))
                grupoIBS.Add(new XElement(nfe + "classTrib", grupo.ClassificacaoTributaria));

            if (!string.IsNullOrEmpty(grupo.CodigoBeneficio))
                grupoIBS.Add(new XElement(nfe + "cBenef", grupo.CodigoBeneficio));

            if (!string.IsNullOrEmpty(grupo.TipoOperacao))
                grupoIBS.Add(new XElement(nfe + "tpOp", grupo.TipoOperacao));

            return grupoIBS;
        }

        private XElement GenerateGrupoCBS(GrupoCBS grupo)
        {
            var grupoCBS = new XElement(nfe + "CBS",
                new XElement(nfe + "UF", grupo.UF),
                new XElement(nfe + "cMun", grupo.CodigoMunicipio),
                new XElement(nfe + "vBCCBS", grupo.vBCCBS.ToString("F2")),
                new XElement(nfe + "pCBS", grupo.pCBS.ToString("F2")),
                new XElement(nfe + "vCBS", grupo.vCBS.ToString("F2"))
            );

            // Campos condicionais
            if (grupo.pDif.HasValue)
                grupoCBS.Add(new XElement(nfe + "pDif", grupo.pDif.Value.ToString("F2")));

            if (grupo.vDevTrib.HasValue)
                grupoCBS.Add(new XElement(nfe + "vDevTrib", grupo.vDevTrib.Value.ToString("F2")));

            if (grupo.gIBSCBSMono)
                grupoCBS.Add(new XElement(nfe + "gIBSCBSMono", "1"));

            if (grupo.gCBSCredPres)
                grupoCBS.Add(new XElement(nfe + "gCBSCredPres", "1"));

            if (!string.IsNullOrEmpty(grupo.ClassificacaoTributaria))
                grupoCBS.Add(new XElement(nfe + "classTrib", grupo.ClassificacaoTributaria));

            if (!string.IsNullOrEmpty(grupo.CodigoBeneficio))
                grupoCBS.Add(new XElement(nfe + "cBenef", grupo.CodigoBeneficio));

            if (!string.IsNullOrEmpty(grupo.TipoOperacao))
                grupoCBS.Add(new XElement(nfe + "tpOp", grupo.TipoOperacao));

            return grupoCBS;
        }

        private XElement GenerateGrupoIS(GrupoIS grupo)
        {
            var grupoIS = new XElement(nfe + "IS",
                new XElement(nfe + "UF", grupo.UF),
                new XElement(nfe + "cMun", grupo.CodigoMunicipio),
                new XElement(nfe + "vBCIS", grupo.vBCIS.ToString("F2")),
                new XElement(nfe + "pIS", grupo.pIS.ToString("F2")),
                new XElement(nfe + "vIS", grupo.vIS.ToString("F2"))
            );

            // Campos condicionais
            if (grupo.pDif.HasValue)
                grupoIS.Add(new XElement(nfe + "pDif", grupo.pDif.Value.ToString("F2")));

            if (grupo.vDevTrib.HasValue)
                grupoIS.Add(new XElement(nfe + "vDevTrib", grupo.vDevTrib.Value.ToString("F2")));

            if (grupo.gIBSCBSMono)
                grupoIS.Add(new XElement(nfe + "gIBSCBSMono", "1"));

            if (grupo.gCBSCredPres)
                grupoIS.Add(new XElement(nfe + "gCBSCredPres", "1"));

            if (!string.IsNullOrEmpty(grupo.ClassificacaoTributaria))
                grupoIS.Add(new XElement(nfe + "classTrib", grupo.ClassificacaoTributaria));

            if (!string.IsNullOrEmpty(grupo.CodigoBeneficio))
                grupoIS.Add(new XElement(nfe + "cBenef", grupo.CodigoBeneficio));

            if (!string.IsNullOrEmpty(grupo.TipoOperacao))
                grupoIS.Add(new XElement(nfe + "tpOp", grupo.TipoOperacao));

            return grupoIS;
        }

        private XElement GenerateGrupoVC(IEnumerable<ReferenciaDocumento> referencias)
        {
            var grupoVC = new XElement(nfe + "VC");

            foreach (var referencia in referencias)
            {
                var refElement = new XElement(nfe + "ref",
                    new XElement(nfe + "chNFe", referencia.ChaveAcessoReferenciada),
                    new XElement(nfe + "nItem", referencia.nItemReferenciado),
                    new XElement(nfe + "tpDoc", referencia.TipoDocumento),
                    new XElement(nfe + "dhEmi", referencia.DataEmissaoReferenciada.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    new XElement(nfe + "UF", referencia.UFReferenciada),
                    new XElement(nfe + "vDoc", referencia.ValorReferenciado.ToString("F2")),
                    new XElement(nfe + "motRef", referencia.MotivoReferencia)
                );

                grupoVC.Add(refElement);
            }

            return grupoVC;
        }

        private XElement GenerateRastreabilidade(IEnumerable<RastreabilidadeItem> rastreabilidade)
        {
            var grupoRast = new XElement(nfe + "rast");

            foreach (var item in rastreabilidade)
            {
                var rastElement = new XElement(nfe + "item",
                    new XElement(nfe + "GTIN", item.GTIN),
                    new XElement(nfe + "tpRast", item.TipoRastreabilidade)
                );

                if (!string.IsNullOrEmpty(item.NumeroLote))
                    rastElement.Add(new XElement(nfe + "nLote", item.NumeroLote));

                if (item.DataFabricacao.HasValue)
                    rastElement.Add(new XElement(nfe + "dFab", item.DataFabricacao.Value.ToString("yyyy-MM-dd")));

                if (item.DataValidade.HasValue)
                    rastElement.Add(new XElement(nfe + "dVal", item.DataValidade.Value.ToString("yyyy-MM-dd")));

                if (!string.IsNullOrEmpty(item.CodigoRastreamento))
                    rastElement.Add(new XElement(nfe + "cAgreg", item.CodigoRastreamento));

                if (!string.IsNullOrEmpty(item.InformacoesAdicionais))
                    rastElement.Add(new XElement(nfe + "infAdic", item.InformacoesAdicionais));

                grupoRast.Add(rastElement);
            }

            return grupoRast;
        }

        private XElement GenerateTotal2026(NFe2026 nfe2026)
        {
            var total = new XElement(nfe + "total");

            // Totais existentes (ICMS, IPI, etc.)
            total.Add(GenerateICMSTotal(nfe2026.Total.ICMSTot));
            total.Add(GenerateIPITotal(nfe2026.Total.ICMSTot));

            // Novos totais 2026 - Grupo VB
            total.Add(GenerateTotalIBS(nfe2026.TotaisIBS));
            total.Add(GenerateTotalCBS(nfe2026.TotaisCBS));
            total.Add(GenerateTotalIS(nfe2026.TotaisIS));

            // Totais por UF
            if (nfe2026.TotaisPorUF.Any())
            {
                total.Add(GenerateTotaisPorUF(nfe2026.TotaisPorUF));
            }

            // Totais por município
            if (nfe2026.TotaisPorMunicipio.Any())
            {
                total.Add(GenerateTotaisPorMunicipio(nfe2026.TotaisPorMunicipio));
            }

            return total;
        }

        private XElement GenerateTotalIBS(TotalIBS totalIBS)
        {
            return new XElement(nfe + "IBS",
                new XElement(nfe + "vBCIBS", totalIBS.vBCIBS.ToString("F2")),
                new XElement(nfe + "vIBS", totalIBS.vIBS.ToString("F2")),
                new XElement(nfe + "vIBSST", totalIBS.vIBSST.ToString("F2")),
                new XElement(nfe + "vFCPIBS", totalIBS.vFCPIBS.ToString("F2")),
                new XElement(nfe + "vIBSDevolvido", totalIBS.vIBSDevolvido.ToString("F2")),
                new XElement(nfe + "vIBSRetido", totalIBS.vIBSRetido.ToString("F2"))
            );
        }

        private XElement GenerateTotalCBS(TotalCBS totalCBS)
        {
            return new XElement(nfe + "CBS",
                new XElement(nfe + "vBCCBS", totalCBS.vBCCBS.ToString("F2")),
                new XElement(nfe + "vCBS", totalCBS.vCBS.ToString("F2")),
                new XElement(nfe + "vCBSST", totalCBS.vCBSST.ToString("F2")),
                new XElement(nfe + "vFCPCBS", totalCBS.vFCPCBS.ToString("F2")),
                new XElement(nfe + "vCBSDevolvido", totalCBS.vCBSDevolvido.ToString("F2")),
                new XElement(nfe + "vCBSRetido", totalCBS.vCBSRetido.ToString("F2"))
            );
        }

        private XElement GenerateTotalIS(TotalIS totalIS)
        {
            return new XElement(nfe + "IS",
                new XElement(nfe + "vBCIS", totalIS.vBCIS.ToString("F2")),
                new XElement(nfe + "vIS", totalIS.vIS.ToString("F2")),
                new XElement(nfe + "vISST", totalIS.vISST.ToString("F2")),
                new XElement(nfe + "vFCPIS", totalIS.vFCPIS.ToString("F2")),
                new XElement(nfe + "vISDevolvido", totalIS.vISDevolvido.ToString("F2")),
                new XElement(nfe + "vISRetido", totalIS.vISRetido.ToString("F2"))
            );
        }

        private XElement GenerateTotaisPorUF(IEnumerable<TotalPorUF> totaisPorUF)
        {
            var grupoVB = new XElement(nfe + "VB");

            foreach (var totalUF in totaisPorUF)
            {
                var totalUFElement = new XElement(nfe + "UF",
                    new XElement(nfe + "UF", totalUF.UF),
                    new XElement(nfe + "vBCIBS", totalUF.vBCIBS.ToString("F2")),
                    new XElement(nfe + "vIBS", totalUF.vIBS.ToString("F2")),
                    new XElement(nfe + "vBCCBS", totalUF.vBCCBS.ToString("F2")),
                    new XElement(nfe + "vCBS", totalUF.vCBS.ToString("F2")),
                    new XElement(nfe + "vBCIS", totalUF.vBCIS.ToString("F2")),
                    new XElement(nfe + "vIS", totalUF.vIS.ToString("F2")),
                    new XElement(nfe + "qItem", totalUF.QuantidadeItens)
                );

                grupoVB.Add(totalUFElement);
            }

            return grupoVB;
        }

        private XElement GenerateTotaisPorMunicipio(IEnumerable<TotalPorMunicipio> totaisPorMunicipio)
        {
            var grupoVB = new XElement(nfe + "VB");

            foreach (var totalMunicipio in totaisPorMunicipio)
            {
                var totalMunicipioElement = new XElement(nfe + "municipio",
                    new XElement(nfe + "UF", totalMunicipio.UF),
                    new XElement(nfe + "cMun", totalMunicipio.CodigoMunicipio),
                    new XElement(nfe + "xMun", totalMunicipio.NomeMunicipio),
                    new XElement(nfe + "vBCIBS", totalMunicipio.vBCIBS.ToString("F2")),
                    new XElement(nfe + "vIBS", totalMunicipio.vIBS.ToString("F2")),
                    new XElement(nfe + "vBCCBS", totalMunicipio.vBCCBS.ToString("F2")),
                    new XElement(nfe + "vCBS", totalMunicipio.vCBS.ToString("F2")),
                    new XElement(nfe + "vBCIS", totalMunicipio.vBCIS.ToString("F2")),
                    new XElement(nfe + "vIS", totalMunicipio.vIS.ToString("F2")),
                    new XElement(nfe + "qItem", totalMunicipio.QuantidadeItens)
                );

                grupoVB.Add(totalMunicipioElement);
            }

            return grupoVB;
        }

        private XElement GenerateICMSTotal(ICMSTotal icmsTotal)
        {
            return new XElement(nfe + "ICMSTot",
                new XElement(nfe + "vBC", icmsTotal.vBC.ToString("F2")),
                new XElement(nfe + "vICMS", icmsTotal.vICMS.ToString("F2")),
                new XElement(nfe + "vICMSDeson", icmsTotal.vICMSDeson.ToString("F2")),
                new XElement(nfe + "vFCP", icmsTotal.vFCP.ToString("F2")),
                new XElement(nfe + "vBCST", icmsTotal.vBCST.ToString("F2")),
                new XElement(nfe + "vST", icmsTotal.vST.ToString("F2")),
                new XElement(nfe + "vFCPST", icmsTotal.vFCPST.ToString("F2")),
                new XElement(nfe + "vFCPSTRet", icmsTotal.vFCPSTRet.ToString("F2")),
                new XElement(nfe + "vProd", icmsTotal.vProd.ToString("F2")),
                new XElement(nfe + "vFrete", icmsTotal.vFrete.ToString("F2")),
                new XElement(nfe + "vSeg", icmsTotal.vSeg.ToString("F2")),
                new XElement(nfe + "vDesc", icmsTotal.vDesc.ToString("F2")),
                new XElement(nfe + "vII", icmsTotal.vII.ToString("F2")),
                new XElement(nfe + "vIPI", icmsTotal.vIPI.ToString("F2")),
                new XElement(nfe + "vIPIDevol", icmsTotal.vIPIDevol.ToString("F2")),
                new XElement(nfe + "vPIS", icmsTotal.vPIS.ToString("F2")),
                new XElement(nfe + "vCOFINS", icmsTotal.vCOFINS.ToString("F2")),
                new XElement(nfe + "vOutro", icmsTotal.vOutro.ToString("F2")),
                new XElement(nfe + "vNF", icmsTotal.vNF.ToString("F2"))
            );
        }

        private XElement GenerateIPITotal(ICMSTotal icmsTotal)
        {
            return new XElement(nfe + "IPITot",
                new XElement(nfe + "vBC", icmsTotal.vBC.ToString("F2")),
                new XElement(nfe + "vIPI", icmsTotal.vIPI.ToString("F2"))
            );
        }
    }
}
