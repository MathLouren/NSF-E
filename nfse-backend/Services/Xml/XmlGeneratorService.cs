using System;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Globalization;
using nfse_backend.Models.NFe;
using nfse_backend.Models;
using System.Collections.Generic;

namespace nfse_backend.Services.Xml
{
    public class XmlGeneratorService
    {
        private readonly XNamespace nfe = "http://www.portalfiscal.inf.br/nfe";
        
        public string GenerateNFeXml(nfse_backend.Models.NFe.NFe nfe)
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(this.nfe + "NFe",
                    new XAttribute("xmlns", this.nfe),
                    GenerateInfNFe(nfe)
                )
            );

            return xml.ToString();
        }

        private XElement GenerateInfNFe(nfse_backend.Models.NFe.NFe nfe)
        {
            var infNFe = new XElement(this.nfe + "infNFe",
                new XAttribute("versao", nfe.Versao),
                new XAttribute("Id", GenerateNFeId(nfe))
            );

            // ide - Identificação da NF-e
            infNFe.Add(GenerateIde(nfe.Ide));
            
            // emit - Emitente
            infNFe.Add(GenerateEmit(nfe.Emit));
            
            // dest - Destinatário
            infNFe.Add(GenerateDest(nfe.Dest));
            
            // det - Detalhamento de Produtos e Serviços
            foreach (var det in nfe.Det)
            {
                infNFe.Add(GenerateDet(det));
            }
            
            // total - Totais da NF-e
            infNFe.Add(GenerateTotal(nfe.Total));
            
            // transp - Transporte
            infNFe.Add(GenerateTransp(nfe.Transp));
            
            // cobr - Cobrança (opcional)
            if (nfe.Cobr != null && nfe.Cobr.Fat != null)
            {
                infNFe.Add(GenerateCobr(nfe.Cobr));
            }
            
            // pag - Pagamento
            infNFe.Add(GeneratePag(nfe.Pag));
            
            // infAdic - Informações Adicionais (opcional)
            if (nfe.InfAdic != null && (!string.IsNullOrEmpty(nfe.InfAdic.infCpl) || !string.IsNullOrEmpty(nfe.InfAdic.infAdFisco)))
            {
                infNFe.Add(GenerateInfAdic(nfe.InfAdic));
            }

            return infNFe;
        }

        protected string GenerateNFeId(nfse_backend.Models.NFe.NFe nfe)
        {
            // Formato: NFe + cUF + AAMM + CNPJ + mod + serie + nNF + tpEmis + cNF + cDV
            var chaveAcesso = $"{nfe.Ide.cUF:00}{nfe.Ide.dhEmi:yyMM}{nfe.Emit.CNPJ}{nfe.Ide.mod:00}{nfe.Ide.serie:000}{nfe.Ide.nNF:000000000}{nfe.Ide.tpEmis}{nfe.Ide.cNF}";
            var dv = CalcularDigitoVerificador(chaveAcesso);
            nfe.Ide.cDV = dv;
            nfe.ChaveAcesso = chaveAcesso + dv;
            return $"NFe{nfe.ChaveAcesso}";
        }

        private int CalcularDigitoVerificador(string chave)
        {
            int soma = 0;
            int peso = 2;
            
            for (int i = chave.Length - 1; i >= 0; i--)
            {
                soma += int.Parse(chave[i].ToString()) * peso;
                peso++;
                if (peso > 9) peso = 2;
            }
            
            int resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        protected XElement GenerateIde(IdentificacaoNFe ide)
        {
            var ideElement = new XElement(nfe + "ide",
                new XElement(nfe + "cUF", ide.cUF),
                new XElement(nfe + "cNF", ide.cNF),
                new XElement(nfe + "natOp", ide.natOp),
                new XElement(nfe + "mod", ide.mod),
                new XElement(nfe + "serie", ide.serie),
                new XElement(nfe + "nNF", ide.nNF),
                new XElement(nfe + "dhEmi", ide.dhEmi?.ToString("yyyy-MM-ddTHH:mm:sszzz"))
            );

            if (ide.dhSaiEnt.HasValue)
                ideElement.Add(new XElement(nfe + "dhSaiEnt", ide.dhSaiEnt.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")));

            ideElement.Add(
                new XElement(nfe + "tpNF", ide.tpNF),
                new XElement(nfe + "idDest", ide.idDest),
                new XElement(nfe + "cMunFG", ide.cMunFG),
                new XElement(nfe + "tpImp", ide.tpImp),
                new XElement(nfe + "tpEmis", ide.tpEmis),
                new XElement(nfe + "cDV", ide.cDV),
                new XElement(nfe + "tpAmb", ide.tpAmb),
                new XElement(nfe + "finNFe", ide.finNFe),
                new XElement(nfe + "indFinal", ide.indFinal),
                new XElement(nfe + "indPres", ide.indPres),
                new XElement(nfe + "procEmi", ide.procEmi),
                new XElement(nfe + "verProc", ide.verProc)
            );

            return ideElement;
        }

        protected XElement GenerateEmit(Emitente emit)
        {
            var emitElement = new XElement(nfe + "emit");

            if (!string.IsNullOrEmpty(emit.CNPJ))
                emitElement.Add(new XElement(nfe + "CNPJ", emit.CNPJ));
            else if (!string.IsNullOrEmpty(emit.CPF))
                emitElement.Add(new XElement(nfe + "CPF", emit.CPF));

            emitElement.Add(new XElement(nfe + "xNome", emit.xNome));
            
            if (!string.IsNullOrEmpty(emit.xFant))
                emitElement.Add(new XElement(nfe + "xFant", emit.xFant));

            emitElement.Add(GenerateEndereco(emit.EnderEmit, "enderEmit"));
            emitElement.Add(new XElement(nfe + "IE", emit.IE));

            if (!string.IsNullOrEmpty(emit.IEST))
                emitElement.Add(new XElement(nfe + "IEST", emit.IEST));
            
            if (!string.IsNullOrEmpty(emit.IM))
                emitElement.Add(new XElement(nfe + "IM", emit.IM));
            
            if (!string.IsNullOrEmpty(emit.CNAE))
                emitElement.Add(new XElement(nfe + "CNAE", emit.CNAE));

            emitElement.Add(new XElement(nfe + "CRT", emit.CRT));

            return emitElement;
        }

        protected XElement GenerateDest(Destinatario dest)
        {
            var destElement = new XElement(nfe + "dest");

            if (!string.IsNullOrEmpty(dest.CNPJ))
                destElement.Add(new XElement(nfe + "CNPJ", dest.CNPJ));
            else if (!string.IsNullOrEmpty(dest.CPF))
                destElement.Add(new XElement(nfe + "CPF", dest.CPF));
            else if (!string.IsNullOrEmpty(dest.idEstrangeiro))
                destElement.Add(new XElement(nfe + "idEstrangeiro", dest.idEstrangeiro));

            destElement.Add(new XElement(nfe + "xNome", dest.xNome));
            destElement.Add(GenerateEndereco(dest.EnderDest, "enderDest"));
            destElement.Add(new XElement(nfe + "indIEDest", dest.indIEDest));

            if (!string.IsNullOrEmpty(dest.IE))
                destElement.Add(new XElement(nfe + "IE", dest.IE));
            
            if (!string.IsNullOrEmpty(dest.ISUF))
                destElement.Add(new XElement(nfe + "ISUF", dest.ISUF));
            
            if (!string.IsNullOrEmpty(dest.IM))
                destElement.Add(new XElement(nfe + "IM", dest.IM));
            
            if (!string.IsNullOrEmpty(dest.email))
                destElement.Add(new XElement(nfe + "email", dest.email));

            return destElement;
        }

        private XElement GenerateEndereco(EnderecoNFe endereco, string tagName)
        {
            var enderecoElement = new XElement(nfe + tagName,
                new XElement(nfe + "xLgr", endereco.xLgr),
                new XElement(nfe + "nro", endereco.nro)
            );

            if (!string.IsNullOrEmpty(endereco.xCpl))
                enderecoElement.Add(new XElement(nfe + "xCpl", endereco.xCpl));

            enderecoElement.Add(
                new XElement(nfe + "xBairro", endereco.xBairro),
                new XElement(nfe + "cMun", endereco.cMun),
                new XElement(nfe + "xMun", endereco.xMun),
                new XElement(nfe + "UF", endereco.UF)
            );

            if (!string.IsNullOrEmpty(endereco.CEP))
                enderecoElement.Add(new XElement(nfe + "CEP", endereco.CEP));

            if (endereco.cPais.HasValue)
                enderecoElement.Add(new XElement(nfe + "cPais", endereco.cPais.Value));

            if (!string.IsNullOrEmpty(endereco.xPais))
                enderecoElement.Add(new XElement(nfe + "xPais", endereco.xPais));

            if (!string.IsNullOrEmpty(endereco.fone))
                enderecoElement.Add(new XElement(nfe + "fone", endereco.fone));

            return enderecoElement;
        }

        protected XElement GenerateDet(DetalheNFe det)
        {
            var detElement = new XElement(nfe + "det",
                new XAttribute("nItem", det.nItem),
                GenerateProd(det.Prod),
                GenerateImposto(det.Imposto)
            );

            if (!string.IsNullOrEmpty(det.infAdProd))
                detElement.Add(new XElement(nfe + "infAdProd", det.infAdProd));

            return detElement;
        }

        protected XElement GenerateProd(ProdutoNFe prod)
        {
            var prodElement = new XElement(nfe + "prod",
                new XElement(nfe + "cProd", prod.cProd),
                new XElement(nfe + "cEAN", string.IsNullOrEmpty(prod.cEAN) ? "SEM GTIN" : prod.cEAN),
                new XElement(nfe + "xProd", prod.xProd),
                new XElement(nfe + "NCM", prod.NCM)
            );

            if (!string.IsNullOrEmpty(prod.CEST))
                prodElement.Add(new XElement(nfe + "CEST", prod.CEST));

            prodElement.Add(
                new XElement(nfe + "CFOP", prod.CFOP),
                new XElement(nfe + "uCom", prod.uCom),
                new XElement(nfe + "qCom", prod.qCom.ToString("F4", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vUnCom", prod.vUnCom.ToString("F10", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vProd", prod.vProd.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "cEANTrib", string.IsNullOrEmpty(prod.cEANTrib) ? "SEM GTIN" : prod.cEANTrib),
                new XElement(nfe + "uTrib", prod.uTrib),
                new XElement(nfe + "qTrib", prod.qTrib.ToString("F4", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vUnTrib", prod.vUnTrib.ToString("F10", CultureInfo.InvariantCulture))
            );

            if (prod.vFrete.HasValue && prod.vFrete.Value > 0)
                prodElement.Add(new XElement(nfe + "vFrete", prod.vFrete.Value.ToString("F2", CultureInfo.InvariantCulture)));

            if (prod.vSeg.HasValue && prod.vSeg.Value > 0)
                prodElement.Add(new XElement(nfe + "vSeg", prod.vSeg.Value.ToString("F2", CultureInfo.InvariantCulture)));

            if (prod.vDesc.HasValue && prod.vDesc.Value > 0)
                prodElement.Add(new XElement(nfe + "vDesc", prod.vDesc.Value.ToString("F2", CultureInfo.InvariantCulture)));

            if (prod.vOutro.HasValue && prod.vOutro.Value > 0)
                prodElement.Add(new XElement(nfe + "vOutro", prod.vOutro.Value.ToString("F2", CultureInfo.InvariantCulture)));

            prodElement.Add(new XElement(nfe + "indTot", prod.indTot));

            return prodElement;
        }

        protected XElement GenerateImposto(ImpostoNFe imposto)
        {
            var impostoElement = new XElement(nfe + "imposto");

            if (imposto.vTotTrib > 0)
                impostoElement.Add(new XElement(nfe + "vTotTrib", imposto.vTotTrib.ToString("F2", CultureInfo.InvariantCulture)));

            impostoElement.Add(GenerateICMS(imposto.ICMS));

            if (imposto.IPI != null && !string.IsNullOrEmpty(imposto.IPI.CST))
                impostoElement.Add(GenerateIPI(imposto.IPI));

            impostoElement.Add(GeneratePIS(imposto.PIS));
            impostoElement.Add(GenerateCOFINS(imposto.COFINS));

            return impostoElement;
        }

        private XElement GenerateICMS(ICMS icms)
        {
            var icmsElement = new XElement(nfe + "ICMS");
            var icmsDetail = new XElement(nfe + $"ICMS{icms.CST}");

            icmsDetail.Add(
                new XElement(nfe + "orig", icms.orig),
                new XElement(nfe + "CST", icms.CST)
            );

            if (icms.CST == "00" || icms.CST == "10" || icms.CST == "20" || icms.CST == "51" || icms.CST == "70" || icms.CST == "90")
            {
                icmsDetail.Add(
                    new XElement(nfe + "modBC", icms.modBC),
                    new XElement(nfe + "vBC", icms.vBC.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "pICMS", icms.pICMS.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "vICMS", icms.vICMS.ToString("F2", CultureInfo.InvariantCulture))
                );
            }

            if ((icms.CST == "10" || icms.CST == "30" || icms.CST == "70" || icms.CST == "90") && 
                icms.vBCST.HasValue && icms.pICMSST.HasValue && icms.vICMSST.HasValue)
            {
                icmsDetail.Add(
                    new XElement(nfe + "vBCST", icms.vBCST.Value.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "pICMSST", icms.pICMSST.Value.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "vICMSST", icms.vICMSST.Value.ToString("F2", CultureInfo.InvariantCulture))
                );
            }

            icmsElement.Add(icmsDetail);
            return icmsElement;
        }

        private XElement GenerateIPI(IPI ipi)
        {
            var ipiElement = new XElement(nfe + "IPI");

            if (!string.IsNullOrEmpty(ipi.cEnq))
                ipiElement.Add(new XElement(nfe + "cEnq", ipi.cEnq));

            var ipiTrib = new XElement(nfe + "IPITrib",
                new XElement(nfe + "CST", ipi.CST)
            );

            if (ipi.vBC.HasValue && ipi.pIPI.HasValue && ipi.vIPI.HasValue)
            {
                ipiTrib.Add(
                    new XElement(nfe + "vBC", ipi.vBC.Value.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "pIPI", ipi.pIPI.Value.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "vIPI", ipi.vIPI.Value.ToString("F2", CultureInfo.InvariantCulture))
                );
            }

            ipiElement.Add(ipiTrib);
            return ipiElement;
        }

        private XElement GeneratePIS(PIS pis)
        {
            var pisElement = new XElement(nfe + "PIS");
            var pisAliq = new XElement(nfe + "PISAliq",
                new XElement(nfe + "CST", pis.CST)
            );

            if (pis.vBC.HasValue && pis.pPIS.HasValue && pis.vPIS.HasValue)
            {
                pisAliq.Add(
                    new XElement(nfe + "vBC", pis.vBC.Value.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "pPIS", pis.pPIS.Value.ToString("F4", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "vPIS", pis.vPIS.Value.ToString("F2", CultureInfo.InvariantCulture))
                );
            }

            pisElement.Add(pisAliq);
            return pisElement;
        }

        private XElement GenerateCOFINS(COFINS cofins)
        {
            var cofinsElement = new XElement(nfe + "COFINS");
            var cofinsAliq = new XElement(nfe + "COFINSAliq",
                new XElement(nfe + "CST", cofins.CST)
            );

            if (cofins.vBC.HasValue && cofins.pCOFINS.HasValue && cofins.vCOFINS.HasValue)
            {
                cofinsAliq.Add(
                    new XElement(nfe + "vBC", cofins.vBC.Value.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "pCOFINS", cofins.pCOFINS.Value.ToString("F4", CultureInfo.InvariantCulture)),
                    new XElement(nfe + "vCOFINS", cofins.vCOFINS.Value.ToString("F2", CultureInfo.InvariantCulture))
                );
            }

            cofinsElement.Add(cofinsAliq);
            return cofinsElement;
        }

        protected XElement GenerateTotal(TotalNFe total)
        {
            var totalElement = new XElement(nfe + "total");
            var icmsTot = new XElement(nfe + "ICMSTot",
                new XElement(nfe + "vBC", total.ICMSTot.vBC.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vICMS", total.ICMSTot.vICMS.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vICMSDeson", total.ICMSTot.vICMSDeson.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vFCP", total.ICMSTot.vFCP.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vBCST", total.ICMSTot.vBCST.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vST", total.ICMSTot.vST.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vFCPST", total.ICMSTot.vFCPST.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vFCPSTRet", total.ICMSTot.vFCPSTRet.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vProd", total.ICMSTot.vProd.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vFrete", total.ICMSTot.vFrete.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vSeg", total.ICMSTot.vSeg.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vDesc", total.ICMSTot.vDesc.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vII", total.ICMSTot.vII.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vIPI", total.ICMSTot.vIPI.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vIPIDevol", total.ICMSTot.vIPIDevol.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vPIS", total.ICMSTot.vPIS.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vCOFINS", total.ICMSTot.vCOFINS.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vOutro", total.ICMSTot.vOutro.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(nfe + "vNF", total.ICMSTot.vNF.ToString("F2", CultureInfo.InvariantCulture))
            );

            if (total.ICMSTot.vTotTrib > 0)
                icmsTot.Add(new XElement(nfe + "vTotTrib", total.ICMSTot.vTotTrib.ToString("F2", CultureInfo.InvariantCulture)));

            totalElement.Add(icmsTot);

            return totalElement;
        }

        protected XElement GenerateTransp(Transporte transp)
        {
            var transpElement = new XElement(nfe + "transp",
                new XElement(nfe + "modFrete", transp.modFrete)
            );

            if (transp.Transporta != null && (!string.IsNullOrEmpty(transp.Transporta.CNPJ) || !string.IsNullOrEmpty(transp.Transporta.CPF)))
            {
                transpElement.Add(GenerateTransporta(transp.Transporta));
            }

            if (transp.Vol != null && transp.Vol.Count > 0)
            {
                foreach (var vol in transp.Vol)
                {
                    transpElement.Add(GenerateVolume(vol));
                }
            }

            return transpElement;
        }

        private XElement GenerateTransporta(Transportador transporta)
        {
            var transportaElement = new XElement(nfe + "transporta");

            if (!string.IsNullOrEmpty(transporta.CNPJ))
                transportaElement.Add(new XElement(nfe + "CNPJ", transporta.CNPJ));
            else if (!string.IsNullOrEmpty(transporta.CPF))
                transportaElement.Add(new XElement(nfe + "CPF", transporta.CPF));

            if (!string.IsNullOrEmpty(transporta.xNome))
                transportaElement.Add(new XElement(nfe + "xNome", transporta.xNome));

            if (!string.IsNullOrEmpty(transporta.IE))
                transportaElement.Add(new XElement(nfe + "IE", transporta.IE));

            if (!string.IsNullOrEmpty(transporta.xEnder))
                transportaElement.Add(new XElement(nfe + "xEnder", transporta.xEnder));

            if (!string.IsNullOrEmpty(transporta.xMun))
                transportaElement.Add(new XElement(nfe + "xMun", transporta.xMun));

            if (!string.IsNullOrEmpty(transporta.UF))
                transportaElement.Add(new XElement(nfe + "UF", transporta.UF));

            return transportaElement;
        }

        private XElement GenerateVolume(Volume vol)
        {
            var volElement = new XElement(nfe + "vol");

            if (vol.qVol.HasValue)
                volElement.Add(new XElement(nfe + "qVol", vol.qVol.Value));

            if (!string.IsNullOrEmpty(vol.esp))
                volElement.Add(new XElement(nfe + "esp", vol.esp));

            if (!string.IsNullOrEmpty(vol.marca))
                volElement.Add(new XElement(nfe + "marca", vol.marca));

            if (!string.IsNullOrEmpty(vol.nVol))
                volElement.Add(new XElement(nfe + "nVol", vol.nVol));

            if (vol.pesoL.HasValue)
                volElement.Add(new XElement(nfe + "pesoL", vol.pesoL.Value.ToString("F3", CultureInfo.InvariantCulture)));

            if (vol.pesoB.HasValue)
                volElement.Add(new XElement(nfe + "pesoB", vol.pesoB.Value.ToString("F3", CultureInfo.InvariantCulture)));

            return volElement;
        }

        protected XElement GenerateCobr(Cobranca cobr)
        {
            var cobrElement = new XElement(nfe + "cobr");

            if (cobr.Fat != null)
            {
                var fatElement = new XElement(nfe + "fat");

                if (!string.IsNullOrEmpty(cobr.Fat.nFat))
                    fatElement.Add(new XElement(nfe + "nFat", cobr.Fat.nFat));

                if (cobr.Fat.vOrig.HasValue)
                    fatElement.Add(new XElement(nfe + "vOrig", cobr.Fat.vOrig.Value.ToString("F2", CultureInfo.InvariantCulture)));

                if (cobr.Fat.vDesc.HasValue)
                    fatElement.Add(new XElement(nfe + "vDesc", cobr.Fat.vDesc.Value.ToString("F2", CultureInfo.InvariantCulture)));

                if (cobr.Fat.vLiq.HasValue)
                    fatElement.Add(new XElement(nfe + "vLiq", cobr.Fat.vLiq.Value.ToString("F2", CultureInfo.InvariantCulture)));

                cobrElement.Add(fatElement);
            }

            if (cobr.Dup != null)
            {
                foreach (var dup in cobr.Dup)
                {
                    var dupElement = new XElement(nfe + "dup");

                    if (!string.IsNullOrEmpty(dup.nDup))
                        dupElement.Add(new XElement(nfe + "nDup", dup.nDup));

                    if (dup.dVenc.HasValue)
                        dupElement.Add(new XElement(nfe + "dVenc", dup.dVenc.Value.ToString("yyyy-MM-dd")));

                    dupElement.Add(new XElement(nfe + "vDup", dup.vDup.ToString("F2", CultureInfo.InvariantCulture)));

                    cobrElement.Add(dupElement);
                }
            }

            return cobrElement;
        }

        protected XElement GeneratePag(Pagamento pag)
        {
            var pagElement = new XElement(nfe + "pag");

            if (pag.DetPag != null)
            {
                foreach (var detPag in pag.DetPag)
                {
                    var detPagElement = new XElement(nfe + "detPag");

                    if (detPag.indPag.HasValue)
                        detPagElement.Add(new XElement(nfe + "indPag", detPag.indPag.Value));

                    detPagElement.Add(
                        new XElement(nfe + "tPag", detPag.tPag.ToString("00")),
                        new XElement(nfe + "vPag", detPag.vPag.ToString("F2", CultureInfo.InvariantCulture))
                    );

                    if (detPag.Card != null && !string.IsNullOrEmpty(detPag.Card.CNPJ))
                    {
                        var cardElement = new XElement(nfe + "card");

                        if (detPag.Card.tpIntegra.HasValue)
                            cardElement.Add(new XElement(nfe + "tpIntegra", detPag.Card.tpIntegra.Value));

                        cardElement.Add(new XElement(nfe + "CNPJ", detPag.Card.CNPJ));

                        if (detPag.Card.tBand.HasValue)
                            cardElement.Add(new XElement(nfe + "tBand", detPag.Card.tBand.Value.ToString("00")));

                        if (!string.IsNullOrEmpty(detPag.Card.cAut))
                            cardElement.Add(new XElement(nfe + "cAut", detPag.Card.cAut));

                        detPagElement.Add(cardElement);
                    }

                    pagElement.Add(detPagElement);
                }
            }

            if (pag.vTroco.HasValue && pag.vTroco.Value > 0)
                pagElement.Add(new XElement(nfe + "vTroco", pag.vTroco.Value.ToString("F2", CultureInfo.InvariantCulture)));

            return pagElement;
        }

        protected XElement GenerateInfAdic(InformacaoAdicional infAdic)
        {
            var infAdicElement = new XElement(nfe + "infAdic");

            if (!string.IsNullOrEmpty(infAdic.infAdFisco))
                infAdicElement.Add(new XElement(nfe + "infAdFisco", infAdic.infAdFisco));

            if (!string.IsNullOrEmpty(infAdic.infCpl))
                infAdicElement.Add(new XElement(nfe + "infCpl", infAdic.infCpl));

            return infAdicElement;
        }

        public string GenerateNFSeXml(Nfse nfse)
        {
            // Implementação para NFS-e seguindo padrão ABRASF
            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("ConsultarNfseEnvio",
                    new XAttribute("xmlns", "http://www.abrasf.org.br/nfse.xsd"),
                    GenerateNFSeContent(nfse)
                )
            );

            return xml.ToString();
        }

        private XElement GenerateNFSeContent(Nfse nfse)
        {
            var content = new XElement("Prestador",
                new XElement("Cnpj", nfse.Prestador.Cnpj),
                new XElement("InscricaoMunicipal", nfse.Prestador.InscricaoMunicipal)
            );

            // Adicionar demais elementos conforme padrão ABRASF/Municipal
            // Esta é uma implementação básica que deve ser adaptada para cada município

            return content;
        }
    }
}
