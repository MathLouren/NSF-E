using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using nfse_backend.Models.NFe;
using nfse_backend.Services.Monitoramento;

namespace nfse_backend.Services.Audit
{
    /// <summary>
    /// Serviço de auditoria para NF-e 2026
    /// Implementa auditoria de dados IBS/CBS/IS com separação por UF/município
    /// </summary>
    public class NFe2026AuditService
    {
        private readonly AuditoriaService _auditoriaService;
        private readonly MonitoramentoService _monitoramentoService;

        public NFe2026AuditService(AuditoriaService auditoriaService, MonitoramentoService monitoramentoService)
        {
            _auditoriaService = auditoriaService;
            _monitoramentoService = monitoramentoService;
        }

        #region Auditoria Principal

        public void AuditarNFe2026(NFe2026 nfe)
        {
            // 1. Auditoria de conformidade
            AuditarConformidade(nfe);

            // 2. Auditoria de cálculos
            AuditarCalculos(nfe);

            // 3. Auditoria de rastreabilidade
            AuditarRastreabilidade(nfe);

            // 4. Auditoria de eventos fiscais
            AuditarEventosFiscais(nfe);

            // 5. Auditoria de separação por UF/município
            AuditarSeparacaoUFMunicipio(nfe);

            // 6. Gerar hash de validação
            GerarHashValidacao(nfe);

            // 7. Registrar auditoria
            RegistrarAuditoria(nfe);
        }

        #endregion

        #region Auditorias Específicas

        private void AuditarConformidade(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>
            {
                ["TipoAuditoria"] = "CONFORMIDADE_2026",
                ["ChaveAcesso"] = nfe.ChaveAcesso,
                ["VersaoLayout"] = nfe.VersaoLayout,
                ["DataAuditoria"] = DateTime.UtcNow
            };

            // Verificar conformidade com NT 2025.002
            var conformidade = VerificarConformidadeLayout(nfe);
            auditoria["Conforme"] = conformidade.Conforme;
            auditoria["Inconformidades"] = conformidade.Inconformidades;

            // Verificar grupos obrigatórios
            var gruposObrigatorios = VerificarGruposObrigatorios(nfe);
            auditoria["GruposObrigatorios"] = gruposObrigatorios;

            _auditoriaService.RegistrarAuditoria("NFe2026_Conformidade", System.Text.Json.JsonSerializer.Serialize(auditoria));
        }

        private void AuditarCalculos(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>
            {
                ["TipoAuditoria"] = "CALCULOS_2026",
                ["ChaveAcesso"] = nfe.ChaveAcesso,
                ["DataAuditoria"] = DateTime.UtcNow
            };

            // Auditoria de cálculos IBS
            var calculosIBS = AuditarCalculosIBS(nfe);
            auditoria["CalculosIBS"] = calculosIBS;

            // Auditoria de cálculos CBS
            var calculosCBS = AuditarCalculosCBS(nfe);
            auditoria["CalculosCBS"] = calculosCBS;

            // Auditoria de cálculos IS
            var calculosIS = AuditarCalculosIS(nfe);
            auditoria["CalculosIS"] = calculosIS;

            // Verificar consistência dos totais
            var consistenciaTotais = VerificarConsistenciaTotais(nfe);
            auditoria["ConsistenciaTotais"] = consistenciaTotais;

            _auditoriaService.RegistrarAuditoria("NFe2026_Calculos", System.Text.Json.JsonSerializer.Serialize(auditoria));
        }

        private void AuditarRastreabilidade(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>
            {
                ["TipoAuditoria"] = "RASTREABILIDADE_2026",
                ["ChaveAcesso"] = nfe.ChaveAcesso,
                ["DataAuditoria"] = DateTime.UtcNow
            };

            var itensComRastreabilidade = nfe.Rastreabilidade.Count;
            var itensExigemRastreabilidade = 0;
            var itensSemRastreabilidade = new List<int>();

            foreach (var item in nfe.Det)
            {
                if (ExigeRastreabilidade(item.Prod.NCM, item.Prod.xProd))
                {
                    itensExigemRastreabilidade++;
                    
                    var rastreabilidade = nfe.Rastreabilidade.FirstOrDefault(r => r.nItem == item.nItem);
                    if (rastreabilidade == null)
                    {
                        itensSemRastreabilidade.Add(item.nItem);
                    }
                }
            }

            auditoria["ItensComRastreabilidade"] = itensComRastreabilidade;
            auditoria["ItensExigemRastreabilidade"] = itensExigemRastreabilidade;
            auditoria["ItensSemRastreabilidade"] = itensSemRastreabilidade;
            auditoria["ConformidadeRastreabilidade"] = itensSemRastreabilidade.Count == 0;

            _auditoriaService.RegistrarAuditoria("NFe2026_Rastreabilidade", System.Text.Json.JsonSerializer.Serialize(auditoria));
        }

        private void AuditarEventosFiscais(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>
            {
                ["TipoAuditoria"] = "EVENTOS_FISCAIS_2026",
                ["ChaveAcesso"] = nfe.ChaveAcesso,
                ["DataAuditoria"] = DateTime.UtcNow
            };

            var eventos = nfe.EventosFiscais.Select(e => new
            {
                e.TipoEvento,
                e.DataEvento,
                e.ValorEvento,
                e.StatusEvento,
                e.ProtocoloEvento
            }).ToList();

            auditoria["EventosFiscais"] = eventos;
            auditoria["QuantidadeEventos"] = eventos.Count;

            // Verificar eventos pendentes
            var eventosPendentes = eventos.Count(e => e.StatusEvento == "PENDENTE");
            auditoria["EventosPendentes"] = eventosPendentes;

            _auditoriaService.RegistrarAuditoria("NFe2026_EventosFiscais", System.Text.Json.JsonSerializer.Serialize(auditoria));
        }

        private void AuditarSeparacaoUFMunicipio(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>
            {
                ["TipoAuditoria"] = "SEPARACAO_UF_MUNICIPIO_2026",
                ["ChaveAcesso"] = nfe.ChaveAcesso,
                ["DataAuditoria"] = DateTime.UtcNow
            };

            // Auditoria por UF
            var auditoriaUF = AuditarPorUF(nfe);
            auditoria["AuditoriaUF"] = auditoriaUF;

            // Auditoria por município
            var auditoriaMunicipio = AuditarPorMunicipio(nfe);
            auditoria["AuditoriaMunicipio"] = auditoriaMunicipio;

            // Verificar separação de créditos e monofásicos
            var separacaoCreditos = AuditarSeparacaoCreditos(nfe);
            auditoria["SeparacaoCreditos"] = separacaoCreditos;

            _auditoriaService.RegistrarAuditoria("NFe2026_SeparacaoUFMunicipio", System.Text.Json.JsonSerializer.Serialize(auditoria));
        }

        #endregion

        #region Auditorias Detalhadas

        private ConformidadeLayout VerificarConformidadeLayout(NFe2026 nfe)
        {
            var conformidade = new ConformidadeLayout
            {
                VersaoLayout = nfe.VersaoLayout,
                NumeroNotaTecnica = "2025.002",
                DataVigencia = new DateTime(2026, 1, 1),
                Conforme = true
            };

            // Verificar versão do layout
            if (nfe.VersaoLayout != "2026.001")
            {
                conformidade.Inconformidades.Add($"Versão do layout {nfe.VersaoLayout} não está atualizada");
                conformidade.Conforme = false;
            }

            // Verificar grupos obrigatórios
            if (!nfe.GruposIBS.Any() && !nfe.GruposCBS.Any() && !nfe.GruposIS.Any())
            {
                conformidade.Inconformidades.Add("Layout 2026 exige pelo menos um grupo IBS, CBS ou IS");
                conformidade.Conforme = false;
            }

            // Verificar campos obrigatórios
            foreach (var item in nfe.Det)
            {
                var gruposIBS = nfe.GruposIBS.Where(g => g.nItem == item.nItem);
                var gruposCBS = nfe.GruposCBS.Where(g => g.nItem == item.nItem);
                var gruposIS = nfe.GruposIS.Where(g => g.nItem == item.nItem);

                foreach (var grupo in gruposIBS)
                {
                    if (string.IsNullOrEmpty(grupo.UF))
                    {
                        conformidade.Inconformidades.Add($"UF não informada no grupo IBS do item {grupo.nItem}");
                        conformidade.Conforme = false;
                    }

                    if (grupo.CodigoMunicipio <= 0)
                    {
                        conformidade.Inconformidades.Add($"Código do município não informado no grupo IBS do item {grupo.nItem}");
                        conformidade.Conforme = false;
                    }
                }
            }

            return conformidade;
        }

        private Dictionary<string, object> VerificarGruposObrigatorios(NFe2026 nfe)
        {
            var grupos = new Dictionary<string, object>
            {
                ["GruposIBS"] = nfe.GruposIBS.Count,
                ["GruposCBS"] = nfe.GruposCBS.Count,
                ["GruposIS"] = nfe.GruposIS.Count,
                ["TotaisPorUF"] = nfe.TotaisPorUF.Count,
                ["TotaisPorMunicipio"] = nfe.TotaisPorMunicipio.Count,
                ["Referencias"] = nfe.Referencias.Count,
                ["Rastreabilidade"] = nfe.Rastreabilidade.Count,
                ["EventosFiscais"] = nfe.EventosFiscais.Count
            };

            // Verificar se pelo menos um grupo está presente
            grupos["TemGruposObrigatorios"] = nfe.GruposIBS.Any() || nfe.GruposCBS.Any() || nfe.GruposIS.Any();

            return grupos;
        }

        private Dictionary<string, object> AuditarCalculosIBS(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>();

            // Verificar consistência dos cálculos
            var totalCalculado = nfe.GruposIBS.Sum(g => g.vIBS);
            var totalInformado = nfe.TotaisIBS.vIBS;
            var diferenca = Math.Abs(totalCalculado - totalInformado);

            auditoria["TotalCalculado"] = totalCalculado;
            auditoria["TotalInformado"] = totalInformado;
            auditoria["Diferenca"] = diferenca;
            auditoria["Consistente"] = diferenca < 0.01m;

            // Verificar alíquotas
            var aliqutotas = nfe.GruposIBS.Select(g => g.pIBS).Distinct().ToList();
            auditoria["Aliquotas"] = aliqutotas;
            auditoria["AliquotasValidas"] = aliqutotas.All(a => a >= 0 && a <= 100);

            // Verificar bases de cálculo
            var basesCalculo = nfe.GruposIBS.Select(g => g.vBCIBS).ToList();
            auditoria["BasesCalculo"] = basesCalculo;
            auditoria["BasesCalculoValidas"] = basesCalculo.All(b => b >= 0);

            return auditoria;
        }

        private Dictionary<string, object> AuditarCalculosCBS(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>();

            var totalCalculado = nfe.GruposCBS.Sum(g => g.vCBS);
            var totalInformado = nfe.TotaisCBS.vCBS;
            var diferenca = Math.Abs(totalCalculado - totalInformado);

            auditoria["TotalCalculado"] = totalCalculado;
            auditoria["TotalInformado"] = totalInformado;
            auditoria["Diferenca"] = diferenca;
            auditoria["Consistente"] = diferenca < 0.01m;

            var aliqutotas = nfe.GruposCBS.Select(g => g.pCBS).Distinct().ToList();
            auditoria["Aliquotas"] = aliqutotas;
            auditoria["AliquotasValidas"] = aliqutotas.All(a => a >= 0 && a <= 100);

            var basesCalculo = nfe.GruposCBS.Select(g => g.vBCCBS).ToList();
            auditoria["BasesCalculo"] = basesCalculo;
            auditoria["BasesCalculoValidas"] = basesCalculo.All(b => b >= 0);

            return auditoria;
        }

        private Dictionary<string, object> AuditarCalculosIS(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>();

            var totalCalculado = nfe.GruposIS.Sum(g => g.vIS);
            var totalInformado = nfe.TotaisIS.vIS;
            var diferenca = Math.Abs(totalCalculado - totalInformado);

            auditoria["TotalCalculado"] = totalCalculado;
            auditoria["TotalInformado"] = totalInformado;
            auditoria["Diferenca"] = diferenca;
            auditoria["Consistente"] = diferenca < 0.01m;

            var aliqutotas = nfe.GruposIS.Select(g => g.pIS).Distinct().ToList();
            auditoria["Aliquotas"] = aliqutotas;
            auditoria["AliquotasValidas"] = aliqutotas.All(a => a >= 0 && a <= 100);

            var basesCalculo = nfe.GruposIS.Select(g => g.vBCIS).ToList();
            auditoria["BasesCalculo"] = basesCalculo;
            auditoria["BasesCalculoValidas"] = basesCalculo.All(b => b >= 0);

            return auditoria;
        }

        private Dictionary<string, object> VerificarConsistenciaTotais(NFe2026 nfe)
        {
            var consistencia = new Dictionary<string, object>();

            // Verificar totais por UF
            var totaisUFConsistentes = true;
            foreach (var totalUF in nfe.TotaisPorUF)
            {
                var gruposIBS = nfe.GruposIBS.Where(g => g.UF == totalUF.UF);
                var gruposCBS = nfe.GruposCBS.Where(g => g.UF == totalUF.UF);
                var gruposIS = nfe.GruposIS.Where(g => g.UF == totalUF.UF);

                var totalIBS = gruposIBS.Sum(g => g.vIBS);
                var totalCBS = gruposCBS.Sum(g => g.vCBS);
                var totalIS = gruposIS.Sum(g => g.vIS);

                if (Math.Abs(totalUF.vIBS - totalIBS) > 0.01m ||
                    Math.Abs(totalUF.vCBS - totalCBS) > 0.01m ||
                    Math.Abs(totalUF.vIS - totalIS) > 0.01m)
                {
                    totaisUFConsistentes = false;
                    break;
                }
            }

            consistencia["TotaisUFConsistentes"] = totaisUFConsistentes;
            consistencia["TotaisMunicipioConsistentes"] = VerificarConsistenciaTotaisMunicipio(nfe);

            return consistencia;
        }

        private bool VerificarConsistenciaTotaisMunicipio(NFe2026 nfe)
        {
            foreach (var totalMunicipio in nfe.TotaisPorMunicipio)
            {
                var gruposIBS = nfe.GruposIBS.Where(g => g.UF == totalMunicipio.UF && g.CodigoMunicipio == totalMunicipio.CodigoMunicipio);
                var gruposCBS = nfe.GruposCBS.Where(g => g.UF == totalMunicipio.UF && g.CodigoMunicipio == totalMunicipio.CodigoMunicipio);
                var gruposIS = nfe.GruposIS.Where(g => g.UF == totalMunicipio.UF && g.CodigoMunicipio == totalMunicipio.CodigoMunicipio);

                var totalIBS = gruposIBS.Sum(g => g.vIBS);
                var totalCBS = gruposCBS.Sum(g => g.vCBS);
                var totalIS = gruposIS.Sum(g => g.vIS);

                if (Math.Abs(totalMunicipio.vIBS - totalIBS) > 0.01m ||
                    Math.Abs(totalMunicipio.vCBS - totalCBS) > 0.01m ||
                    Math.Abs(totalMunicipio.vIS - totalIS) > 0.01m)
                {
                    return false;
                }
            }

            return true;
        }

        private Dictionary<string, object> AuditarPorUF(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>();

            var ufs = nfe.GruposIBS.Concat(nfe.GruposCBS.Cast<object>()).Concat(nfe.GruposIS.Cast<object>())
                .Select(g => ((dynamic)g).UF).Distinct().ToList();

            foreach (var uf in ufs)
            {
                var gruposIBS = nfe.GruposIBS.Where(g => g.UF == uf);
                var gruposCBS = nfe.GruposCBS.Where(g => g.UF == uf);
                var gruposIS = nfe.GruposIS.Where(g => g.UF == uf);

                auditoria[uf] = new
                {
                    GruposIBS = gruposIBS.Count(),
                    GruposCBS = gruposCBS.Count(),
                    GruposIS = gruposIS.Count(),
                    TotalIBS = gruposIBS.Sum(g => g.vIBS),
                    TotalCBS = gruposCBS.Sum(g => g.vCBS),
                    TotalIS = gruposIS.Sum(g => g.vIS),
                    Itens = gruposIBS.Concat(gruposCBS.Cast<object>()).Concat(gruposIS.Cast<object>())
                        .Select(g => ((dynamic)g).nItem).Distinct().Count()
                };
            }

            return auditoria;
        }

        private Dictionary<string, object> AuditarPorMunicipio(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>();

            var municipios = nfe.GruposIBS.Concat(nfe.GruposCBS.Cast<object>()).Concat(nfe.GruposIS.Cast<object>())
                .Select(g => new { UF = ((dynamic)g).UF, Codigo = ((dynamic)g).CodigoMunicipio })
                .Distinct().ToList();

            foreach (var municipio in municipios)
            {
                var gruposIBS = nfe.GruposIBS.Where(g => g.UF == municipio.UF && g.CodigoMunicipio == municipio.Codigo);
                var gruposCBS = nfe.GruposCBS.Where(g => g.UF == municipio.UF && g.CodigoMunicipio == municipio.Codigo);
                var gruposIS = nfe.GruposIS.Where(g => g.UF == municipio.UF && g.CodigoMunicipio == municipio.Codigo);

                var chave = $"{municipio.UF}_{municipio.Codigo}";
                auditoria[chave] = new
                {
                    UF = municipio.UF,
                    CodigoMunicipio = municipio.Codigo,
                    GruposIBS = gruposIBS.Count(),
                    GruposCBS = gruposCBS.Count(),
                    GruposIS = gruposIS.Count(),
                    TotalIBS = gruposIBS.Sum(g => g.vIBS),
                    TotalCBS = gruposCBS.Sum(g => g.vCBS),
                    TotalIS = gruposIS.Sum(g => g.vIS)
                };
            }

            return auditoria;
        }

        private Dictionary<string, object> AuditarSeparacaoCreditos(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>();

            // Créditos presumidos
            var creditosPresumidosIBS = nfe.GruposIBS.Where(g => g.gCBSCredPres).Count();
            var creditosPresumidosCBS = nfe.GruposCBS.Where(g => g.gCBSCredPres).Count();
            var creditosPresumidosIS = nfe.GruposIS.Where(g => g.gCBSCredPres).Count();

            // Operações monofásicas
            var monofasicosIBS = nfe.GruposIBS.Where(g => g.gIBSCBSMono).Count();
            var monofasicosCBS = nfe.GruposCBS.Where(g => g.gIBSCBSMono).Count();
            var monofasicosIS = nfe.GruposIS.Where(g => g.gIBSCBSMono).Count();

            auditoria["CreditosPresumidos"] = new
            {
                IBS = creditosPresumidosIBS,
                CBS = creditosPresumidosCBS,
                IS = creditosPresumidosIS,
                Total = creditosPresumidosIBS + creditosPresumidosCBS + creditosPresumidosIS
            };

            auditoria["Monofasicos"] = new
            {
                IBS = monofasicosIBS,
                CBS = monofasicosCBS,
                IS = monofasicosIS,
                Total = monofasicosIBS + monofasicosCBS + monofasicosIS
            };

            return auditoria;
        }

        #endregion

        #region Métodos Auxiliares

        private bool ExigeRastreabilidade(string ncm, string descricao)
        {
            if (string.IsNullOrEmpty(ncm) || ncm.Length < 4)
                return false;

            var ncmInicio = ncm.Substring(0, 4);

            // Medicamentos
            if (ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || ncmInicio.StartsWith("3006"))
                return true;

            // Bebidas alcoólicas
            if (ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205") ||
                ncmInicio.StartsWith("2206") || ncmInicio.StartsWith("2207") || ncmInicio.StartsWith("2208"))
                return true;

            // Combustíveis
            if (ncmInicio.StartsWith("2710") || ncmInicio.StartsWith("2711") || ncmInicio.StartsWith("2712"))
                return true;

            return false;
        }

        private void GerarHashValidacao(NFe2026 nfe)
        {
            // Gerar hash baseado nos dados críticos da NF-e
            var dadosCriticos = new StringBuilder();
            dadosCriticos.Append(nfe.ChaveAcesso);
            dadosCriticos.Append(nfe.TotaisIBS.vIBS.ToString("F2"));
            dadosCriticos.Append(nfe.TotaisCBS.vCBS.ToString("F2"));
            dadosCriticos.Append(nfe.TotaisIS.vIS.ToString("F2"));
            dadosCriticos.Append(nfe.VersaoLayout);

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dadosCriticos.ToString()));
                nfe.HashValidacao = Convert.ToBase64String(hashBytes);
            }
        }

        private void RegistrarAuditoria(NFe2026 nfe)
        {
            var auditoria = new Dictionary<string, object>
            {
                ["TipoAuditoria"] = "NFe2026_COMPLETA",
                ["ChaveAcesso"] = nfe.ChaveAcesso,
                ["VersaoLayout"] = nfe.VersaoLayout,
                ["HashValidacao"] = nfe.HashValidacao,
                ["DataAuditoria"] = DateTime.UtcNow,
                ["Status"] = "AUDITADO"
            };

            _auditoriaService.RegistrarAuditoria("NFe2026_AuditoriaCompleta", System.Text.Json.JsonSerializer.Serialize(auditoria));

            // Registrar no monitoramento
            _monitoramentoService.RegistrarEvento("NFe2026_Auditoria", "NF-e 2026 auditada com sucesso", nfe.ChaveAcesso);
        }

        #endregion
    }
}
