using Microsoft.AspNetCore.Mvc;

namespace nfse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiscalTablesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetFiscalTables()
        {
            try
            {
                var tables = new
                {
                    cst = new[]
                    {
                        new { code = "00", description = "Tributada integralmente", operationType = "produto" },
                        new { code = "10", description = "Tributada e com cobrança do ICMS por substituição tributária", operationType = "produto" },
                        new { code = "20", description = "Com redução de base de cálculo", operationType = "produto" },
                        new { code = "30", description = "Isenta ou não tributada e com cobrança do ICMS por substituição tributária", operationType = "produto" },
                        new { code = "40", description = "Isenta", operationType = "produto" },
                        new { code = "41", description = "Não tributada", operationType = "produto" },
                        new { code = "50", description = "Suspensão", operationType = "produto" },
                        new { code = "51", description = "Diferimento", operationType = "produto" },
                        new { code = "60", description = "ICMS cobrado anteriormente por substituição tributária", operationType = "produto" },
                        new { code = "70", description = "Com redução de base de cálculo e cobrança do ICMS por substituição tributária", operationType = "produto" },
                        new { code = "90", description = "Outras", operationType = "produto" }
                    },
                    cfop = new[]
                    {
                        new { code = "1102", description = "Compra para comercialização", operationType = "compra" },
                        new { code = "5102", description = "Venda de mercadoria adquirida ou recebida de terceiros", operationType = "venda" },
                        new { code = "1202", description = "Compra para industrialização", operationType = "compra" },
                        new { code = "5202", description = "Venda de mercadoria industrializada", operationType = "venda" },
                        new { code = "1203", description = "Compra para comercialização", operationType = "compra" },
                        new { code = "5203", description = "Venda de mercadoria adquirida ou recebida de terceiros", operationType = "venda" }
                    },
                    ncm = new[]
                    {
                        new { code = "61091000", description = "Camisetas de malha de algodão" },
                        new { code = "61091000", description = "Vestuário de malha de algodão" },
                        new { code = "62034200", description = "Calças de malha de algodão" },
                        new { code = "62034200", description = "Vestuário de malha de algodão" }
                    },
                    gtin = new[]
                    {
                        new { code = "7891234567890", description = "Produto exemplo 1" },
                        new { code = "7891234567891", description = "Produto exemplo 2" }
                    },
                    cnae = new[]
                    {
                        new { code = "4711301", description = "Comércio varejista de mercadorias em geral, com predominância de produtos alimentícios - hipermercados" },
                        new { code = "4711302", description = "Comércio varejista de mercadorias em geral, com predominância de produtos alimentícios - supermercados" }
                    },
                    municipios = new[]
                    {
                        new { codigo = "3304557", nome = "São João de Meriti", uf = "RJ" },
                        new { codigo = "3303302", nome = "Rio de Janeiro", uf = "RJ" },
                        new { codigo = "3550308", nome = "São Paulo", uf = "SP" }
                    },
                    ufs = new[]
                    {
                        new { codigo = "33", nome = "Rio de Janeiro", sigla = "RJ" },
                        new { codigo = "35", nome = "São Paulo", sigla = "SP" },
                        new { codigo = "31", nome = "Minas Gerais", sigla = "MG" }
                    },
                    ibsRates = new
                    {
                        RJ = new { geral = 8.5, alimenticio = 8.5, medicamento = 8.5 },
                        SP = new { geral = 8.5, alimenticio = 8.5, medicamento = 8.5 },
                        MG = new { geral = 8.5, alimenticio = 8.5, medicamento = 8.5 }
                    },
                    cbsRates = new
                    {
                        RJ = new { geral = 1.5, alimenticio = 1.5, medicamento = 1.5 },
                        SP = new { geral = 1.5, alimenticio = 1.5, medicamento = 1.5 },
                        MG = new { geral = 1.5, alimenticio = 1.5, medicamento = 1.5 }
                    },
                    isRates = new
                    {
                        municipio3304557 = new { geral = 2.0, consultoria = 2.0, tecnologia = 2.0 },
                        municipio3303302 = new { geral = 2.0, consultoria = 2.0, tecnologia = 2.0 },
                        municipio3550308 = new { geral = 2.0, consultoria = 2.0, tecnologia = 2.0 }
                    }
                };

                return Ok(tables);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao carregar tabelas fiscais: {ex.Message}"
                });
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateFiscalData([FromBody] object fiscalData)
        {
            try
            {
                // Implementar validação fiscal aqui
                return Ok(new
                {
                    sucesso = true,
                    valido = true,
                    mensagem = "Dados fiscais válidos"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro na validação fiscal: {ex.Message}"
                });
            }
        }
    }
}
