using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using nfse_backend.Models.NFe;
using nfse_backend.Services.Impostos;

namespace nfse_backend.Services.Validation
{
    /// <summary>
    /// Serviço de validação específico para NF-e 2026 conforme NT 2025.002
    /// Implementa validações de códigos tributários, regras condicionais e conformidade
    /// </summary>
    public class NFe2026ValidationService
    {
        private readonly TabelasImpostosService _tabelasImpostos;
        private readonly Dictionary<string, List<string>> _codigosValidos;

        public NFe2026ValidationService(TabelasImpostosService tabelasImpostos)
        {
            _tabelasImpostos = tabelasImpostos;
            _codigosValidos = new Dictionary<string, List<string>>();
            InicializarCodigosValidos();
        }

        #region Validação Principal

        public ValidacaoNFE2026 ValidarNFe2026(NFe2026 nfe)
        {
            var validacao = new ValidacaoNFE2026();

            // 1. Validar schema XML
            validacao.SchemaValido = ValidarSchema(nfe);

            // 2. Validar regras de negócio
            validacao.RegrasNegocioValidas = ValidarRegrasNegocio(nfe, validacao.ErrosValidacao);

            // 3. Validar códigos tributários
            validacao.CodigosTributariosValidos = ValidarCodigosTributarios(nfe, validacao.ErrosValidacao);

            // 4. Validar rastreabilidade
            validacao.RastreabilidadeValida = ValidarRastreabilidade(nfe, validacao.ErrosValidacao);

            // 5. Validar totais
            validacao.TotaisCalculadosCorretamente = ValidarTotais(nfe, validacao.ErrosValidacao);

            // 6. Validar conformidade com layout 2026
            ValidarConformidadeLayout(nfe, validacao);

            return validacao;
        }

        #endregion

        #region Validações Específicas

        private bool ValidarSchema(NFe2026 nfe)
        {
            // Validação básica de campos obrigatórios
            var erros = new List<string>();

            if (string.IsNullOrEmpty(nfe.ChaveAcesso))
                erros.Add("Chave de acesso é obrigatória");

            if (nfe.Ide == null)
                erros.Add("Identificação da NF-e é obrigatória");

            if (nfe.Emit == null)
                erros.Add("Emitente é obrigatório");

            if (nfe.Dest == null)
                erros.Add("Destinatário é obrigatório");

            if (nfe.Det == null || !nfe.Det.Any())
                erros.Add("Pelo menos um item deve ser informado");

            // Validar grupos obrigatórios 2026
            if (!nfe.GruposIBS.Any() && !nfe.GruposCBS.Any() && !nfe.GruposIS.Any())
                erros.Add("Pelo menos um grupo IBS, CBS ou IS deve ser informado");

            return !erros.Any();
        }

        private bool ValidarRegrasNegocio(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Validar tipo de operação
            valido &= ValidarTipoOperacao(nfe, erros);

            // Validar preenchimento condicional
            valido &= ValidarPreenchimentoCondicional(nfe, erros);

            // Validar operações de devolução/crédito
            valido &= ValidarOperacoesDevolucaoCredito(nfe, erros);

            // Validar eventos fiscais
            valido &= ValidarEventosFiscais(nfe, erros);

            return valido;
        }

        private bool ValidarCodigosTributarios(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            foreach (var item in nfe.Det)
            {
                // Validar NCM
                if (!ValidarNCM(item.Prod.NCM))
                {
                    erros.Add($"NCM inválido no item {item.nItem}: {item.Prod.NCM}");
                    valido = false;
                }

                // Validar CFOP
                if (!ValidarCFOP(item.Prod.CFOP, nfe.Ide.tpNF, nfe.Dest.EnderDest.UF, nfe.Emit.EnderEmit.UF))
                {
                    erros.Add($"CFOP inválido no item {item.nItem}: {item.Prod.CFOP}");
                    valido = false;
                }

                // Validar CST/CSOSN
                if (!ValidarCST(item.Imposto.ICMS.CST, item.Imposto.ICMS.orig, nfe.Emit.CRT))
                {
                    erros.Add($"CST/CSOSN inválido no item {item.nItem}: {item.Imposto.ICMS.CST}");
                    valido = false;
                }

                // Validar classificação tributária IBS/CBS/IS
                valido &= ValidarClassificacaoTributaria(item, nfe, erros);
            }

            return valido;
        }

        private bool ValidarRastreabilidade(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            foreach (var item in nfe.Det)
            {
                // Verificar se item exige rastreabilidade
                if (ExigeRastreabilidade(item.Prod.NCM, item.Prod.xProd))
                {
                    var rastreabilidade = nfe.Rastreabilidade.FirstOrDefault(r => r.nItem == item.nItem);
                    
                    if (rastreabilidade == null)
                    {
                        erros.Add($"Item {item.nItem} exige rastreabilidade mas não foi informada");
                        valido = false;
                    }
                    else
                    {
                        // Validar campos obrigatórios de rastreabilidade
                        if (string.IsNullOrEmpty(rastreabilidade.GTIN))
                        {
                            erros.Add($"GTIN é obrigatório para item {item.nItem}");
                            valido = false;
                        }

                        if (string.IsNullOrEmpty(rastreabilidade.NumeroLote))
                        {
                            erros.Add($"Número do lote é obrigatório para item {item.nItem}");
                            valido = false;
                        }
                    }
                }
            }

            return valido;
        }

        private bool ValidarTotais(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Validar totais IBS
            valido &= ValidarTotaisIBS(nfe, erros);

            // Validar totais CBS
            valido &= ValidarTotaisCBS(nfe, erros);

            // Validar totais IS
            valido &= ValidarTotaisIS(nfe, erros);

            // Validar totais por UF
            valido &= ValidarTotaisPorUF(nfe, erros);

            // Validar totais por município
            valido &= ValidarTotaisPorMunicipio(nfe, erros);

            return valido;
        }

        #endregion

        #region Validações de Códigos

        private bool ValidarNCM(string ncm)
        {
            if (string.IsNullOrEmpty(ncm))
                return false;

            // NCM deve ter 8 dígitos
            if (ncm.Length != 8)
                return false;

            // Deve conter apenas números
            return Regex.IsMatch(ncm, @"^\d{8}$");
        }

        private bool ValidarCFOP(int cfop, int tpNF, string ufDestino, string ufOrigem)
        {
            // CFOP deve ter 4 dígitos
            if (cfop < 1000 || cfop > 9999)
                return false;

            // Validar regras de CFOP baseadas no tipo de operação e UFs
            var cfopStr = cfop.ToString();

            if (tpNF == 1) // Saída
            {
                if (ufOrigem == ufDestino)
                {
                    // Operação interna - CFOP deve começar com 5
                    return cfopStr.StartsWith("5");
                }
                else
                {
                    // Operação interestadual - CFOP deve começar com 6
                    return cfopStr.StartsWith("6");
                }
            }
            else if (tpNF == 0) // Entrada
            {
                if (ufOrigem == ufDestino)
                {
                    // Operação interna - CFOP deve começar com 1
                    return cfopStr.StartsWith("1");
                }
                else
                {
                    // Operação interestadual - CFOP deve começar com 2
                    return cfopStr.StartsWith("2");
                }
            }

            return false;
        }

        private bool ValidarCST(string cst, string origem, int crt)
        {
            if (string.IsNullOrEmpty(cst) || string.IsNullOrEmpty(origem))
                return false;

            // Validar origem (0-8)
            if (!int.TryParse(origem, out int orig) || orig < 0 || orig > 8)
                return false;

            // Validar CST baseado no regime tributário
            if (crt == 1) // Simples Nacional
            {
                // CSOSN deve ter 3 dígitos
                if (cst.Length != 3)
                    return false;

                var csosn = int.Parse(cst);
                return csosn >= 101 && csosn <= 900;
            }
            else
            {
                // CST deve ter 2 dígitos
                if (cst.Length != 2)
                    return false;

                var cstInt = int.Parse(cst);
                return cstInt >= 0 && cstInt <= 90;
            }
        }

        private bool ValidarClassificacaoTributaria(DetalheNFe item, NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Verificar se item tem classificação tributária válida
            var gruposIBS = nfe.GruposIBS.Where(g => g.nItem == item.nItem);
            var gruposCBS = nfe.GruposCBS.Where(g => g.nItem == item.nItem);
            var gruposIS = nfe.GruposIS.Where(g => g.nItem == item.nItem);

            if (!gruposIBS.Any() && !gruposCBS.Any() && !gruposIS.Any())
            {
                erros.Add($"Item {item.nItem} deve ter pelo menos uma classificação tributária (IBS, CBS ou IS)");
                valido = false;
            }

            // Validar valores de base de cálculo e alíquotas
            foreach (var grupo in gruposIBS)
            {
                if (grupo.vBCIBS <= 0)
                {
                    erros.Add($"Base de cálculo IBS inválida no item {item.nItem}");
                    valido = false;
                }

                if (grupo.pIBS < 0 || grupo.pIBS > 100)
                {
                    erros.Add($"Alíquota IBS inválida no item {item.nItem}: {grupo.pIBS}%");
                    valido = false;
                }
            }

            return valido;
        }

        #endregion

        #region Validações de Regras de Negócio

        private bool ValidarTipoOperacao(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Validar natureza da operação
            if (string.IsNullOrEmpty(nfe.Ide.natOp))
            {
                erros.Add("Natureza da operação é obrigatória");
                valido = false;
            }

            // Validar tipo de NF-e
            if (nfe.Ide.tpNF != 0 && nfe.Ide.tpNF != 1)
            {
                erros.Add("Tipo de NF-e deve ser 0 (Entrada) ou 1 (Saída)");
                valido = false;
            }

            return valido;
        }

        private bool ValidarPreenchimentoCondicional(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Validar campos condicionais baseados no tipo de operação
            foreach (var item in nfe.Det)
            {
                // Se é operação de devolução, deve ter referência
                if (nfe.Ide.natOp.ToUpper().Contains("DEVOLUÇÃO") || nfe.Ide.natOp.ToUpper().Contains("DEVOLUCAO"))
                {
                    var referencia = nfe.Referencias.FirstOrDefault(r => r.nItem == item.nItem);
                    if (referencia == null)
                    {
                        erros.Add($"Operação de devolução no item {item.nItem} deve ter documento de referência");
                        valido = false;
                    }
                }

                // Validar campos monofásicos
                var gruposMonofasicos = nfe.GruposIBS.Where(g => g.nItem == item.nItem && g.gIBSCBSMono);
                foreach (var grupo in gruposMonofasicos)
                {
                    if (grupo.pDif == null || grupo.pDif <= 0)
                    {
                        erros.Add($"Operação monofásica no item {item.nItem} deve ter percentual diferencial");
                        valido = false;
                    }
                }
            }

            return valido;
        }

        private bool ValidarOperacoesDevolucaoCredito(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            foreach (var referencia in nfe.Referencias)
            {
                // Validar chave de acesso referenciada
                if (string.IsNullOrEmpty(referencia.ChaveAcessoReferenciada))
                {
                    erros.Add($"Chave de acesso referenciada é obrigatória no item {referencia.nItem}");
                    valido = false;
                }
                else if (referencia.ChaveAcessoReferenciada.Length != 44)
                {
                    erros.Add($"Chave de acesso referenciada inválida no item {referencia.nItem}");
                    valido = false;
                }

                // Validar data de emissão referenciada
                if (referencia.DataEmissaoReferenciada > DateTime.Now)
                {
                    erros.Add($"Data de emissão referenciada não pode ser futura no item {referencia.nItem}");
                    valido = false;
                }
            }

            return valido;
        }

        private bool ValidarEventosFiscais(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            foreach (var evento in nfe.EventosFiscais)
            {
                // Validar tipo de evento
                var tiposValidos = new[] { "CREDITO_PRESUMIDO", "PERDA_ROUBO", "CANCELAMENTO", "TRANSFERENCIA_CREDITO" };
                if (!tiposValidos.Contains(evento.TipoEvento))
                {
                    erros.Add($"Tipo de evento fiscal inválido: {evento.TipoEvento}");
                    valido = false;
                }

                // Validar justificativa para eventos que exigem
                if (evento.TipoEvento == "PERDA_ROUBO" && string.IsNullOrEmpty(evento.Justificativa))
                {
                    erros.Add("Evento de perda/roubo deve ter justificativa");
                    valido = false;
                }
            }

            return valido;
        }

        #endregion

        #region Validações de Totais

        private bool ValidarTotaisIBS(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Calcular totais esperados
            var totalVBCIBS = nfe.GruposIBS.Sum(g => g.vBCIBS);
            var totalVIBS = nfe.GruposIBS.Sum(g => g.vIBS);

            // Comparar com totais informados
            if (Math.Abs(nfe.TotaisIBS.vBCIBS - totalVBCIBS) > 0.01m)
            {
                erros.Add($"Total de base de cálculo IBS não confere. Esperado: {totalVBCIBS:N2}, Informado: {nfe.TotaisIBS.vBCIBS:N2}");
                valido = false;
            }

            if (Math.Abs(nfe.TotaisIBS.vIBS - totalVIBS) > 0.01m)
            {
                erros.Add($"Total de IBS não confere. Esperado: {totalVIBS:N2}, Informado: {nfe.TotaisIBS.vIBS:N2}");
                valido = false;
            }

            return valido;
        }

        private bool ValidarTotaisCBS(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            var totalVBCCBS = nfe.GruposCBS.Sum(g => g.vBCCBS);
            var totalVCBS = nfe.GruposCBS.Sum(g => g.vCBS);

            if (Math.Abs(nfe.TotaisCBS.vBCCBS - totalVBCCBS) > 0.01m)
            {
                erros.Add($"Total de base de cálculo CBS não confere. Esperado: {totalVBCCBS:N2}, Informado: {nfe.TotaisCBS.vBCCBS:N2}");
                valido = false;
            }

            if (Math.Abs(nfe.TotaisCBS.vCBS - totalVCBS) > 0.01m)
            {
                erros.Add($"Total de CBS não confere. Esperado: {totalVCBS:N2}, Informado: {nfe.TotaisCBS.vCBS:N2}");
                valido = false;
            }

            return valido;
        }

        private bool ValidarTotaisIS(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            var totalVBCIS = nfe.GruposIS.Sum(g => g.vBCIS);
            var totalVIS = nfe.GruposIS.Sum(g => g.vIS);

            if (Math.Abs(nfe.TotaisIS.vBCIS - totalVBCIS) > 0.01m)
            {
                erros.Add($"Total de base de cálculo IS não confere. Esperado: {totalVBCIS:N2}, Informado: {nfe.TotaisIS.vBCIS:N2}");
                valido = false;
            }

            if (Math.Abs(nfe.TotaisIS.vIS - totalVIS) > 0.01m)
            {
                erros.Add($"Total de IS não confere. Esperado: {totalVIS:N2}, Informado: {nfe.TotaisIS.vIS:N2}");
                valido = false;
            }

            return valido;
        }

        private bool ValidarTotaisPorUF(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Agrupar por UF e validar totais
            var gruposPorUF = nfe.GruposIBS.Concat(nfe.GruposCBS.Cast<object>()).Concat(nfe.GruposIS.Cast<object>())
                .GroupBy(g => ((dynamic)g).UF);

            foreach (var grupoUF in gruposPorUF)
            {
                var totalUF = nfe.TotaisPorUF.FirstOrDefault(t => t.UF == grupoUF.Key);
                if (totalUF == null)
                {
                    erros.Add($"Total por UF não encontrado para {grupoUF.Key}");
                    valido = false;
                }
            }

            return valido;
        }

        private bool ValidarTotaisPorMunicipio(NFe2026 nfe, List<string> erros)
        {
            bool valido = true;

            // Similar à validação por UF, mas agrupando por município
            var gruposPorMunicipio = nfe.GruposIBS.Concat(nfe.GruposCBS.Cast<object>()).Concat(nfe.GruposIS.Cast<object>())
                .GroupBy(g => new { UF = ((dynamic)g).UF, CodigoMunicipio = ((dynamic)g).CodigoMunicipio });

            foreach (var grupoMunicipio in gruposPorMunicipio)
            {
                var totalMunicipio = nfe.TotaisPorMunicipio.FirstOrDefault(t => 
                    t.UF == grupoMunicipio.Key.UF && t.CodigoMunicipio == grupoMunicipio.Key.CodigoMunicipio);
                
                if (totalMunicipio == null)
                {
                    erros.Add($"Total por município não encontrado para {grupoMunicipio.Key.UF}/{grupoMunicipio.Key.CodigoMunicipio}");
                    valido = false;
                }
            }

            return valido;
        }

        #endregion

        #region Métodos Auxiliares

        private bool ExigeRastreabilidade(string ncm, string descricao)
        {
            // Verificar se NCM exige rastreabilidade (medicamentos, bebidas, etc.)
            var ncmInicio = ncm.Substring(0, 4);
            
            // Medicamentos (3004, 3005, 3006)
            if (ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || ncmInicio.StartsWith("3006"))
                return true;

            // Bebidas alcoólicas (2203, 2204, 2205, 2206, 2207, 2208)
            if (ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205") ||
                ncmInicio.StartsWith("2206") || ncmInicio.StartsWith("2207") || ncmInicio.StartsWith("2208"))
                return true;

            // Combustíveis (2710, 2711, 2712)
            if (ncmInicio.StartsWith("2710") || ncmInicio.StartsWith("2711") || ncmInicio.StartsWith("2712"))
                return true;

            return false;
        }

        private void ValidarConformidadeLayout(NFe2026 nfe, ValidacaoNFE2026 validacao)
        {
            // Verificar se está usando a versão correta do layout
            if (nfe.VersaoLayout != "2026.001")
            {
                validacao.AvisosValidacao.Add($"Versão do layout {nfe.VersaoLayout} pode não estar atualizada");
            }

            // Verificar se todos os grupos obrigatórios estão presentes
            if (!nfe.GruposIBS.Any() && !nfe.GruposCBS.Any() && !nfe.GruposIS.Any())
            {
                validacao.ErrosValidacao.Add("Layout 2026 exige pelo menos um grupo IBS, CBS ou IS");
            }
        }

        private void InicializarCodigosValidos()
        {
            // Inicializar códigos válidos para validação
            _codigosValidos["CST"] = new List<string> { "00", "10", "20", "30", "40", "41", "50", "51", "60", "70", "90" };
            _codigosValidos["CSOSN"] = new List<string> { "101", "102", "103", "201", "202", "203", "300", "400", "500", "900" };
            _codigosValidos["Origem"] = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8" };
        }

        #endregion
    }
}
