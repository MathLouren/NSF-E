using System;
using System.Collections.Generic;
using System.Linq;
using nfse_backend.Models.NFe;
using nfse_backend.Services.Impostos;

namespace nfse_backend.Services.Calculation
{
    /// <summary>
    /// Serviço de cálculo de totais para NF-e 2026
    /// Implementa cálculos de IBS, CBS, IS por UF, município, item e nota
    /// </summary>
    public class NFe2026CalculationService
    {
        private readonly TabelasImpostosService _tabelasImpostos;

        public NFe2026CalculationService(TabelasImpostosService tabelasImpostos)
        {
            _tabelasImpostos = tabelasImpostos;
        }

        #region Cálculo Principal

        public void CalcularTotaisNFe2026(NFe2026 nfe)
        {
            // 1. Calcular totais por item
            CalcularTotaisPorItem(nfe);

            // 2. Calcular totais gerais
            CalcularTotaisGerais(nfe);

            // 3. Calcular totais por UF
            CalcularTotaisPorUF(nfe);

            // 4. Calcular totais por município
            CalcularTotaisPorMunicipio(nfe);

            // 5. Calcular separação de créditos e monofásicos
            CalcularSeparacaoCreditosMonofasicos(nfe);

            // 6. Calcular totais de devolução
            CalcularTotaisDevolucao(nfe);
        }

        #endregion

        #region Cálculos por Item

        private void CalcularTotaisPorItem(NFe2026 nfe)
        {
            foreach (var item in nfe.Det)
            {
                // Calcular IBS por item
                CalcularIBSItem(item, nfe);

                // Calcular CBS por item
                CalcularCBSItem(item, nfe);

                // Calcular IS por item
                CalcularISItem(item, nfe);
            }
        }

        private void CalcularIBSItem(DetalheNFe item, NFe2026 nfe)
        {
            var grupoIBS = nfe.GruposIBS.FirstOrDefault(g => g.nItem == item.nItem);
            if (grupoIBS == null) return;

            // Obter alíquota IBS da tabela
            var aliquotaIBS = ObterAliquotaIBS(item.Prod.NCM, grupoIBS.UF, grupoIBS.CodigoMunicipio);

            // Calcular base de cálculo
            grupoIBS.vBCIBS = CalcularBaseCalculoIBS(item, grupoIBS);

            // Calcular valor IBS
            grupoIBS.pIBS = aliquotaIBS;
            grupoIBS.vIBS = grupoIBS.vBCIBS * (aliquotaIBS / 100);

            // Aplicar percentual diferencial se aplicável
            if (grupoIBS.pDif.HasValue && grupoIBS.pDif > 0)
            {
                grupoIBS.vIBS = grupoIBS.vIBS * (1 - grupoIBS.pDif.Value / 100);
            }

            // Calcular valor devolvido se aplicável
            if (grupoIBS.vDevTrib.HasValue)
            {
                grupoIBS.vIBS -= grupoIBS.vDevTrib.Value;
            }
        }

        private void CalcularCBSItem(DetalheNFe item, NFe2026 nfe)
        {
            var grupoCBS = nfe.GruposCBS.FirstOrDefault(g => g.nItem == item.nItem);
            if (grupoCBS == null) return;

            // Obter alíquota CBS da tabela
            var aliquotaCBS = ObterAliquotaCBS(item.Prod.NCM, grupoCBS.UF, grupoCBS.CodigoMunicipio);

            // Calcular base de cálculo
            grupoCBS.vBCCBS = CalcularBaseCalculoCBS(item, grupoCBS);

            // Calcular valor CBS
            grupoCBS.pCBS = aliquotaCBS;
            grupoCBS.vCBS = grupoCBS.vBCCBS * (aliquotaCBS / 100);

            // Aplicar percentual diferencial se aplicável
            if (grupoCBS.pDif.HasValue && grupoCBS.pDif > 0)
            {
                grupoCBS.vCBS = grupoCBS.vCBS * (1 - grupoCBS.pDif.Value / 100);
            }

            // Calcular valor devolvido se aplicável
            if (grupoCBS.vDevTrib.HasValue)
            {
                grupoCBS.vCBS -= grupoCBS.vDevTrib.Value;
            }
        }

        private void CalcularISItem(DetalheNFe item, NFe2026 nfe)
        {
            var grupoIS = nfe.GruposIS.FirstOrDefault(g => g.nItem == item.nItem);
            if (grupoIS == null) return;

            // Obter alíquota IS da tabela
            var aliquotaIS = ObterAliquotaIS(item.Prod.NCM, grupoIS.UF, grupoIS.CodigoMunicipio);

            // Calcular base de cálculo
            grupoIS.vBCIS = CalcularBaseCalculoIS(item, grupoIS);

            // Calcular valor IS
            grupoIS.pIS = aliquotaIS;
            grupoIS.vIS = grupoIS.vBCIS * (aliquotaIS / 100);

            // Aplicar percentual diferencial se aplicável
            if (grupoIS.pDif.HasValue && grupoIS.pDif > 0)
            {
                grupoIS.vIS = grupoIS.vIS * (1 - grupoIS.pDif.Value / 100);
            }

            // Calcular valor devolvido se aplicável
            if (grupoIS.vDevTrib.HasValue)
            {
                grupoIS.vIS -= grupoIS.vDevTrib.Value;
            }
        }

        #endregion

        #region Cálculos de Base

        private decimal CalcularBaseCalculoIBS(DetalheNFe item, GrupoIBS grupo)
        {
            decimal baseCalculo = item.Prod.vProd;

            // Adicionar frete, seguro e outras despesas
            baseCalculo += item.Prod.vFrete ?? 0;
            baseCalculo += item.Prod.vSeg ?? 0;
            baseCalculo += item.Prod.vOutro ?? 0;

            // Subtrair desconto
            baseCalculo -= item.Prod.vDesc ?? 0;

            // Aplicar redução de base se aplicável
            if (grupo.TipoOperacao == "REDUCAO")
            {
                baseCalculo *= 0.5m; // Exemplo: 50% de redução
            }

            return Math.Max(0, baseCalculo);
        }

        private decimal CalcularBaseCalculoCBS(DetalheNFe item, GrupoCBS grupo)
        {
            decimal baseCalculo = item.Prod.vProd;

            // Adicionar frete, seguro e outras despesas
            baseCalculo += item.Prod.vFrete ?? 0;
            baseCalculo += item.Prod.vSeg ?? 0;
            baseCalculo += item.Prod.vOutro ?? 0;

            // Subtrair desconto
            baseCalculo -= item.Prod.vDesc ?? 0;

            // Aplicar redução de base se aplicável
            if (grupo.TipoOperacao == "REDUCAO")
            {
                baseCalculo *= 0.5m; // Exemplo: 50% de redução
            }

            return Math.Max(0, baseCalculo);
        }

        private decimal CalcularBaseCalculoIS(DetalheNFe item, GrupoIS grupo)
        {
            decimal baseCalculo = item.Prod.vProd;

            // Adicionar frete, seguro e outras despesas
            baseCalculo += item.Prod.vFrete ?? 0;
            baseCalculo += item.Prod.vSeg ?? 0;
            baseCalculo += item.Prod.vOutro ?? 0;

            // Subtrair desconto
            baseCalculo -= item.Prod.vDesc ?? 0;

            // Aplicar redução de base se aplicável
            if (grupo.TipoOperacao == "REDUCAO")
            {
                baseCalculo *= 0.5m; // Exemplo: 50% de redução
            }

            return Math.Max(0, baseCalculo);
        }

        #endregion

        #region Cálculos de Totais Gerais

        private void CalcularTotaisGerais(NFe2026 nfe)
        {
            // Totais IBS
            nfe.TotaisIBS.vBCIBS = nfe.GruposIBS.Sum(g => g.vBCIBS);
            nfe.TotaisIBS.vIBS = nfe.GruposIBS.Sum(g => g.vIBS);
            nfe.TotaisIBS.vIBSST = nfe.GruposIBS.Where(g => g.gIBSCBSMono).Sum(g => g.vIBS);
            nfe.TotaisIBS.vFCPIBS = CalcularFCPIBS(nfe);
            nfe.TotaisIBS.vIBSDevolvido = nfe.GruposIBS.Sum(g => g.vDevTrib ?? 0);
            nfe.TotaisIBS.vIBSRetido = CalcularIBSRetido(nfe);

            // Totais CBS
            nfe.TotaisCBS.vBCCBS = nfe.GruposCBS.Sum(g => g.vBCCBS);
            nfe.TotaisCBS.vCBS = nfe.GruposCBS.Sum(g => g.vCBS);
            nfe.TotaisCBS.vCBSST = nfe.GruposCBS.Where(g => g.gIBSCBSMono).Sum(g => g.vCBS);
            nfe.TotaisCBS.vFCPCBS = CalcularFCPCBS(nfe);
            nfe.TotaisCBS.vCBSDevolvido = nfe.GruposCBS.Sum(g => g.vDevTrib ?? 0);
            nfe.TotaisCBS.vCBSRetido = CalcularCBSRetido(nfe);

            // Totais IS
            nfe.TotaisIS.vBCIS = nfe.GruposIS.Sum(g => g.vBCIS);
            nfe.TotaisIS.vIS = nfe.GruposIS.Sum(g => g.vIS);
            nfe.TotaisIS.vISST = nfe.GruposIS.Where(g => g.gIBSCBSMono).Sum(g => g.vIS);
            nfe.TotaisIS.vFCPIS = CalcularFCPIS(nfe);
            nfe.TotaisIS.vISDevolvido = nfe.GruposIS.Sum(g => g.vDevTrib ?? 0);
            nfe.TotaisIS.vISRetido = CalcularISRetido(nfe);
        }

        #endregion

        #region Cálculos por UF e Município

        private void CalcularTotaisPorUF(NFe2026 nfe)
        {
            nfe.TotaisPorUF.Clear();

            // Agrupar por UF
            var gruposPorUF = nfe.GruposIBS.Concat(nfe.GruposCBS.Cast<object>()).Concat(nfe.GruposIS.Cast<object>())
                .GroupBy(g => ((dynamic)g).UF);

            foreach (var grupoUF in gruposPorUF)
            {
                var totalUF = new TotalPorUF
                {
                    UF = grupoUF.Key,
                    QuantidadeItens = grupoUF.Count()
                };

                // Calcular totais IBS para esta UF
                var gruposIBS = nfe.GruposIBS.Where(g => g.UF == grupoUF.Key);
                totalUF.vBCIBS = gruposIBS.Sum(g => g.vBCIBS);
                totalUF.vIBS = gruposIBS.Sum(g => g.vIBS);

                // Calcular totais CBS para esta UF
                var gruposCBS = nfe.GruposCBS.Where(g => g.UF == grupoUF.Key);
                totalUF.vBCCBS = gruposCBS.Sum(g => g.vBCCBS);
                totalUF.vCBS = gruposCBS.Sum(g => g.vCBS);

                // Calcular totais IS para esta UF
                var gruposIS = nfe.GruposIS.Where(g => g.UF == grupoUF.Key);
                totalUF.vBCIS = gruposIS.Sum(g => g.vBCIS);
                totalUF.vIS = gruposIS.Sum(g => g.vIS);

                nfe.TotaisPorUF.Add(totalUF);
            }
        }

        private void CalcularTotaisPorMunicipio(NFe2026 nfe)
        {
            nfe.TotaisPorMunicipio.Clear();

            // Agrupar por UF e município
            var gruposPorMunicipio = nfe.GruposIBS.Concat(nfe.GruposCBS.Cast<object>()).Concat(nfe.GruposIS.Cast<object>())
                .GroupBy(g => new { UF = ((dynamic)g).UF, CodigoMunicipio = ((dynamic)g).CodigoMunicipio, NomeMunicipio = ((dynamic)g).NomeMunicipio });

            foreach (var grupoMunicipio in gruposPorMunicipio)
            {
                var totalMunicipio = new TotalPorMunicipio
                {
                    UF = grupoMunicipio.Key.UF,
                    CodigoMunicipio = grupoMunicipio.Key.CodigoMunicipio,
                    NomeMunicipio = grupoMunicipio.Key.NomeMunicipio,
                    QuantidadeItens = grupoMunicipio.Count()
                };

                // Calcular totais IBS para este município
                var gruposIBS = nfe.GruposIBS.Where(g => g.UF == grupoMunicipio.Key.UF && g.CodigoMunicipio == grupoMunicipio.Key.CodigoMunicipio);
                totalMunicipio.vBCIBS = gruposIBS.Sum(g => g.vBCIBS);
                totalMunicipio.vIBS = gruposIBS.Sum(g => g.vIBS);

                // Calcular totais CBS para este município
                var gruposCBS = nfe.GruposCBS.Where(g => g.UF == grupoMunicipio.Key.UF && g.CodigoMunicipio == grupoMunicipio.Key.CodigoMunicipio);
                totalMunicipio.vBCCBS = gruposCBS.Sum(g => g.vBCCBS);
                totalMunicipio.vCBS = gruposCBS.Sum(g => g.vCBS);

                // Calcular totais IS para este município
                var gruposIS = nfe.GruposIS.Where(g => g.UF == grupoMunicipio.Key.UF && g.CodigoMunicipio == grupoMunicipio.Key.CodigoMunicipio);
                totalMunicipio.vBCIS = gruposIS.Sum(g => g.vBCIS);
                totalMunicipio.vIS = gruposIS.Sum(g => g.vIS);

                nfe.TotaisPorMunicipio.Add(totalMunicipio);
            }
        }

        #endregion

        #region Cálculos Específicos

        private void CalcularSeparacaoCreditosMonofasicos(NFe2026 nfe)
        {
            // Separar créditos presumidos
            var creditosPresumidos = nfe.GruposIBS.Where(g => g.gCBSCredPres).ToList();
            var creditosPresumidosCBS = nfe.GruposCBS.Where(g => g.gCBSCredPres).ToList();
            var creditosPresumidosIS = nfe.GruposIS.Where(g => g.gCBSCredPres).ToList();

            // Separar operações monofásicas
            var monofasicos = nfe.GruposIBS.Where(g => g.gIBSCBSMono).ToList();
            var monofasicosCBS = nfe.GruposCBS.Where(g => g.gIBSCBSMono).ToList();
            var monofasicosIS = nfe.GruposIS.Where(g => g.gIBSCBSMono).ToList();

            // Calcular totais específicos se necessário
            // Estes valores podem ser usados para relatórios específicos
        }

        private void CalcularTotaisDevolucao(NFe2026 nfe)
        {
            // Calcular totais de devolução baseados nas referências
            foreach (var referencia in nfe.Referencias)
            {
                if (referencia.MotivoReferencia == "DEVOLUCAO")
                {
                    // Buscar grupos relacionados ao item
                    var gruposIBS = nfe.GruposIBS.Where(g => g.nItem == referencia.nItem);
                    var gruposCBS = nfe.GruposCBS.Where(g => g.nItem == referencia.nItem);
                    var gruposIS = nfe.GruposIS.Where(g => g.nItem == referencia.nItem);

                    // Marcar como devolução
                    foreach (var grupo in gruposIBS)
                    {
                        grupo.TipoOperacao = "DEVOLUCAO";
                    }
                    foreach (var grupo in gruposCBS)
                    {
                        grupo.TipoOperacao = "DEVOLUCAO";
                    }
                    foreach (var grupo in gruposIS)
                    {
                        grupo.TipoOperacao = "DEVOLUCAO";
                    }
                }
            }
        }

        #endregion

        #region Cálculos de FCP e Retenções

        private decimal CalcularFCPIBS(NFe2026 nfe)
        {
            // Calcular FCP (Fundo de Combate à Pobreza) para IBS
            // Regras específicas por UF
            decimal fcp = 0;

            foreach (var grupo in nfe.GruposIBS)
            {
                if (grupo.UF == "RJ") // Exemplo: RJ tem FCP
                {
                    fcp += grupo.vIBS * 0.02m; // 2% de FCP
                }
            }

            return fcp;
        }

        private decimal CalcularFCPCBS(NFe2026 nfe)
        {
            // Calcular FCP para CBS
            decimal fcp = 0;

            foreach (var grupo in nfe.GruposCBS)
            {
                if (grupo.UF == "RJ") // Exemplo: RJ tem FCP
                {
                    fcp += grupo.vCBS * 0.02m; // 2% de FCP
                }
            }

            return fcp;
        }

        private decimal CalcularFCPIS(NFe2026 nfe)
        {
            // Calcular FCP para IS
            decimal fcp = 0;

            foreach (var grupo in nfe.GruposIS)
            {
                if (grupo.UF == "RJ") // Exemplo: RJ tem FCP
                {
                    fcp += grupo.vIS * 0.02m; // 2% de FCP
                }
            }

            return fcp;
        }

        private decimal CalcularIBSRetido(NFe2026 nfe)
        {
            // Calcular IBS retido (exemplo: retenção na fonte)
            return nfe.GruposIBS.Where(g => g.TipoOperacao == "RETENCAO").Sum(g => g.vIBS);
        }

        private decimal CalcularCBSRetido(NFe2026 nfe)
        {
            // Calcular CBS retido
            return nfe.GruposCBS.Where(g => g.TipoOperacao == "RETENCAO").Sum(g => g.vCBS);
        }

        private decimal CalcularISRetido(NFe2026 nfe)
        {
            // Calcular IS retido
            return nfe.GruposIS.Where(g => g.TipoOperacao == "RETENCAO").Sum(g => g.vIS);
        }

        #endregion

        #region Métodos Auxiliares

        private decimal ObterAliquotaIBS(string ncm, string uf, int codigoMunicipio)
        {
            // Obter alíquota IBS da tabela de impostos
            // Implementar consulta à tabela de alíquotas
            return _tabelasImpostos.ObterAliquotaIBS(ncm, uf, codigoMunicipio);
        }

        private decimal ObterAliquotaCBS(string ncm, string uf, int codigoMunicipio)
        {
            // Obter alíquota CBS da tabela de impostos
            return _tabelasImpostos.ObterAliquotaCBS(ncm, uf, codigoMunicipio);
        }

        private decimal ObterAliquotaIS(string ncm, string uf, int codigoMunicipio)
        {
            // Obter alíquota IS da tabela de impostos
            return _tabelasImpostos.ObterAliquotaIS(ncm, uf, codigoMunicipio);
        }

        #endregion
    }
}
