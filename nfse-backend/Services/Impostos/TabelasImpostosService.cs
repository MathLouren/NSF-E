using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace nfse_backend.Services.Impostos
{
    public class TabelasImpostosService
    {
        private readonly ILogger<TabelasImpostosService> _logger;
        private readonly Dictionary<string, Dictionary<string, decimal>> _aliquotasICMS;
        private readonly Dictionary<string, decimal> _mvaSubstituicao;
        private readonly Dictionary<string, decimal> _fcpEstados;
        
        public TabelasImpostosService(ILogger<TabelasImpostosService> logger)
        {
            _logger = logger;
            _aliquotasICMS = new Dictionary<string, Dictionary<string, decimal>>();
            _mvaSubstituicao = new Dictionary<string, decimal>();
            _fcpEstados = new Dictionary<string, decimal>();
            
            InicializarTabelas();
        }

        private void InicializarTabelas()
        {
            try
            {
                CarregarAliquotasICMS();
                CarregarMVASubstituicao();
                CarregarFCPEstados();
                _logger.LogInformation("Tabelas de impostos carregadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tabelas de impostos");
                // Carregar tabelas padrão em caso de erro
                CarregarTabelasPadrao();
            }
        }

        private void CarregarAliquotasICMS()
        {
            // Alíquotas ICMS interestaduais (2024)
            var aliquotasInterestaduais = new Dictionary<string, decimal>
            {
                ["AC"] = 7.00m, ["AL"] = 7.00m, ["AP"] = 7.00m, ["AM"] = 7.00m, ["BA"] = 7.00m,
                ["CE"] = 7.00m, ["DF"] = 7.00m, ["ES"] = 7.00m, ["GO"] = 7.00m, ["MA"] = 7.00m,
                ["MT"] = 7.00m, ["MS"] = 7.00m, ["MG"] = 7.00m, ["PA"] = 7.00m, ["PB"] = 7.00m,
                ["PR"] = 7.00m, ["PE"] = 7.00m, ["PI"] = 7.00m, ["RJ"] = 12.00m, ["RN"] = 7.00m,
                ["RS"] = 12.00m, ["RO"] = 7.00m, ["RR"] = 7.00m, ["SC"] = 12.00m, ["SP"] = 12.00m,
                ["SE"] = 7.00m, ["TO"] = 7.00m
            };

            // Alíquotas internas por UF
            var aliquotasInternas = new Dictionary<string, decimal>
            {
                ["RJ"] = 20.00m, // RJ tem alíquota de 20% para operações internas
                ["SP"] = 18.00m,
                ["MG"] = 18.00m,
                ["RS"] = 18.00m,
                ["PR"] = 18.00m,
                ["SC"] = 17.00m
            };

            // Montar matriz completa
            foreach (var ufOrigem in aliquotasInterestaduais.Keys)
            {
                _aliquotasICMS[ufOrigem] = new Dictionary<string, decimal>();
                
                foreach (var ufDestino in aliquotasInterestaduais.Keys)
                {
                    if (ufOrigem == ufDestino)
                    {
                        // Operação interna
                        _aliquotasICMS[ufOrigem][ufDestino] = aliquotasInternas.ContainsKey(ufOrigem) 
                            ? aliquotasInternas[ufOrigem] 
                            : 18.00m;
                    }
                    else
                    {
                        // Operação interestadual
                        _aliquotasICMS[ufOrigem][ufDestino] = aliquotasInterestaduais[ufDestino];
                    }
                }
            }
        }

        private void CarregarMVASubstituicao()
        {
            // MVA por NCM para substituição tributária (RJ)
            _mvaSubstituicao.Clear();
            _mvaSubstituicao["2202"] = 51.28m; // Águas
            _mvaSubstituicao["2203"] = 55.32m; // Cervejas
            _mvaSubstituicao["2204"] = 81.71m; // Vinhos
            _mvaSubstituicao["2208"] = 82.44m; // Destilados
            _mvaSubstituicao["3303"] = 85.71m; // Perfumes
            _mvaSubstituicao["3304"] = 66.67m; // Cosméticos
            _mvaSubstituicao["8703"] = 40.00m; // Automóveis
            _mvaSubstituicao["8704"] = 35.00m; // Caminhões
            _mvaSubstituicao["8711"] = 35.00m; // Motocicletas
            _mvaSubstituicao["2402"] = 336.92m; // Cigarros
        }

        private void CarregarFCPEstados()
        {
            // FCP por estado (2024)
            _fcpEstados.Clear();
            _fcpEstados["RJ"] = 2.00m; // Rio de Janeiro - 2%
            _fcpEstados["CE"] = 2.00m; // Ceará - 2%
            _fcpEstados["BA"] = 2.00m; // Bahia - 2%
            _fcpEstados["AL"] = 2.00m; // Alagoas - 2%
            _fcpEstados["SE"] = 2.00m; // Sergipe - 2%
            _fcpEstados["PE"] = 2.00m; // Pernambuco - 2%
            _fcpEstados["PB"] = 2.00m; // Paraíba - 2%
            _fcpEstados["RN"] = 2.00m; // Rio Grande do Norte - 2%
            _fcpEstados["PI"] = 2.00m; // Piauí - 2%
            _fcpEstados["MA"] = 2.00m; // Maranhão - 2%
            _fcpEstados["AM"] = 1.00m; // Amazonas - 1%
        }

        private void CarregarTabelasPadrao()
        {
            _logger.LogWarning("Carregando tabelas padrão devido a erro na carga");
            
            // Tabela mínima para funcionamento
            _aliquotasICMS["RJ"] = new Dictionary<string, decimal>
            {
                ["RJ"] = 20.00m,
                ["SP"] = 12.00m,
                ["MG"] = 7.00m
            };
            
            _fcpEstados["RJ"] = 2.00m;
            _mvaSubstituicao["DEFAULT"] = 30.00m;
        }

        public decimal ObterAliquotaICMS(string ufOrigem, string ufDestino)
        {
            if (_aliquotasICMS.ContainsKey(ufOrigem) && 
                _aliquotasICMS[ufOrigem].ContainsKey(ufDestino))
            {
                return _aliquotasICMS[ufOrigem][ufDestino];
            }
            
            // Fallback
            return ufOrigem == ufDestino ? 18.00m : 12.00m;
        }

        public decimal ObterMVA(string ncm, string uf = "RJ")
        {
            // Buscar por NCM completo primeiro
            if (_mvaSubstituicao.ContainsKey(ncm))
            {
                return _mvaSubstituicao[ncm];
            }
            
            // Buscar por prefixo do NCM
            foreach (var kvp in _mvaSubstituicao)
            {
                if (ncm.StartsWith(kvp.Key) && kvp.Key != "DEFAULT")
                {
                    return kvp.Value;
                }
            }
            
            return 0; // Não sujeito à substituição tributária
        }

        public decimal ObterFCP(string uf)
        {
            return _fcpEstados.ContainsKey(uf) ? _fcpEstados[uf] : 0;
        }

        public bool TemSubstituicaoTributaria(string ncm, string uf = "RJ")
        {
            return ObterMVA(ncm, uf) > 0;
        }

        public Task AtualizarTabelasAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de tabelas de impostos");
                
                // TODO: Implementar busca automática em APIs oficiais
                // Por exemplo: API da SEFAZ, Receita Federal, etc.
                
                // Por enquanto, recarregar tabelas locais
                InicializarTabelas();
                
                _logger.LogInformation("Tabelas de impostos atualizadas com sucesso");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tabelas de impostos");
                return Task.FromException(ex);
            }
        }

        public Dictionary<string, object> ObterEstatisticasTabelas()
        {
            return new Dictionary<string, object>
            {
                ["TotalUFs"] = _aliquotasICMS.Count,
                ["TotalAliquotasICMS"] = _aliquotasICMS.Values.Sum(d => d.Count),
                ["TotalMVA"] = _mvaSubstituicao.Count,
                ["TotalFCP"] = _fcpEstados.Count,
                ["UltimaAtualizacao"] = DateTime.Now
            };
        }

        #region Métodos para NF-e 2026

        public decimal ObterAliquotaIBS(string ncm, string uf, int codigoMunicipio)
        {
            // Implementar consulta à tabela de alíquotas IBS
            // Por enquanto, retorna valores padrão baseados no NCM e UF
            var ncmInicio = ncm.Substring(0, 4);
            
            // Alíquotas padrão por categoria de produto
            if (ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || ncmInicio.StartsWith("3006"))
                return 18.0m; // Medicamentos
            
            if (ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205"))
                return 25.0m; // Bebidas alcoólicas
            
            if (ncmInicio.StartsWith("2710") || ncmInicio.StartsWith("2711") || ncmInicio.StartsWith("2712"))
                return 15.0m; // Combustíveis
            
            // Alíquota padrão
            return 12.0m;
        }

        public decimal ObterAliquotaCBS(string ncm, string uf, int codigoMunicipio)
        {
            // Implementar consulta à tabela de alíquotas CBS
            var ncmInicio = ncm.Substring(0, 4);
            
            // Alíquotas padrão por categoria de produto
            if (ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || ncmInicio.StartsWith("3006"))
                return 3.0m; // Medicamentos
            
            if (ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205"))
                return 5.0m; // Bebidas alcoólicas
            
            if (ncmInicio.StartsWith("2710") || ncmInicio.StartsWith("2711") || ncmInicio.StartsWith("2712"))
                return 2.0m; // Combustíveis
            
            // Alíquota padrão
            return 1.5m;
        }

        public decimal ObterAliquotaIS(string ncm, string uf, int codigoMunicipio)
        {
            // Implementar consulta à tabela de alíquotas IS
            var ncmInicio = ncm.Substring(0, 4);
            
            // Alíquotas padrão por categoria de produto
            if (ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || ncmInicio.StartsWith("3006"))
                return 0.0m; // Medicamentos isentos
            
            if (ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205"))
                return 8.0m; // Bebidas alcoólicas
            
            if (ncmInicio.StartsWith("2710") || ncmInicio.StartsWith("2711") || ncmInicio.StartsWith("2712"))
                return 0.0m; // Combustíveis isentos
            
            // Alíquota padrão
            return 0.0m;
        }

        public bool ValidarClassificacaoTributaria(string ncm, string uf, int codigoMunicipio, string classificacao)
        {
            // Implementar validação de classificação tributária
            // Verificar se o NCM pode ter a classificação informada na UF/município
            var ncmInicio = ncm.Substring(0, 4);
            
            switch (classificacao.ToUpper())
            {
                case "IBS":
                    // IBS aplicável para a maioria dos produtos
                    return true;
                
                case "CBS":
                    // CBS aplicável para produtos específicos
                    return ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || 
                           ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204");
                
                case "IS":
                    // IS aplicável para produtos seletivos
                    return ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || 
                           ncmInicio.StartsWith("2205") || ncmInicio.StartsWith("2206");
                
                default:
                    return false;
            }
        }

        public Dictionary<string, decimal> ObterAliquotasPorUF(string ncm, int codigoMunicipio)
        {
            // Implementar consulta de alíquotas por UF
            var aliquotas = new Dictionary<string, decimal>();
            
            // Alíquotas padrão por UF (exemplo)
            var ufs = new[] { "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", 
                             "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", 
                             "RS", "RO", "RR", "SC", "SP", "SE", "TO" };
            
            foreach (var uf in ufs)
            {
                aliquotas[uf] = ObterAliquotaIBS(ncm, uf, codigoMunicipio);
            }
            
            return aliquotas;
        }

        public List<string> ObterCodigosBeneficio(string ncm, string uf, int codigoMunicipio)
        {
            // Implementar consulta de códigos de benefício fiscal
            var codigos = new List<string>();
            
            var ncmInicio = ncm.Substring(0, 4);
            
            // Códigos de benefício por categoria
            if (ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || ncmInicio.StartsWith("3006"))
            {
                codigos.Add("MED001"); // Medicamentos essenciais
                codigos.Add("MED002"); // Medicamentos genéricos
            }
            
            if (ncmInicio.StartsWith("2203") || ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205"))
            {
                codigos.Add("BEB001"); // Bebidas alcoólicas
            }
            
            return codigos;
        }

        public bool ValidarOperacaoMonofasica(string ncm, string uf, int codigoMunicipio)
        {
            // Implementar validação de operação monofásica
            var ncmInicio = ncm.Substring(0, 4);
            
            // Produtos que podem ter operação monofásica
            return ncmInicio.StartsWith("2710") || ncmInicio.StartsWith("2711") || 
                   ncmInicio.StartsWith("2712") || ncmInicio.StartsWith("2203") || 
                   ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205");
        }

        public bool ValidarCreditoPresumido(string ncm, string uf, int codigoMunicipio)
        {
            // Implementar validação de crédito presumido
            var ncmInicio = ncm.Substring(0, 4);
            
            // Produtos que podem ter crédito presumido
            return ncmInicio.StartsWith("3004") || ncmInicio.StartsWith("3005") || 
                   ncmInicio.StartsWith("3006") || ncmInicio.StartsWith("2203") || 
                   ncmInicio.StartsWith("2204") || ncmInicio.StartsWith("2205");
        }

        #endregion
    }
}
