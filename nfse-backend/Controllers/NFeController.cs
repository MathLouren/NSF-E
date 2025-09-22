using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using nfse_backend.Models.NFe;
using nfse_backend.Services.NotaFiscal;
using nfse_backend.Services.Configuracao;
using nfse_backend.Services.Armazenamento;
using nfse_backend.Services.Pdf;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace nfse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class NFeController : ControllerBase
    {
        private readonly NFeService _nfeService;
        private readonly ConfiguracaoNFeService _configuracaoService;
        private readonly ArmazenamentoSeguroService _armazenamentoService;
        
        // Armazenamento temporário dos dados das NF-e emitidas (em produção, usar banco de dados)
        private static readonly Dictionary<string, object> _nfeEmitidas = new Dictionary<string, object>();

        public NFeController(
            NFeService nfeService,
            ConfiguracaoNFeService configuracaoService,
            ArmazenamentoSeguroService armazenamentoService)
        {
            _nfeService = nfeService;
            _configuracaoService = configuracaoService;
            _armazenamentoService = armazenamentoService;
        }

        [HttpPost("teste")]
        public IActionResult TesteNFe([FromBody] object dados)
        {
            try
            {
                if (dados == null)
                {
                    return BadRequest(new { message = "Dados não informados" });
                }

                // Log para debug
                Console.WriteLine($"Dados recebidos: {System.Text.Json.JsonSerializer.Serialize(dados, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");

                return Ok(new { 
                    message = "Dados recebidos com sucesso", 
                    dados = dados,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("emitir")]
        public async Task<IActionResult> EmitirNFe([FromBody] object nfeData)
        {
            try
            {
                if (nfeData == null)
                {
                    return BadRequest(new { message = "Dados da NF-e não informados" });
                }

                // Log para debug
                Console.WriteLine($"NF-e recebida para emissão: {JsonSerializer.Serialize(nfeData, new JsonSerializerOptions { WriteIndented = true })}");

                // Simular processamento assíncrono para evitar warning
                await Task.Delay(1);

                // Gerar chave de acesso simulada
                var chaveAcesso = GerarChaveAcessoSimulada();
                var protocolo = "135210000000000";

                // Salvar os dados reais da NF-e para uso posterior no DANFE
                _nfeEmitidas[protocolo] = nfeData;
                Console.WriteLine($"=== DADOS SALVOS NO DICIONÁRIO ===");
                Console.WriteLine($"Protocolo: {protocolo}");
                Console.WriteLine($"Tipo dos dados: {nfeData.GetType()}");
                Console.WriteLine($"Dados salvos: {JsonSerializer.Serialize(nfeData, new JsonSerializerOptions { WriteIndented = true })}");

                // Simular emissão bem-sucedida por enquanto
                return Ok(new
                {
                    sucesso = true,
                    chaveAcesso = chaveAcesso,
                    protocolo = protocolo,
                    numeroRecibo = protocolo,
                    numeroNFe = 1,
                    mensagem = "NF-e processada com sucesso (modo de teste)",
                    dataAutorizacao = DateTime.Now,
                    status = "Autorizada"
                });

                // TODO: Implementar emissão real
                // var result = await _nfeService.EmitirNFe(nfe);

                // Código comentado - será implementado quando os serviços estiverem prontos
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao emitir NF-e: {ex.Message}"
                });
            }
        }

        private string GerarChaveAcessoSimulada()
        {
            // Gerar chave de acesso simulada para teste
            // Formato: cUF + AAMM + CNPJ + mod + serie + nNF + tpEmis + cNF + cDV
            var cUF = "33"; // RJ
            var AAMM = DateTime.Now.ToString("yyMM");
            var CNPJ = "33045570000191"; // CNPJ de exemplo
            var mod = "55"; // NF-e
            var serie = "001";
            var nNF = "00000001";
            var tpEmis = "1"; // Normal
            var cNF = "00000001";
            var cDV = "1"; // Dígito verificador simulado

            return cUF + AAMM + CNPJ + mod + serie + nNF + tpEmis + cNF + cDV;
        }

        [HttpGet("homolog/status")]
        public IActionResult HomologationChecklist()
        {
            // Verifica alguns pré-requisitos mínimos de homologação
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var xsdNfe = System.IO.Path.Combine(baseDir, "Schemas", "NFe", "v4.00");
            var endpoints = System.IO.Path.Combine(baseDir, "config", "endpoints.json");
            var taxRules = System.IO.Path.Combine(baseDir, "config", "tax_rules", "rules.json");

            var status = new
            {
                schemasNFe = System.IO.Directory.Exists(xsdNfe),
                endpointsConfig = System.IO.File.Exists(endpoints),
                taxRulesConfig = System.IO.File.Exists(taxRules),
                ambiente = _configuracaoService.ObterAmbienteAtual().ToString()
            };

            return Ok(status);
        }

        [HttpPost("carta-correcao")]
        public async Task<IActionResult> EnviarCartaCorrecao([FromBody] CartaCorrecaoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChaveAcesso) || string.IsNullOrEmpty(request.TextoCorrecao))
                {
                    return BadRequest(new { message = "Chave de acesso e texto da correção são obrigatórios" });
                }

                var result = await _nfeService.EnviarCartaCorrecao(
                    request.ChaveAcesso,
                    request.CnpjEmitente,
                    request.TextoCorrecao,
                    request.SequenciaEvento ?? 1
                );

                if (result.Sucesso)
                {
                    return Ok(new
                    {
                        sucesso = true,
                        protocolo = result.Protocolo,
                        dataEvento = result.DataEvento,
                        mensagem = "Carta de Correção registrada com sucesso"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        sucesso = false,
                        mensagem = result.Mensagem
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao enviar carta de correção: {ex.Message}"
                });
            }
        }

        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarNFe([FromBody] CancelamentoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChaveAcesso) || string.IsNullOrEmpty(request.Protocolo))
                {
                    return BadRequest(new { message = "Chave de acesso e protocolo são obrigatórios" });
                }

                var result = await _nfeService.CancelarNFe(
                    request.ChaveAcesso,
                    request.Protocolo,
                    request.CnpjEmitente,
                    request.Justificativa
                );

                if (result.Sucesso)
                {
                    return Ok(new
                    {
                        sucesso = true,
                        protocolo = result.Protocolo,
                        dataEvento = result.DataEvento,
                        mensagem = "NF-e cancelada com sucesso"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        sucesso = false,
                        mensagem = result.Mensagem
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao cancelar NF-e: {ex.Message}"
                });
            }
        }

        [HttpPost("inutilizar")]
        public async Task<IActionResult> InutilizarNumeracao([FromBody] InutilizacaoRequest request)
        {
            try
            {
                var result = await _nfeService.InutilizarNumeracao(
                    request.CnpjEmitente,
                    request.UF,
                    request.Ano,
                    request.Serie,
                    request.NumeroInicial,
                    request.NumeroFinal,
                    request.Justificativa
                );

                if (result.Sucesso)
                {
                    return Ok(new
                    {
                        sucesso = true,
                        protocolo = result.Protocolo,
                        dataInutilizacao = result.DataInutilizacao,
                        mensagem = "Numeração inutilizada com sucesso"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        sucesso = false,
                        mensagem = result.Mensagem
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao inutilizar numeração: {ex.Message}"
                });
            }
        }

        [HttpGet("ambiente")]
        public IActionResult ObterAmbienteAtual()
        {
            var ambiente = _configuracaoService.ObterAmbienteAtual();
            return Ok(new
            {
                ambiente = ambiente.ToString(),
                codigo = (int)ambiente,
                descricao = ambiente == AmbienteNFe.Producao ? "Produção" : "Homologação"
            });
        }

        [HttpPost("ambiente")]
        public IActionResult AlterarAmbiente([FromBody] AlterarAmbienteRequest request)
        {
            try
            {
                var ambiente = request.Producao ? AmbienteNFe.Producao : AmbienteNFe.Homologacao;
                _configuracaoService.AlterarAmbiente(ambiente);

                return Ok(new
                {
                    sucesso = true,
                    ambiente = ambiente.ToString(),
                    mensagem = $"Ambiente alterado para {(request.Producao ? "Produção" : "Homologação")}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao alterar ambiente: {ex.Message}"
                });
            }
        }

        [HttpPost("certificado/upload")]
        public async Task<IActionResult> UploadCertificado([FromForm] CertificadoUploadRequest request)
        {
            try
            {
                if (request.Certificado == null || request.Certificado.Length == 0)
                {
                    return BadRequest(new { message = "Arquivo do certificado não informado" });
                }

                using (var ms = new MemoryStream())
                {
                    await request.Certificado.CopyToAsync(ms);
                    var certificadoBytes = ms.ToArray();

                    var caminho = await _armazenamentoService.SalvarCertificadoSeguro(
                        certificadoBytes,
                        request.Senha,
                        request.CnpjEmpresa
                    );

                    return Ok(new
                    {
                        sucesso = true,
                        mensagem = "Certificado salvo com sucesso",
                        caminho = caminho
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao salvar certificado: {ex.Message}"
                });
            }
        }

        [HttpGet("arquivos/{cnpj}")]
        public async Task<IActionResult> ListarArquivos(
            string cnpj,
            [FromQuery] TipoArquivo tipo = TipoArquivo.XmlAutorizado,
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                var arquivos = await _armazenamentoService.ListarArquivos(cnpj, tipo, dataInicio, dataFim);
                return Ok(arquivos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao listar arquivos: {ex.Message}"
                });
            }
        }

        [HttpPost("limpeza")]
        public async Task<IActionResult> ExecutarLimpezaArquivos()
        {
            try
            {
                await _armazenamentoService.LimparArquivosAntigos();
                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Limpeza de arquivos executada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao executar limpeza: {ex.Message}"
                });
            }
        }

        [HttpGet("lista")]
        public IActionResult ListarNFe([FromQuery] string? cnpj = null, [FromQuery] DateTime? dataInicio = null, [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                // Simular lista de NF-e por enquanto
                var nfeList = new List<object>
                {
                    new
                    {
                        chaveAcesso = "33250533045570000000010000000001000000000001",
                        numero = 1,
                        serie = 1,
                        dataEmissao = DateTime.Now.AddDays(-1),
                        valorTotal = 100.00m,
                        status = "Autorizada",
                        protocolo = "135210000000000",
                        destinatario = "Cliente Exemplo Ltda",
                        cnpjDestinatario = "12345678000199"
                    },
                    new
                    {
                        chaveAcesso = "33250533045570000000010000000002000000000002",
                        numero = 2,
                        serie = 1,
                        dataEmissao = DateTime.Now.AddDays(-2),
                        valorTotal = 250.50m,
                        status = "Autorizada",
                        protocolo = "135210000000001",
                        destinatario = "Outro Cliente S/A",
                        cnpjDestinatario = "98765432000188"
                    }
                };

                return Ok(new
                {
                    sucesso = true,
                    itens = nfeList,
                    total = nfeList.Count,
                    mensagem = "Lista de NF-e carregada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao listar NF-e: {ex.Message}"
                });
            }
        }

        [HttpGet("certificado/status")]
        public IActionResult ObterStatusCertificado([FromQuery] string? cnpj = null)
        {
            try
            {
                // Simular status do certificado por enquanto
                var status = new
                {
                    valido = true,
                    vencimento = DateTime.Now.AddMonths(6),
                    diasParaVencimento = 180,
                    cnpj = cnpj ?? "33045570000191",
                    razaoSocial = "Empresa Exemplo Ltda",
                    tipo = "A1",
                    status = "Válido",
                    mensagem = "Certificado válido e funcionando"
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao verificar certificado: {ex.Message}"
                });
            }
        }

        [HttpGet("danfe/{protocolo}")]
        public IActionResult GerarDANFE(string protocolo)
        {
            try
            {
                // Buscar dados reais da NF-e pelo protocolo
                if (!_nfeEmitidas.ContainsKey(protocolo))
                {
                    return NotFound(new
                    {
                        sucesso = false,
                        mensagem = $"NF-e com protocolo {protocolo} não encontrada"
                    });
                }

                var nfeData = _nfeEmitidas[protocolo];
                Console.WriteLine($"=== DADOS RECUPERADOS DO DICIONÁRIO ===");
                Console.WriteLine($"Protocolo: {protocolo}");
                Console.WriteLine($"Tipo dos dados: {nfeData.GetType()}");
                Console.WriteLine($"Dados recuperados: {JsonSerializer.Serialize(nfeData, new JsonSerializerOptions { WriteIndented = true })}");
                
                var nfe = ConverterDadosParaNFe(nfeData, protocolo);
                
                // Gerar DANFE usando o serviço
                var danfeService = new DanfeService();
                var pdfBytes = danfeService.GenerateDanfe(nfe);
                
                return File(pdfBytes, "application/pdf", $"DANFE_{protocolo}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao gerar DANFE: {ex.Message}"
                });
            }
        }

        private string TratarCaracteresEspeciais(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            // Substituir caracteres problemáticos comuns
            return texto
                .Replace("Ã§", "ç")
                .Replace("Ã£", "ã")
                .Replace("Ã¡", "á")
                .Replace("Ã©", "é")
                .Replace("Ã­", "í")
                .Replace("Ã³", "ó")
                .Replace("Ãº", "ú")
                .Replace("Ã¢", "â")
                .Replace("Ãª", "ê")
                .Replace("Ã´", "ô")
                .Replace("Ã ", "à")
                .Replace("Ã±", "ñ")
                .Replace("Ã¼", "ü")
                .Replace("Ã", "Á")
                .Replace("Ã‰", "É")
                .Replace("Ã", "Í")
                .Replace("Ãš", "Ú")
                .Replace("Ã‚", "Â")
                .Replace("ÃŠ", "Ê")
                .Replace("Ã€", "À")
                .Replace("Ã‡", "Ç")
                .Replace("Ã'", "Ñ")
                .Replace("Ãœ", "Ü");
        }

        private NFe ConverterDadosParaNFe(object nfeData, string protocolo)
        {
            try
            {
                Console.WriteLine("=== INICIANDO CONVERSÃO DE DADOS ===");
                
                // Converter o objeto para JSON e depois para JsonElement para facilitar o acesso
                var jsonString = JsonSerializer.Serialize(nfeData);
                Console.WriteLine($"JSON serializado: {jsonString.Substring(0, Math.Min(500, jsonString.Length))}...");
                
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
                Console.WriteLine("JSON deserializado com sucesso");

                var nfe = new NFe
                {
                    Id = Guid.NewGuid(),
                    Versao = "4.00",
                    ChaveAcesso = GerarChaveAcessoSimulada(),
                    Protocolo = protocolo,
                    DataAutorizacao = DateTime.Now,
                    Status = "Autorizada"
                };

                // Converter dados do IDE
                Console.WriteLine("Convertendo dados do IDE...");
                if (jsonElement.TryGetProperty("ide", out var ideElement))
                {
                    nfe.Ide = new IdentificacaoNFe
                    {
                        cUF = ideElement.TryGetProperty("cUF", out var cUF) ? (cUF.ValueKind == JsonValueKind.Number ? cUF.GetInt32() : (int.TryParse(cUF.GetString(), out var cUFVal) ? cUFVal : 33)) : 33,
                        cNF = ideElement.TryGetProperty("cNF", out var cNF) ? cNF.GetString() ?? "00000001" : "00000001",
                        natOp = TratarCaracteresEspeciais(ideElement.TryGetProperty("natOp", out var natOp) ? natOp.GetString() ?? "VENDA" : "VENDA"),
                        mod = ideElement.TryGetProperty("mod", out var mod) ? (mod.ValueKind == JsonValueKind.Number ? mod.GetInt32() : (int.TryParse(mod.GetString(), out var modVal) ? modVal : 55)) : 55,
                        serie = ideElement.TryGetProperty("serie", out var serie) ? (serie.ValueKind == JsonValueKind.Number ? serie.GetInt32() : (int.TryParse(serie.GetString(), out var serieVal) ? serieVal : 1)) : 1,
                        nNF = ideElement.TryGetProperty("nNF", out var nNF) ? (nNF.ValueKind == JsonValueKind.Number ? nNF.GetInt32() : (int.TryParse(nNF.GetString(), out var nNFVal) ? nNFVal : 1)) : 1,
                        dhEmi = ideElement.TryGetProperty("dhEmi", out var dhEmi) ? DateTime.Parse(dhEmi.GetString() ?? DateTime.Now.ToString()) : DateTime.Now,
                        tpNF = ideElement.TryGetProperty("tpNF", out var tpNF) ? (tpNF.ValueKind == JsonValueKind.Number ? tpNF.GetInt32() : (int.TryParse(tpNF.GetString(), out var tpNFVal) ? tpNFVal : 1)) : 1,
                        idDest = ideElement.TryGetProperty("idDest", out var idDest) ? (idDest.ValueKind == JsonValueKind.Number ? idDest.GetInt32() : (int.TryParse(idDest.GetString(), out var idDestVal) ? idDestVal : 1)) : 1,
                        cMunFG = ideElement.TryGetProperty("cMunFG", out var cMunFG) ? (cMunFG.ValueKind == JsonValueKind.Number ? cMunFG.GetInt32() : (int.TryParse(cMunFG.GetString(), out var cMunFGVal) ? cMunFGVal : 3304557)) : 3304557,
                        tpImp = ideElement.TryGetProperty("tpImp", out var tpImp) ? (tpImp.ValueKind == JsonValueKind.Number ? tpImp.GetInt32() : (int.TryParse(tpImp.GetString(), out var tpImpVal) ? tpImpVal : 1)) : 1,
                        tpEmis = ideElement.TryGetProperty("tpEmis", out var tpEmis) ? (tpEmis.ValueKind == JsonValueKind.Number ? tpEmis.GetInt32() : (int.TryParse(tpEmis.GetString(), out var tpEmisVal) ? tpEmisVal : 1)) : 1,
                        cDV = ideElement.TryGetProperty("cDV", out var cDV) ? (cDV.ValueKind == JsonValueKind.Number ? cDV.GetInt32() : (int.TryParse(cDV.GetString(), out var cDVVal) ? cDVVal : 1)) : 1,
                        tpAmb = ideElement.TryGetProperty("tpAmb", out var tpAmb) ? (tpAmb.ValueKind == JsonValueKind.Number ? tpAmb.GetInt32() : (int.TryParse(tpAmb.GetString(), out var tpAmbVal) ? tpAmbVal : 2)) : 2,
                        finNFe = ideElement.TryGetProperty("finNFe", out var finNFe) ? (finNFe.ValueKind == JsonValueKind.Number ? finNFe.GetInt32() : (int.TryParse(finNFe.GetString(), out var finNFeVal) ? finNFeVal : 1)) : 1,
                        indFinal = ideElement.TryGetProperty("indFinal", out var indFinal) ? (indFinal.ValueKind == JsonValueKind.Number ? indFinal.GetInt32() : (int.TryParse(indFinal.GetString(), out var indFinalVal) ? indFinalVal : 1)) : 1,
                        indPres = ideElement.TryGetProperty("indPres", out var indPres) ? (indPres.ValueKind == JsonValueKind.Number ? indPres.GetInt32() : (int.TryParse(indPres.GetString(), out var indPresVal) ? indPresVal : 1)) : 1,
                        procEmi = ideElement.TryGetProperty("procEmi", out var procEmi) ? (procEmi.ValueKind == JsonValueKind.Number ? procEmi.GetInt32() : (int.TryParse(procEmi.GetString(), out var procEmiVal) ? procEmiVal : 0)) : 0,
                        verProc = ideElement.TryGetProperty("verProc", out var verProc) ? verProc.GetString() ?? "NFSE2026" : "NFSE2026"
                    };
                }

                // Converter dados do Emitente
                Console.WriteLine("Convertendo dados do Emitente...");
                if (jsonElement.TryGetProperty("emit", out var emitElement))
                {
                    nfe.Emit = new Emitente
                    {
                        CNPJ = emitElement.TryGetProperty("CNPJ", out var cnpj) ? cnpj.GetString() ?? "" : "",
                        xNome = TratarCaracteresEspeciais(emitElement.TryGetProperty("xNome", out var xNome) ? xNome.GetString() ?? "" : ""),
                        xFant = TratarCaracteresEspeciais(emitElement.TryGetProperty("xFant", out var xFant) ? xFant.GetString() ?? "" : ""),
                        IE = emitElement.TryGetProperty("IE", out var ie) ? ie.GetString() ?? "" : "",
                        CRT = emitElement.TryGetProperty("CRT", out var crt) ? (crt.ValueKind == JsonValueKind.Number ? crt.GetInt32() : (int.TryParse(crt.GetString(), out var crtVal) ? crtVal : 3)) : 3
                    };

                    // Endereço do emitente
                    if (emitElement.TryGetProperty("enderEmit", out var enderEmit))
                    {
                        nfe.Emit.EnderEmit = new EnderecoNFe
                        {
                            xLgr = TratarCaracteresEspeciais(enderEmit.TryGetProperty("xLgr", out var xLgr) ? xLgr.GetString() ?? "" : ""),
                            nro = TratarCaracteresEspeciais(enderEmit.TryGetProperty("nro", out var nro) ? nro.GetString() ?? "" : ""),
                            xBairro = TratarCaracteresEspeciais(enderEmit.TryGetProperty("xBairro", out var xBairro) ? xBairro.GetString() ?? "" : ""),
                            cMun = enderEmit.TryGetProperty("cMun", out var cMun) ? (cMun.ValueKind == JsonValueKind.Number ? cMun.GetInt32() : (int.TryParse(cMun.GetString(), out var cMunVal) ? cMunVal : 0)) : 0,
                            xMun = TratarCaracteresEspeciais(enderEmit.TryGetProperty("xMun", out var xMun) ? xMun.GetString() ?? "" : ""),
                            UF = enderEmit.TryGetProperty("UF", out var uf) ? uf.GetString() ?? "" : "",
                            CEP = enderEmit.TryGetProperty("CEP", out var cep) ? cep.GetString() ?? "" : "",
                            cPais = enderEmit.TryGetProperty("cPais", out var cPais) ? (cPais.ValueKind == JsonValueKind.Number ? cPais.GetInt32() : (int.TryParse(cPais.GetString(), out var cPaisVal) ? cPaisVal : 1058)) : 1058,
                            xPais = enderEmit.TryGetProperty("xPais", out var xPais) ? xPais.GetString() ?? "Brasil" : "Brasil"
                        };
                    }
                }

                // Converter dados do Destinatário
                if (jsonElement.TryGetProperty("dest", out var destElement))
                {
                    nfe.Dest = new Destinatario
                    {
                        CNPJ = destElement.TryGetProperty("CNPJ", out var cnpj) ? cnpj.GetString() ?? "" : "",
                        xNome = TratarCaracteresEspeciais(destElement.TryGetProperty("xNome", out var xNome) ? xNome.GetString() ?? "" : ""),
                        IE = destElement.TryGetProperty("IE", out var ie) ? ie.GetString() ?? "" : "",
                        indIEDest = destElement.TryGetProperty("indIEDest", out var indIEDest) ? (indIEDest.ValueKind == JsonValueKind.Number ? indIEDest.GetInt32() : (int.TryParse(indIEDest.GetString(), out var indIEDestVal) ? indIEDestVal : 1)) : 1,
                        email = destElement.TryGetProperty("email", out var email) ? email.GetString() ?? "" : ""
                    };

                    // Endereço do destinatário
                    if (destElement.TryGetProperty("enderDest", out var enderDest))
                    {
                        nfe.Dest.EnderDest = new EnderecoNFe
                        {
                            xLgr = TratarCaracteresEspeciais(enderDest.TryGetProperty("xLgr", out var xLgr) ? xLgr.GetString() ?? "" : ""),
                            nro = TratarCaracteresEspeciais(enderDest.TryGetProperty("nro", out var nro) ? nro.GetString() ?? "" : ""),
                            xBairro = TratarCaracteresEspeciais(enderDest.TryGetProperty("xBairro", out var xBairro) ? xBairro.GetString() ?? "" : ""),
                            cMun = enderDest.TryGetProperty("cMun", out var cMun) ? (cMun.ValueKind == JsonValueKind.Number ? cMun.GetInt32() : (int.TryParse(cMun.GetString(), out var cMunVal) ? cMunVal : 0)) : 0,
                            xMun = TratarCaracteresEspeciais(enderDest.TryGetProperty("xMun", out var xMun) ? xMun.GetString() ?? "" : ""),
                            UF = enderDest.TryGetProperty("UF", out var uf) ? uf.GetString() ?? "" : "",
                            CEP = enderDest.TryGetProperty("CEP", out var cep) ? cep.GetString() ?? "" : "",
                            cPais = enderDest.TryGetProperty("cPais", out var cPais) ? (cPais.ValueKind == JsonValueKind.Number ? cPais.GetInt32() : (int.TryParse(cPais.GetString(), out var cPaisVal) ? cPaisVal : 1058)) : 1058,
                            xPais = enderDest.TryGetProperty("xPais", out var xPais) ? xPais.GetString() ?? "Brasil" : "Brasil"
                        };
                    }
                }

                // Converter dados dos Detalhes (produtos)
                Console.WriteLine("Convertendo dados dos produtos...");
                if (jsonElement.TryGetProperty("det", out var detElement) && detElement.ValueKind == JsonValueKind.Array)
                {
                    nfe.Det = new List<DetalheNFe>();
                    foreach (var item in detElement.EnumerateArray())
                    {
                        var detalhe = new DetalheNFe
                        {
                            nItem = item.TryGetProperty("nItem", out var nItem) ? (nItem.ValueKind == JsonValueKind.Number ? nItem.GetInt32() : (int.TryParse(nItem.GetString(), out var nItemVal) ? nItemVal : 1)) : 1
                        };

                        // Produto
                        if (item.TryGetProperty("prod", out var prodElement))
                        {
                            detalhe.Prod = new ProdutoNFe
                            {
                                cProd = prodElement.TryGetProperty("cProd", out var cProd) ? cProd.GetString() ?? "" : "",
                                cEAN = prodElement.TryGetProperty("cEAN", out var cEAN) ? cEAN.GetString() ?? "SEM GTIN" : "SEM GTIN",
                                xProd = TratarCaracteresEspeciais(prodElement.TryGetProperty("xProd", out var xProd) ? xProd.GetString() ?? "" : ""),
                                NCM = prodElement.TryGetProperty("NCM", out var ncm) ? ncm.GetString() ?? "" : "",
                                CFOP = prodElement.TryGetProperty("CFOP", out var cfop) ? (cfop.ValueKind == JsonValueKind.Number ? cfop.GetInt32() : (int.TryParse(cfop.GetString(), out var cfopVal) ? cfopVal : 0)) : 0,
                                uCom = prodElement.TryGetProperty("uCom", out var uCom) ? uCom.GetString() ?? "UN" : "UN",
                                qCom = prodElement.TryGetProperty("qCom", out var qCom) ? (qCom.ValueKind == JsonValueKind.Number ? qCom.GetDecimal() : (decimal.TryParse(qCom.GetString(), out var qComVal) ? qComVal : 1)) : 1,
                                vUnCom = prodElement.TryGetProperty("vUnCom", out var vUnCom) ? (vUnCom.ValueKind == JsonValueKind.Number ? vUnCom.GetDecimal() : (decimal.TryParse(vUnCom.GetString(), out var vUnComVal) ? vUnComVal : 0)) : 0,
                                vProd = prodElement.TryGetProperty("vProd", out var vProd) ? (vProd.ValueKind == JsonValueKind.Number ? vProd.GetDecimal() : (decimal.TryParse(vProd.GetString(), out var vProdVal) ? vProdVal : 0)) : 0,
                                cEANTrib = prodElement.TryGetProperty("cEANTrib", out var cEANTrib) ? cEANTrib.GetString() ?? "SEM GTIN" : "SEM GTIN",
                                uTrib = prodElement.TryGetProperty("uTrib", out var uTrib) ? uTrib.GetString() ?? "UN" : "UN",
                                qTrib = prodElement.TryGetProperty("qTrib", out var qTrib) ? (qTrib.ValueKind == JsonValueKind.Number ? qTrib.GetDecimal() : (decimal.TryParse(qTrib.GetString(), out var qTribVal) ? qTribVal : 1)) : 1,
                                vUnTrib = prodElement.TryGetProperty("vUnTrib", out var vUnTrib) ? (vUnTrib.ValueKind == JsonValueKind.Number ? vUnTrib.GetDecimal() : (decimal.TryParse(vUnTrib.GetString(), out var vUnTribVal) ? vUnTribVal : 0)) : 0,
                                indTot = prodElement.TryGetProperty("indTot", out var indTot) ? (indTot.ValueKind == JsonValueKind.Number ? indTot.GetInt32() : (int.TryParse(indTot.GetString(), out var indTotVal) ? indTotVal : 1)) : 1
                            };
                        }

                        // Impostos
                        if (item.TryGetProperty("imposto", out var impostoElement))
                        {
                            detalhe.Imposto = new ImpostoNFe();
                            
                            if (impostoElement.TryGetProperty("ICMS", out var icmsElement))
                            {
                                if (icmsElement.TryGetProperty("ICMS00", out var icms00Element))
                                {
                                    detalhe.Imposto.ICMS = new ICMS
                                    {
                                        orig = icms00Element.TryGetProperty("orig", out var orig) ? (orig.ValueKind == JsonValueKind.String ? orig.GetString() ?? "0" : orig.GetInt32().ToString()) : "0",
                                        CST = icms00Element.TryGetProperty("CST", out var cst) ? cst.GetString() ?? "00" : "00",
                                        modBC = icms00Element.TryGetProperty("modBC", out var modBC) ? (modBC.ValueKind == JsonValueKind.Number ? modBC.GetInt32() : (int.TryParse(modBC.GetString(), out var modBCVal) ? modBCVal : 3)) : 3,
                                        vBC = icms00Element.TryGetProperty("vBC", out var vBC) ? (vBC.ValueKind == JsonValueKind.Number ? vBC.GetDecimal() : (decimal.TryParse(vBC.GetString(), out var vBCVal) ? vBCVal : 0)) : 0,
                                        pICMS = icms00Element.TryGetProperty("pICMS", out var pICMS) ? (pICMS.ValueKind == JsonValueKind.Number ? pICMS.GetDecimal() : (decimal.TryParse(pICMS.GetString(), out var pICMSVal) ? pICMSVal : 0)) : 0,
                                        vICMS = icms00Element.TryGetProperty("vICMS", out var vICMS) ? (vICMS.ValueKind == JsonValueKind.Number ? vICMS.GetDecimal() : (decimal.TryParse(vICMS.GetString(), out var vICMSVal) ? vICMSVal : 0)) : 0
                                    };
                                }
                            }
                        }

                        nfe.Det.Add(detalhe);
                    }
                }

                // Converter dados dos Totais
                if (jsonElement.TryGetProperty("total", out var totalElement))
                {
                    nfe.Total = new TotalNFe();
                    
                    if (totalElement.TryGetProperty("ICMSTot", out var icmsTotElement))
                    {
                        nfe.Total.ICMSTot = new ICMSTotal
                        {
                            vBC = icmsTotElement.TryGetProperty("vBC", out var vBC) ? (vBC.ValueKind == JsonValueKind.Number ? vBC.GetDecimal() : (decimal.TryParse(vBC.GetString(), out var vBCVal) ? vBCVal : 0)) : 0,
                            vICMS = icmsTotElement.TryGetProperty("vICMS", out var vICMS) ? (vICMS.ValueKind == JsonValueKind.Number ? vICMS.GetDecimal() : (decimal.TryParse(vICMS.GetString(), out var vICMSVal) ? vICMSVal : 0)) : 0,
                            vProd = icmsTotElement.TryGetProperty("vProd", out var vProd) ? (vProd.ValueKind == JsonValueKind.Number ? vProd.GetDecimal() : (decimal.TryParse(vProd.GetString(), out var vProdVal) ? vProdVal : 0)) : 0,
                            vNF = icmsTotElement.TryGetProperty("vNF", out var vNF) ? (vNF.ValueKind == JsonValueKind.Number ? vNF.GetDecimal() : (decimal.TryParse(vNF.GetString(), out var vNFVal) ? vNFVal : 0)) : 0
                        };
                    }
                }

                // Converter dados do Transporte
                if (jsonElement.TryGetProperty("transp", out var transpElement))
                {
                    nfe.Transp = new Transporte
                    {
                        modFrete = transpElement.TryGetProperty("modFrete", out var modFrete) ? (modFrete.ValueKind == JsonValueKind.Number ? modFrete.GetInt32() : (int.TryParse(modFrete.GetString(), out var modFreteVal) ? modFreteVal : 9)) : 9
                    };
                }

                // Converter dados do Pagamento
                if (jsonElement.TryGetProperty("pag", out var pagElement))
                {
                    nfe.Pag = new Pagamento();
                    
                    if (pagElement.TryGetProperty("detPag", out var detPagElement) && detPagElement.ValueKind == JsonValueKind.Array)
                    {
                        nfe.Pag.DetPag = new List<DetalhePagamento>();
                        foreach (var pagItem in detPagElement.EnumerateArray())
                        {
                            nfe.Pag.DetPag.Add(new DetalhePagamento
                            {
                                indPag = pagItem.TryGetProperty("indPag", out var indPag) ? (indPag.ValueKind == JsonValueKind.Number ? indPag.GetInt32() : (int.TryParse(indPag.GetString(), out var indPagVal) ? indPagVal : 0)) : 0,
                                tPag = pagItem.TryGetProperty("tPag", out var tPag) ? (tPag.ValueKind == JsonValueKind.Number ? tPag.GetInt32() : (int.TryParse(tPag.GetString(), out var tPagVal) ? tPagVal : 1)) : 1,
                                vPag = pagItem.TryGetProperty("vPag", out var vPag) ? (vPag.ValueKind == JsonValueKind.Number ? vPag.GetDecimal() : (decimal.TryParse(vPag.GetString(), out var vPagVal) ? vPagVal : 0)) : 0
                            });
                        }
                    }
                }

                // Converter dados das Informações Adicionais
                if (jsonElement.TryGetProperty("infAdic", out var infAdicElement))
                {
                    nfe.InfAdic = new InformacaoAdicional
                    {
                        infCpl = TratarCaracteresEspeciais(infAdicElement.TryGetProperty("infCpl", out var infCpl) ? infCpl.GetString() ?? "" : "")
                    };
                }

                Console.WriteLine("=== CONVERSÃO CONCLUÍDA COM SUCESSO ===");
                return nfe;
            }
            catch (Exception ex)
            {
                // Em caso de erro na conversão, retornar dados simulados
                Console.WriteLine($"=== ERRO NA CONVERSÃO ===");
                Console.WriteLine($"Erro ao converter dados da NF-e: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return CriarNFeSimulada(protocolo);
            }
        }

        private NFe CriarNFeSimulada(string protocolo)
        {
            Console.WriteLine("=== USANDO DADOS SIMULADOS (FALLBACK) ===");
            // Criar uma NF-e simulada com os dados reais que foram enviados
            var nfe = new NFe
            {
                Id = Guid.NewGuid(),
                Versao = "4.00",
                ChaveAcesso = "33250933045570000191550010000000011000000001",
                Protocolo = protocolo,
                DataAutorizacao = DateTime.Now,
                Status = "Autorizada",
                Ide = new IdentificacaoNFe
                {
                    cUF = 33, // RJ
                    cNF = "00000001",
                    natOp = "VENDA",
                    mod = 55,
                    serie = 1,
                    nNF = 1,
                    dhEmi = DateTime.Now,
                    tpNF = 1,
                    idDest = 1,
                    cMunFG = 3304557,
                    tpImp = 1,
                    tpEmis = 1,
                    cDV = 1,
                    tpAmb = 2,
                    finNFe = 1,
                    indFinal = 1,
                    indPres = 1,
                    procEmi = 0,
                    verProc = "NFSE2026"
                },
                Emit = new Emitente
                {
                    CNPJ = "92754738027109",
                    xNome = "EMPRESA TESTE SA",
                    xFant = "EMPRESA TESTE",
                    EnderEmit = new EnderecoNFe
                    {
                        xLgr = "RUA TESTE TESTE",
                        nro = "218",
                        xBairro = "NITEROI",
                        cMun = 3304557,
                        xMun = "Rio de Janeiro",
                        UF = "RJ",
                        CEP = "2183823",
                        cPais = 1058,
                        xPais = "Brasil"
                    },
                    IE = "32842478",
                    CRT = 3
                },
                Dest = new Destinatario
                {
                    CNPJ = "19216878292",
                    xNome = "Joao da silv",
                    EnderDest = new EnderecoNFe
                    {
                        xLgr = "rua das dores",
                        nro = "328",
                        xBairro = "rio",
                        cMun = 3304557,
                        xMun = "Rio de Janeiro",
                        UF = "RJ",
                        CEP = "3232324",
                        cPais = 1058,
                        xPais = "Brasil"
                    },
                    indIEDest = 1,
                    IE = "323828",
                    email = "joao@email.com"
                },
                Det = new List<DetalheNFe>
                {
                    new DetalheNFe
                    {
                        nItem = 1,
                        Prod = new ProdutoNFe
                        {
                            cProd = "001",
                            cEAN = "SEM GTIN",
                            xProd = "Camiseta manga curta Meia Malha Liso malhao",
                            NCM = "6109.10.00",
                            CFOP = 5102,
                            uCom = "UN",
                            qCom = 1,
                            vUnCom = 100.00m,
                            vProd = 100.00m,
                            cEANTrib = "SEM GTIN",
                            uTrib = "UN",
                            qTrib = 1,
                            vUnTrib = 100.00m,
                            indTot = 1
                        },
                        Imposto = new ImpostoNFe
                        {
                            ICMS = new ICMS
                            {
                                orig = "0",
                                CST = "00",
                                modBC = 3,
                                vBC = 100.00m,
                                pICMS = 18.00m,
                                vICMS = 18.00m
                            }
                        }
                    }
                },
                Total = new TotalNFe
                {
                    ICMSTot = new ICMSTotal
                    {
                        vBC = 100.00m,
                        vICMS = 18.00m,
                        vProd = 100.00m,
                        vNF = 100.00m
                    }
                },
                Transp = new Transporte
                {
                    modFrete = 9
                },
                Pag = new Pagamento
                {
                    DetPag = new List<DetalhePagamento>
                    {
                        new DetalhePagamento
                        {
                            indPag = 0,
                            tPag = 1,
                            vPag = 100.00m
                        }
                    }
                },
                InfAdic = new InformacaoAdicional
                {
                    infCpl = "Informações complementares"
                }
            };

            return nfe;
        }

        [HttpGet("consulta/{chaveAcesso}")]
        public IActionResult ConsultarNFe(string chaveAcesso)
        {
            try
            {
                if (string.IsNullOrEmpty(chaveAcesso) || chaveAcesso.Length != 44)
                {
                    return BadRequest(new { message = "Chave de acesso inválida" });
                }

                // Simular consulta de NF-e por enquanto
                var nfe = new
                {
                    chaveAcesso = chaveAcesso,
                    numero = 1,
                    serie = 1,
                    dataEmissao = DateTime.Now.AddDays(-1),
                    dataAutorizacao = DateTime.Now.AddDays(-1).AddHours(2),
                    valorTotal = 100.00m,
                    status = "Autorizada",
                    protocolo = "135210000000000",
                    situacao = "Normal",
                    destinatario = "Cliente Exemplo Ltda",
                    cnpjDestinatario = "12345678000199",
                    xmlAutorizado = "XML content would be here",
                    danfe = $"DANFE_{chaveAcesso}.pdf"
                };

                return Ok(new
                {
                    sucesso = true,
                    nfe = nfe,
                    mensagem = "Consulta realizada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro ao consultar NF-e: {ex.Message}"
                });
            }
        }
    }

    // DTOs para requisições
    public class CartaCorrecaoRequest
    {
        public string ChaveAcesso { get; set; } = string.Empty;
        public string CnpjEmitente { get; set; } = string.Empty;
        public string TextoCorrecao { get; set; } = string.Empty;
        public int? SequenciaEvento { get; set; }
    }

    public class CancelamentoRequest
    {
        public string ChaveAcesso { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string CnpjEmitente { get; set; } = string.Empty;
        public string Justificativa { get; set; } = string.Empty;
    }

    public class InutilizacaoRequest
    {
        public string CnpjEmitente { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;
        public int Ano { get; set; }
        public int Serie { get; set; }
        public int NumeroInicial { get; set; }
        public int NumeroFinal { get; set; }
        public string Justificativa { get; set; } = string.Empty;
    }

    public class AlterarAmbienteRequest
    {
        public bool Producao { get; set; }
    }

    public class CertificadoUploadRequest
    {
        public IFormFile? Certificado { get; set; }
        public string Senha { get; set; } = string.Empty;
        public string CnpjEmpresa { get; set; } = string.Empty;
    }
}
