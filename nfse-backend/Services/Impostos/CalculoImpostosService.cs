using System;
using System.Collections.Generic;
using System.Linq;
using nfse_backend.Models.NFe;

namespace nfse_backend.Services.Impostos
{
    public class CalculoImpostosService
    {
        private Dictionary<string, Dictionary<string, decimal>> _aliquotasICMS = new();
        private Dictionary<string, decimal> _aliquotasISS = new();
        private Dictionary<string, object>? _taxRulesRaw;

        public CalculoImpostosService()
        {
            if (!TentarCarregarRegrasDeArquivo())
            {
                InicializarAliquotasICMS();
                InicializarAliquotasISS();
            }
        }

        private bool TentarCarregarRegrasDeArquivo()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "tax_rules", "rules.json");
                if (!File.Exists(path)) return false;
                var json = File.ReadAllText(path);
                _taxRulesRaw = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                // opcional: mapear para estruturas específicas
                return _taxRulesRaw != null;
            }
            catch
            {
                return false;
            }
        }

        private void InicializarAliquotasICMS()
        {
            // Alíquotas interestaduais de ICMS
            _aliquotasICMS = new Dictionary<string, Dictionary<string, decimal>>();

            // SP para outros estados
            _aliquotasICMS["SP"] = new Dictionary<string, decimal>
            {
                ["SP"] = 18.00m, // Interno
                ["RJ"] = 12.00m, // Sul/Sudeste
                ["MG"] = 12.00m,
                ["ES"] = 12.00m,
                ["PR"] = 12.00m,
                ["SC"] = 12.00m,
                ["RS"] = 12.00m,
                ["AC"] = 7.00m,  // Norte/Nordeste/Centro-Oeste
                ["AL"] = 7.00m,
                ["AP"] = 7.00m,
                ["AM"] = 7.00m,
                ["BA"] = 7.00m,
                ["CE"] = 7.00m,
                ["DF"] = 7.00m,
                ["GO"] = 7.00m,
                ["MA"] = 7.00m,
                ["MT"] = 7.00m,
                ["MS"] = 7.00m,
                ["PA"] = 7.00m,
                ["PB"] = 7.00m,
                ["PE"] = 7.00m,
                ["PI"] = 7.00m,
                ["RN"] = 7.00m,
                ["RO"] = 7.00m,
                ["RR"] = 7.00m,
                ["SE"] = 7.00m,
                ["TO"] = 7.00m
            };

            // Adicionar outras UFs conforme necessário
            // Por simplicidade, vamos usar a mesma estrutura para outros estados
            foreach (var uf in new[] { "RJ", "MG", "ES", "PR", "SC", "RS" })
            {
                _aliquotasICMS[uf] = new Dictionary<string, decimal>(_aliquotasICMS["SP"]);
                _aliquotasICMS[uf][uf] = 18.00m; // Alíquota interna padrão
            }
        }

        private void InicializarAliquotasISS()
        {
            // Alíquotas de ISS por código de serviço (simplificado)
            _aliquotasISS = new Dictionary<string, decimal>
            {
                ["01.01"] = 5.00m, // Análise e desenvolvimento de sistemas
                ["01.02"] = 5.00m, // Programação
                ["01.03"] = 5.00m, // Processamento de dados
                ["01.04"] = 5.00m, // Elaboração de programas
                ["01.05"] = 5.00m, // Licenciamento de software
                ["01.06"] = 5.00m, // Assessoria e consultoria em informática
                ["01.07"] = 5.00m, // Suporte técnico
                ["01.08"] = 5.00m, // Planejamento e confecção de páginas
                ["07.01"] = 5.00m, // Engenharia
                ["07.02"] = 5.00m, // Execução de obras
                ["07.03"] = 5.00m, // Elaboração de planos diretores
                ["11.01"] = 3.00m, // Guarda e estacionamento
                ["11.02"] = 3.00m, // Vigilância
                ["14.01"] = 3.00m, // Lubrificação e limpeza
                ["17.01"] = 2.00m, // Assessoria ou consultoria
                ["17.02"] = 2.00m, // Datilografia
                ["DEFAULT"] = 5.00m // Alíquota padrão
            };
        }

        public ICMS CalcularICMS(ProdutoNFe produto, string ufOrigem, string ufDestino, int crt, 
            bool consumidorFinal = true, bool contribuinteICMS = false)
        {
            var icms = new ICMS
            {
                orig = "0", // Origem nacional
                modBC = 0, // Margem Valor Agregado
                vBC = produto.vProd - (produto.vDesc ?? 0)
            };

            // Determinar CST baseado no CRT
            if (crt == 1) // Simples Nacional
            {
                icms.CST = "102"; // CSOSN - Tributação sem crédito
                icms.pICMS = 0;
                icms.vICMS = 0;
                return icms;
            }

            // Regime Normal
            if (ufOrigem == ufDestino)
            {
                // Operação interna
                icms.CST = "00"; // Tributada integralmente
                icms.pICMS = ObterAliquotaICMS(ufOrigem, ufDestino);
            }
            else
            {
                // Operação interestadual
                if (consumidorFinal && !contribuinteICMS)
                {
                    // DIFAL - Diferencial de alíquotas
                    icms.CST = "00";
                    icms.pICMS = ObterAliquotaICMS(ufOrigem, ufDestino);
                    
                    // Calcular DIFAL
                    decimal aliquotaInterna = ObterAliquotaICMS(ufDestino, ufDestino);
                    decimal aliquotaInterestadual = icms.pICMS;
                    decimal difal = aliquotaInterna - aliquotaInterestadual;
                    
                    if (difal > 0)
                    {
                        // FCP (Fundo de Combate à Pobreza) - 2% para RJ
                        icms.vFCP = Math.Round((icms.vBC * 0.02m), 2);
                    }
                }
                else
                {
                    icms.CST = "00";
                    icms.pICMS = ObterAliquotaICMS(ufOrigem, ufDestino);
                }
            }

            // Calcular valor do ICMS
            icms.vICMS = Math.Round((icms.vBC * icms.pICMS) / 100, 2);

            // Verificar substituição tributária baseado no NCM/CEST
            if (!string.IsNullOrEmpty(produto.CEST))
            {
                // Produto sujeito a ST
                icms.CST = "60"; // ICMS cobrado anteriormente por ST
                icms.vBCST = 0;
                icms.pICMSST = 0;
                icms.vICMSST = 0;
            }

            return icms;
        }

        public IPI CalcularIPI(ProdutoNFe produto, string ncm, bool isIndustria = false)
        {
            var ipi = new IPI
            {
                cEnq = "999" // Código de enquadramento padrão
            };

            if (!isIndustria)
            {
                // Não contribuinte do IPI
                ipi.CST = "53"; // Saída não tributada
                return ipi;
            }

            // Obter alíquota baseada no NCM (simplificado)
            decimal aliquotaIPI = ObterAliquotaIPI(ncm);

            if (aliquotaIPI > 0)
            {
                ipi.CST = "50"; // Saída tributada
                ipi.vBC = produto.vProd - (produto.vDesc ?? 0);
                ipi.pIPI = aliquotaIPI;
                ipi.vIPI = Math.Round((ipi.vBC.Value * ipi.pIPI.Value) / 100, 2);
            }
            else
            {
                ipi.CST = "51"; // Saída com alíquota zero
                ipi.vBC = 0;
                ipi.pIPI = 0;
                ipi.vIPI = 0;
            }

            return ipi;
        }

        public PIS CalcularPIS(ProdutoNFe produto, int crt, bool lucroReal = false)
        {
            var pis = new PIS();

            if (crt == 1) // Simples Nacional
            {
                pis.CST = "99"; // Outras operações
                return pis;
            }

            decimal baseCalculo = produto.vProd - (produto.vDesc ?? 0);

            if (lucroReal) // Regime não cumulativo
            {
                pis.CST = "01"; // Operação tributável - base de cálculo normal
                pis.vBC = baseCalculo;
                pis.pPIS = 1.65m; // Alíquota não cumulativa
                pis.vPIS = Math.Round((pis.vBC.Value * pis.pPIS.Value) / 100, 2);
            }
            else // Regime cumulativo
            {
                pis.CST = "01";
                pis.vBC = baseCalculo;
                pis.pPIS = 0.65m; // Alíquota cumulativa
                pis.vPIS = Math.Round((pis.vBC.Value * pis.pPIS.Value) / 100, 2);
            }

            return pis;
        }

        public COFINS CalcularCOFINS(ProdutoNFe produto, int crt, bool lucroReal = false)
        {
            var cofins = new COFINS();

            if (crt == 1) // Simples Nacional
            {
                cofins.CST = "99"; // Outras operações
                return cofins;
            }

            decimal baseCalculo = produto.vProd - (produto.vDesc ?? 0);

            if (lucroReal) // Regime não cumulativo
            {
                cofins.CST = "01"; // Operação tributável - base de cálculo normal
                cofins.vBC = baseCalculo;
                cofins.pCOFINS = 7.60m; // Alíquota não cumulativa
                cofins.vCOFINS = Math.Round((cofins.vBC.Value * cofins.pCOFINS.Value) / 100, 2);
            }
            else // Regime cumulativo
            {
                cofins.CST = "01";
                cofins.vBC = baseCalculo;
                cofins.pCOFINS = 3.00m; // Alíquota cumulativa
                cofins.vCOFINS = Math.Round((cofins.vBC.Value * cofins.pCOFINS.Value) / 100, 2);
            }

            return cofins;
        }

        public decimal CalcularISS(decimal valorServico, string codigoServico, string codigoMunicipio)
        {
            // Obter alíquota baseada no código do serviço
            decimal aliquota = _aliquotasISS.ContainsKey(codigoServico) 
                ? _aliquotasISS[codigoServico] 
                : _aliquotasISS["DEFAULT"];

            // Alguns municípios têm alíquota mínima de 2%
            if (aliquota < 2.00m)
            {
                aliquota = 2.00m;
            }

            return Math.Round((valorServico * aliquota) / 100, 2);
        }

        public void CalcularTotaisNFe(nfse_backend.Models.NFe.NFe nfe)
        {
            // Zerar totais
            var total = nfe.Total.ICMSTot;
            total.vBC = 0;
            total.vICMS = 0;
            total.vBCST = 0;
            total.vST = 0;
            total.vProd = 0;
            total.vFrete = 0;
            total.vSeg = 0;
            total.vDesc = 0;
            total.vII = 0;
            total.vIPI = 0;
            total.vPIS = 0;
            total.vCOFINS = 0;
            total.vOutro = 0;
            total.vTotTrib = 0;

            // Calcular totais por item
            foreach (var det in nfe.Det)
            {
                if (det.Prod.indTot == 1) // Compõe o total
                {
                    total.vProd += det.Prod.vProd;
                    total.vFrete += det.Prod.vFrete ?? 0;
                    total.vSeg += det.Prod.vSeg ?? 0;
                    total.vDesc += det.Prod.vDesc ?? 0;
                    total.vOutro += det.Prod.vOutro ?? 0;

                    if (det.Imposto != null)
                    {
                        // ICMS
                        if (det.Imposto.ICMS != null)
                        {
                            total.vBC += det.Imposto.ICMS.vBC;
                            total.vICMS += det.Imposto.ICMS.vICMS;
                            total.vBCST += det.Imposto.ICMS.vBCST ?? 0;
                            total.vST += det.Imposto.ICMS.vICMSST ?? 0;
                            total.vFCP += det.Imposto.ICMS.vFCP;
                            total.vFCPST += det.Imposto.ICMS.vFCPST ?? 0;
                        }

                        // IPI
                        if (det.Imposto.IPI != null)
                        {
                            total.vIPI += det.Imposto.IPI.vIPI ?? 0;
                        }

                        // PIS
                        if (det.Imposto.PIS != null)
                        {
                            total.vPIS += det.Imposto.PIS.vPIS ?? 0;
                        }

                        // COFINS
                        if (det.Imposto.COFINS != null)
                        {
                            total.vCOFINS += det.Imposto.COFINS.vCOFINS ?? 0;
                        }

                        total.vTotTrib += det.Imposto.vTotTrib;
                    }
                }
            }

            // Calcular valor total da NF-e
            total.vNF = total.vProd - total.vDesc + total.vST + total.vFrete + 
                       total.vSeg + total.vOutro + total.vII + total.vIPI;

            // Se houver serviços (ISSQN)
            if (nfe.Total.ISSQNtot != null && nfe.Total.ISSQNtot.vServ.HasValue)
            {
                total.vNF += nfe.Total.ISSQNtot.vServ.Value;
            }
        }

        private decimal ObterAliquotaICMS(string ufOrigem, string ufDestino)
        {
            if (_aliquotasICMS.ContainsKey(ufOrigem) && _aliquotasICMS[ufOrigem].ContainsKey(ufDestino))
            {
                return _aliquotasICMS[ufOrigem][ufDestino];
            }

            // Alíquota padrão se não encontrar
            return ufOrigem == ufDestino ? 18.00m : 12.00m;
        }

        private decimal ObterAliquotaIPI(string ncm)
        {
            // Tabela simplificada de IPI por NCM
            // Em produção, isso deve vir de uma base de dados completa
            var aliquotasIPI = new Dictionary<string, decimal>
            {
                ["8471"] = 15.00m, // Máquinas automáticas para processamento de dados
                ["8517"] = 15.00m, // Aparelhos telefônicos
                ["8523"] = 15.00m, // Discos, fitas e outros suportes
                ["2203"] = 20.00m, // Cervejas
                ["2402"] = 300.00m, // Cigarros
                ["8703"] = 25.00m, // Automóveis de passageiros
                ["3303"] = 42.00m, // Perfumes
                ["3304"] = 22.00m, // Produtos de beleza
            };

            // Verificar por código completo ou parcial
            foreach (var kvp in aliquotasIPI)
            {
                if (ncm.StartsWith(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return 0; // Produto não tributado
        }

        public decimal CalcularTributoAproximado(decimal valorProduto, string ncm, string ufDestino)
        {
            // Cálculo simplificado da carga tributária aproximada
            // IBPT - Instituto Brasileiro de Planejamento e Tributação
            
            decimal percentualTributos = 0;

            // Federal (aproximado)
            percentualTributos += 15.00m; // IPI + PIS + COFINS + II

            // Estadual (ICMS)
            percentualTributos += 18.00m;

            // Municipal (se aplicável)
            // percentualTributos += 5.00m;

            return Math.Round((valorProduto * percentualTributos) / 100, 2);
        }
    }
}
