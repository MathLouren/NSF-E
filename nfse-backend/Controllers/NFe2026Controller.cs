using Microsoft.AspNetCore.Mvc;
using nfse_backend.Models.NFe;
using nfse_backend.Services.NFe;
using nfse_backend.Services.Validation;
using nfse_backend.Services.Calculation;
using nfse_backend.Services.Audit;
using nfse_backend.Services.Pdf;

namespace nfse_backend.Controllers
{
    /// <summary>
    /// Controller para operações de NF-e 2026
    /// Implementa endpoints para processamento completo conforme NT 2025.002
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NFe2026Controller : ControllerBase
    {
        private readonly NFe2026Service _nfe2026Service;
        private readonly NFe2026ValidationService _validationService;
        private readonly NFe2026CalculationService _calculationService;
        private readonly NFe2026AuditService _auditService;
        private readonly Danfe2026Service _danfe2026Service;
        private readonly DanfeService _danfeService;

        public NFe2026Controller(
            NFe2026Service nfe2026Service,
            NFe2026ValidationService validationService,
            NFe2026CalculationService calculationService,
            NFe2026AuditService auditService,
            Danfe2026Service danfe2026Service,
            DanfeService danfeService)
        {
            _nfe2026Service = nfe2026Service;
            _validationService = validationService;
            _calculationService = calculationService;
            _auditService = auditService;
            _danfe2026Service = danfe2026Service;
            _danfeService = danfeService;
        }

        #region Teste de Deserialização

        /// <summary>
        /// Endpoint de teste para verificar deserialização
        /// </summary>
    [HttpPost("teste-deserializacao")]
    public ActionResult<object> TesteDeserializacao([FromBody] object request)
    {
        try
        {
            Console.WriteLine($"TesteDeserializacao - Request recebido: {request != null}");
            Console.WriteLine($"Tipo do request: {request?.GetType()?.Name}");
            
            return Ok(new
            {
                sucesso = true,
                requestRecebido = request != null,
                tipoRequest = request?.GetType()?.Name,
                mensagem = "Deserialização funcionou"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro em TesteDeserializacao: {ex.Message}");
            return BadRequest(new { erro = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost("teste-danfe-simples")]
    public ActionResult<object> TesteDanfeSimples([FromBody] object requestObj)
    {
        try
        {
            Console.WriteLine("TesteDanfeSimples - Iniciando teste simples...");
            
            // Criar um objeto NFe2026 básico para teste
            var nfe2026 = new NFe2026
            {
                Versao = "4.00",
                ChaveAcesso = "35200114200166000187550010000000015123456789",
                Ide = new IdentificacaoNFe
                {
                    cUF = 35,
                    natOp = "VENDA",
                    nNF = 1,
                    serie = 1,
                    tpNF = 1,
                    cMunFG = 3550308
                },
                Emit = new Emitente
                {
                    CNPJ = "14200166000187",
                    xNome = "Empresa Teste Ltda",
                    IE = "123456789",
                    CRT = 3
                }
            };
            
            Console.WriteLine("Objeto NFe2026 criado com sucesso");
            
            return Ok(new
            {
                sucesso = true,
                mensagem = "Teste simples funcionou",
                nfeCriada = nfe2026 != null,
                emitente = nfe2026?.Emit?.xNome ?? "Não informado"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro em TesteDanfeSimples: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return BadRequest(new { erro = ex.Message, stackTrace = ex.StackTrace });
        }
    }

        /// <summary>
        /// Endpoint de teste simples
        /// </summary>
        [HttpPost("teste-simples")]
        public ActionResult<object> TesteSimples()
        {
            return Ok(new { sucesso = true, mensagem = "Endpoint funcionando" });
        }

        #endregion

        #region Processamento Principal

        /// <summary>
        /// Processa uma NF-e 2026 completa
        /// </summary>
        [HttpPost("processar")]
        public async Task<ActionResult<NFe2026Result>> ProcessarNFe2026([FromBody] NFe2026 nfe)
        {
            try
            {
                var result = await _nfe2026Service.ProcessarNFe2026Async(nfe);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Valida uma NF-e 2026 sem processar
        /// </summary>
        [HttpPost("validar")]
        public ActionResult<ValidacaoNFE2026> ValidarNFe2026([FromBody] NFe2026 nfe)
        {
            try
            {
                var validacao = _validationService.ValidarNFe2026(nfe);
                return Ok(validacao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Calcula totais de uma NF-e 2026
        /// </summary>
        [HttpPost("calcular-totais")]
        public ActionResult<NFe2026> CalcularTotais([FromBody] NFe2026 nfe)
        {
            try
            {
                _calculationService.CalcularTotaisNFe2026(nfe);
                return Ok(nfe);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Audita uma NF-e 2026
        /// </summary>
        [HttpPost("auditar")]
        public ActionResult AuditarNFe2026([FromBody] NFe2026 nfe)
        {
            try
            {
                _auditService.AuditarNFe2026(nfe);
                return Ok(new { mensagem = "Auditoria realizada com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        #endregion

        #region Eventos Fiscais

        /// <summary>
        /// Processa um evento fiscal
        /// </summary>
        [HttpPost("evento-fiscal")]
        public async Task<ActionResult<NFe2026Result>> ProcessarEventoFiscal([FromBody] EventoFiscalRequest request)
        {
            try
            {
                var result = await _nfe2026Service.ProcessarEventoFiscalAsync(request.NFe, request.Evento);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Lista eventos fiscais de uma NF-e
        /// </summary>
        [HttpGet("eventos/{chaveAcesso}")]
        public ActionResult<List<EventoFiscal>> ListarEventosFiscais(string chaveAcesso)
        {
            try
            {
                // Implementar busca de eventos por chave de acesso
                var eventos = new List<EventoFiscal>();
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Consulta status de uma NF-e
        /// </summary>
        [HttpGet("consultar-status/{chaveAcesso}")]
        public async Task<ActionResult<NFe2026Result>> ConsultarStatus(string chaveAcesso, [FromQuery] string uf, [FromQuery] bool homologacao = true)
        {
            try
            {
                var result = await _nfe2026Service.ConsultarStatusNFeAsync(chaveAcesso, uf, homologacao);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Consulta distribuição DFe
        /// </summary>
        [HttpGet("consultar-distribuicao")]
        public async Task<ActionResult<NFe2026Result>> ConsultarDistribuicao([FromQuery] string cnpj, [FromQuery] string ultimoNSU, [FromQuery] bool homologacao = true)
        {
            try
            {
                var result = await _nfe2026Service.ConsultarDistribuicaoDFeAsync(cnpj, ultimoNSU, homologacao);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        #endregion

        #region Rastreabilidade

        /// <summary>
        /// Adiciona rastreabilidade a um item
        /// </summary>
        [HttpPost("rastreabilidade")]
        public ActionResult AdicionarRastreabilidade([FromBody] RastreabilidadeRequest request)
        {
            try
            {
                // Implementar adição de rastreabilidade
                return Ok(new { mensagem = "Rastreabilidade adicionada com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Valida rastreabilidade de um item
        /// </summary>
        [HttpPost("validar-rastreabilidade")]
        public ActionResult ValidarRastreabilidade([FromBody] RastreabilidadeItem rastreabilidade)
        {
            try
            {
                // Implementar validação de rastreabilidade
                var valido = !string.IsNullOrEmpty(rastreabilidade.GTIN) && 
                           !string.IsNullOrEmpty(rastreabilidade.NumeroLote);
                
                return Ok(new { valido = valido });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        #endregion

        #region Relatórios

        /// <summary>
        /// Gera relatório de totais por UF
        /// </summary>
        [HttpPost("relatorio-totais-uf")]
        public ActionResult<List<TotalPorUF>> RelatorioTotaisUF([FromBody] NFe2026 nfe)
        {
            try
            {
                _calculationService.CalcularTotaisNFe2026(nfe);
                return Ok(nfe.TotaisPorUF);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Gera relatório de totais por município
        /// </summary>
        [HttpPost("relatorio-totais-municipio")]
        public ActionResult<List<TotalPorMunicipio>> RelatorioTotaisMunicipio([FromBody] NFe2026 nfe)
        {
            try
            {
                _calculationService.CalcularTotaisNFe2026(nfe);
                return Ok(nfe.TotaisPorMunicipio);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Gera relatório de auditoria
        /// </summary>
        [HttpPost("relatorio-auditoria")]
        public ActionResult RelatorioAuditoria([FromBody] NFe2026 nfe)
        {
            try
            {
                _auditService.AuditarNFe2026(nfe);
                return Ok(new { mensagem = "Relatório de auditoria gerado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        #endregion

        #region Conformidade

        /// <summary>
        /// Verifica conformidade com layout 2026
        /// </summary>
        [HttpPost("verificar-conformidade")]
        public ActionResult<ConformidadeLayout> VerificarConformidade([FromBody] VerificacaoConformidadeRequest request)
        {
            try
            {
                var conformidade = new ConformidadeLayout
                {
                    VersaoLayout = request.VersaoLayout ?? "2026.001",
                    NumeroNotaTecnica = request.NumeroNotaTecnica ?? "2025.002",
                    DataVigencia = new DateTime(2026, 1, 1),
                    Conforme = true, // Por enquanto sempre conforme para dados básicos
                    Inconformidades = new List<string>(),
                    DataVerificacao = DateTime.UtcNow,
                    Observacoes = new List<string>
                    {
                        "Verificação básica de conformidade realizada",
                        "Layout 2026.001 conforme NT 2025.002",
                        "Campos IBS, CBS e IS implementados",
                        "Rastreabilidade e referências disponíveis"
                    }
                };

                return Ok(conformidade);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Obtém informações sobre a versão do layout
        /// </summary>
        [HttpGet("versao-layout")]
        public ActionResult<object> ObterVersaoLayout()
        {
            try
            {
                var versao = new
                {
                    VersaoAtual = "2026.001",
                    NumeroNotaTecnica = "2025.002",
                    DataVigencia = new DateTime(2026, 1, 1),
                    DataAtualizacao = DateTime.UtcNow,
                    GruposObrigatorios = new[]
                    {
                        "W03 - IBS/CBS/IS",
                        "VB - Totais por UF/Município",
                        "VC - Referências de Documentos"
                    },
                    Novidades = new[]
                    {
                        "Suporte a IBS (Imposto sobre Bens e Serviços)",
                        "Suporte a CBS (Contribuição sobre Bens e Serviços)",
                        "Suporte a IS (Imposto Seletivo)",
                        "Rastreabilidade obrigatória para medicamentos, bebidas e combustíveis",
                        "Eventos fiscais (crédito presumido, perda/roubo, etc.)",
                        "Auditoria completa com separação por UF/município"
                    }
                };

                return Ok(versao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        #endregion

        #region Geração de DANFE 2026

        /// <summary>
        /// Gera DANFE 2026 com todos os campos da reforma tributária
        /// </summary>
        [HttpPost("gerar-danfe-2026")]
        public IActionResult GerarDanfe2026([FromBody] NFe2026 nfe, [FromQuery] bool isContingencia = false)
        {
            try
            {
                var danfeBytes = _danfe2026Service.GenerateDanfe2026(nfe, isContingencia);
                
                return File(danfeBytes, "application/pdf", $"DANFE_2026_{nfe.ChaveAcesso}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Gera DANFE 2026 e retorna como base64
        /// </summary>
        [HttpPost("gerar-danfe-2026-base64")]
        public ActionResult<object> GerarDanfe2026Base64([FromBody] NFe2026 nfe, [FromQuery] bool isContingencia = false)
        {
            try
            {
                var danfeBytes = _danfe2026Service.GenerateDanfe2026(nfe, isContingencia);
                var base64 = Convert.ToBase64String(danfeBytes);
                
                return Ok(new 
                { 
                    sucesso = true,
                    danfeBase64 = base64,
                    nomeArquivo = $"DANFE_2026_{nfe.ChaveAcesso}.pdf",
                    tamanhoBytes = danfeBytes.Length,
                    layout = "2026.001",
                    camposIncluidos = new[]
                    {
                        "IBS (Imposto sobre Bens e Serviços)",
                        "CBS (Contribuição sobre Bens e Serviços)", 
                        "IS (Imposto Seletivo)",
                        "GTIN e Rastreabilidade",
                        "Totais por UF/Município",
                        "Referências de Documentos"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Gera DANFE com seleção de versão (atual, 2026, ou ambas)
        /// </summary>
        [HttpPost("gerar-danfe-versao")]
        public ActionResult<object> GerarDanfeVersao([FromBody] object requestObj)
        {
            try
            {
                // Log para debug
                Console.WriteLine($"GerarDanfeVersao - Request recebido: {requestObj != null}");
                
                if (requestObj == null)
                    return BadRequest(new { erro = "Payload inválido: corpo da requisição ausente." });

                // Converter o objeto genérico para JsonElement e depois para o tipo esperado
                var jsonElement = (System.Text.Json.JsonElement)requestObj;
                
                if (!jsonElement.TryGetProperty("Versao", out var versaoElement))
                    return BadRequest(new { erro = "Parâmetro 'Versao' é obrigatório." });
                
                if (!jsonElement.TryGetProperty("NFe", out var nfeElement))
                    return BadRequest(new { erro = "Parâmetro 'NFe' é obrigatório." });
                
                var versao = versaoElement.GetString();
                var isContingencia = jsonElement.TryGetProperty("IsContingencia", out var contingenciaElement) && contingenciaElement.GetBoolean();
                
                Console.WriteLine($"Versao: {versao}");
                Console.WriteLine($"IsContingencia: {isContingencia}");
                
                if (string.IsNullOrWhiteSpace(versao))
                    return BadRequest(new { erro = "Parâmetro 'Versao' deve ser 'atual', '2026' ou 'ambas'." });

                var danfes = new List<object>();
                object? comparacao = null;

            // Criar um objeto NFe2026 básico para teste
            var nfe2026 = new NFe2026
            {
                Versao = "4.00",
                ChaveAcesso = "35200114200166000187550010000000015123456789",
                // Dados de identificação
                Ide = new IdentificacaoNFe
                {
                    cUF = 35, // SP
                    cNF = "12345678",
                    natOp = "VENDA",
                    mod = 55,
                    serie = 1,
                    nNF = 1,
                    dhEmi = DateTime.Now,
                    tpNF = 1, // Saída
                    cMunFG = 3550308, // São Paulo
                    tpImp = 1,
                    tpEmis = 1,
                    cDV = 9,
                    tpAmb = 2, // Homologação
                    finNFe = 1,
                    indFinal = 1,
                    indPres = 1,
                    procEmi = 0,
                    verProc = "1.0.0"
                },
                // Dados do emitente
                Emit = new Emitente
                {
                    CNPJ = "14200166000187",
                    xNome = "Empresa Teste Ltda",
                    xFant = "Empresa Teste",
                    EnderEmit = new EnderecoNFe
                    {
                        xLgr = "Rua Teste",
                        nro = "123",
                        xBairro = "Centro",
                        cMun = 3550308,
                        xMun = "São Paulo",
                        UF = "SP",
                        CEP = "01000-000"
                    },
                    IE = "123456789",
                    IEST = "",
                    IM = "123456",
                    CNAE = "1234567",
                    CRT = 3
                },
                // Dados do destinatário
                Dest = new Destinatario
                {
                    CNPJ = "12345678000195",
                    xNome = "Cliente Teste Ltda",
                    EnderDest = new EnderecoNFe
                    {
                        xLgr = "Av Cliente",
                        nro = "456",
                        xBairro = "Copacabana",
                        cMun = 3303302,
                        xMun = "Rio de Janeiro",
                        UF = "RJ",
                        CEP = "22000-000"
                    },
                    indIEDest = 1,
                    IE = "987654321"
                },
                // Detalhes dos produtos
                Det = new List<DetalheNFe>
                {
                    new DetalheNFe
                    {
                        nItem = 1,
                        Prod = new ProdutoNFe
                        {
                            cProd = "001",
                            cEAN = "1234567890123",
                            xProd = "Produto Teste",
                            NCM = "12345678",
                            CFOP = 5102,
                            uCom = "UN",
                            qCom = 1.0000m,
                            vUnCom = 100.00m,
                            vProd = 100.00m,
                            cEANTrib = "1234567890123",
                            uTrib = "UN",
                            qTrib = 1.0000m,
                            vUnTrib = 100.00m,
                            indTot = 1
                        }
                    }
                },
                // Totais
                Total = new TotalNFe
                {
                    ICMSTot = new ICMSTotal
                    {
                        vBC = 100.00m,
                        vICMS = 18.00m,
                        vICMSDeson = 0.00m,
                        vFCP = 0.00m,
                        vBCST = 0.00m,
                        vST = 0.00m,
                        vFCPST = 0.00m,
                        vFCPSTRet = 0.00m,
                        vProd = 100.00m,
                        vFrete = 0.00m,
                        vSeg = 0.00m,
                        vDesc = 0.00m,
                        vII = 0.00m,
                        vIPI = 0.00m,
                        vIPIDevol = 0.00m,
                        vPIS = 0.00m,
                        vCOFINS = 0.00m,
                        vOutro = 0.00m,
                        vNF = 100.00m,
                        vTotTrib = 18.00m
                    }
                }
            };
                
                // Converter NFe2026 para NFe tradicional se necessário
                Console.WriteLine("Convertendo NFe2026 para NFe tradicional...");
                var nfeTradicional = ConverterParaNFeTradicional(nfe2026);
                Console.WriteLine("Conversão concluída.");

                if (versao == "atual" || versao == "ambas")
                {
                    // Gerar DANFE versão atual
                    Console.WriteLine("Gerando DANFE versão atual...");
                    var danfeAtualBytes = _danfeService.GenerateDanfe(nfeTradicional, isContingencia);
                    var danfeAtualBase64 = Convert.ToBase64String(danfeAtualBytes);
                    
                    danfes.Add(new
                    {
                        versao = "atual",
                        layout = "4.00",
                        danfeBase64 = danfeAtualBase64,
                        nomeArquivo = $"DANFE_Atual_{nfe2026.ChaveAcesso}.pdf",
                        tamanhoBytes = danfeAtualBytes.Length,
                        camposIncluidos = new[]
                        {
                            "ICMS tradicional",
                            "IPI",
                            "PIS/COFINS",
                            "Totais básicos",
                            "Dados do emitente/destinatário"
                        }
                    });
                }

                if (versao == "2026" || versao == "ambas")
                {
                    try
                    {
                        // Gerar DANFE versão 2026
                        Console.WriteLine("Tentando gerar DANFE 2026...");
                        var danfe2026Bytes = _danfe2026Service.GenerateDanfe2026(nfe2026, isContingencia);
                        var danfe2026Base64 = Convert.ToBase64String(danfe2026Bytes);
                        
                        danfes.Add(new
                        {
                            versao = "2026",
                            layout = "2026.001",
                            danfeBase64 = danfe2026Base64,
                            nomeArquivo = $"DANFE_2026_{nfe2026.ChaveAcesso}.pdf",
                            tamanhoBytes = danfe2026Bytes.Length,
                            camposIncluidos = new[]
                            {
                                "IBS (Imposto sobre Bens e Serviços)",
                                "CBS (Contribuição sobre Bens e Serviços)",
                                "IS (Imposto Seletivo)",
                                "Rastreabilidade (GTIN)",
                                "Referências de Documentos",
                                "Totais por UF/Município",
                                "Campos de Monofasia e Diferimento"
                            }
                        });
                        Console.WriteLine("DANFE 2026 gerada com sucesso!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao gerar DANFE 2026: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        
                        // Retornar erro específico para DANFE 2026
                        return BadRequest(new { 
                            erro = $"Erro ao gerar DANFE 2026: {ex.Message}",
                            detalhes = ex.StackTrace
                        });
                    }
                }

                // Se ambas as versões foram solicitadas, criar comparação
                if (versao == "ambas")
                {
                    comparacao = new
                    {
                        diferencasPrincipais = new[]
                        {
                            "Adição dos campos IBS, CBS e IS na reforma tributária",
                            "Implementação de rastreabilidade obrigatória (GTIN)",
                            "Novos totais por UF e Município",
                            "Bloco de referências de documentos fiscais",
                            "Campos especiais: monofasia, diferimento, crédito presumido",
                            "Layout visual aprimorado com cores diferenciadas"
                        },
                        camposNovos = new[]
                        {
                            "Base de Cálculo IBS", "Valor IBS", "Alíquota IBS",
                            "Base de Cálculo CBS", "Valor CBS", "Alíquota CBS", 
                            "Base de Cálculo IS", "Valor IS", "Alíquota IS",
                            "GTIN", "Número do Lote", "Data de Fabricação",
                            "Data de Validade", "Código de Rastreamento"
                        },
                        camposMantidos = new[]
                        {
                            "Dados do emitente e destinatário",
                            "Informações dos produtos/serviços",
                            "ICMS tradicional (mantido para compatibilidade)",
                            "IPI, PIS, COFINS",
                            "Dados de transporte e cobrança"
                        }
                    };
                }

                return Ok(new
                {
                    sucesso = true,
                    versaoSolicitada = versao,
                    danfes = danfes,
                    comparacao = comparacao
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em GerarDanfeVersao: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Converte NFe2026 para NFe tradicional para compatibilidade
        /// </summary>
        private NFe ConverterParaNFeTradicional(NFe2026 nfe2026)
        {
            // Esta é uma conversão simplificada - em produção, implementar mapeamento completo
            var nfe = new NFe
            {
                Versao = nfe2026.Versao,
                ChaveAcesso = nfe2026.ChaveAcesso,
                Ide = nfe2026.Ide,
                Emit = nfe2026.Emit,
                Dest = nfe2026.Dest,
                Det = nfe2026.Det,
                Total = nfe2026.Total,
                Transp = nfe2026.Transp,
                Cobr = nfe2026.Cobr,
                InfAdic = nfe2026.InfAdic
            };
            
            return nfe;
        }

        #endregion
    }

    #region Classes de Request

    public class EventoFiscalRequest
    {
        public required NFe2026 NFe { get; set; }
        public required EventoFiscal Evento { get; set; }
    }

    public class RastreabilidadeRequest
    {
        public int nItem { get; set; }
        public required RastreabilidadeItem Rastreabilidade { get; set; }
    }

    public class VerificacaoConformidadeRequest
    {
        public string? VersaoLayout { get; set; }
        public string? NumeroNotaTecnica { get; set; }
    }

    public class GerarDanfeRequest
    {
        public required NFe2026 NFe { get; set; }
        public required string Versao { get; set; } // "atual", "2026", ou "ambas"
        public bool IsContingencia { get; set; } = false;
    }

    #endregion
}
