using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using nfse_backend.Models.NFe;
using nfse_backend.Models.Contingencia;
using nfse_backend.Services.Certificado;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Armazenamento;

namespace nfse_backend.Services.NFe
{
    public enum TipoContingencia
    {
        Normal = 1,        // Emissão normal
        FSDA = 2,         // Formulário de Segurança para Impressão de Documento Auxiliar
        SCAN = 3,         // Sistema de Contingência do Ambiente Nacional (descontinuado)
        DPEC = 4,         // Declaração Prévia de Emissão em Contingência (descontinuado)
        FSDA_INSEGURO = 5, // FS-DA sem impressão do DANFE (inseguro)
        SVCAN = 6,        // SVC-AN - Sistema de Contingência Virtual dos Correios
        SVCRS = 7,        // SVC-RS - Sistema de Contingência Virtual do RS
        EPEC = 8          // EPEC - Evento Prévio de Emissão em Contingência
    }

    public class ContingenciaNFeService
    {
        private readonly ILogger<ContingenciaNFeService> _logger;
        private readonly CertificadoDigitalService _certificadoService;
        private readonly SefazWebServiceClient _webServiceClient;
        private readonly ArmazenamentoSeguroService _armazenamentoService;
        private readonly Dictionary<string, DateTime> _ultimaVerificacaoSefaz;

        public ContingenciaNFeService(
            ILogger<ContingenciaNFeService> logger,
            CertificadoDigitalService certificadoService,
            SefazWebServiceClient webServiceClient,
            ArmazenamentoSeguroService armazenamentoService)
        {
            _logger = logger;
            _certificadoService = certificadoService;
            _webServiceClient = webServiceClient;
            _armazenamentoService = armazenamentoService;
            _ultimaVerificacaoSefaz = new Dictionary<string, DateTime>();
        }

        public async Task<bool> VerificarDisponibilidadeSefaz(string uf, bool homologacao = true)
        {
            try
            {
                var chaveVerificacao = $"{uf}_{(homologacao ? "HOM" : "PROD")}";
                
                // Verificar cache (não verificar mais que a cada 5 minutos)
                if (_ultimaVerificacaoSefaz.ContainsKey(chaveVerificacao) &&
                    DateTime.Now.Subtract(_ultimaVerificacaoSefaz[chaveVerificacao]).TotalMinutes < 5)
                {
                    return true; // Assume que está disponível se verificou recentemente
                }

                _logger.LogInformation($"Verificando disponibilidade da SEFAZ {uf} ({(homologacao ? "Homologação" : "Produção")})");
                
                var resposta = await _webServiceClient.ConsultarStatusServico(uf, homologacao);
                var disponivel = AnaliserRespostaStatusServico(resposta);
                
                _ultimaVerificacaoSefaz[chaveVerificacao] = DateTime.Now;
                
                if (!disponivel)
                {
                    _logger.LogWarning($"SEFAZ {uf} indisponível. Resposta: {resposta}");
                }

                return disponivel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar disponibilidade da SEFAZ {uf}");
                return false;
            }
        }

        public async Task<(bool sucesso, string chaveAcesso, string protocolo)> EmitirEmContingencia(
            nfse_backend.Models.NFe.NFe nfe, 
            TipoContingencia tipoContingencia = TipoContingencia.EPEC)
        {
            try
            {
                _logger.LogWarning($"Iniciando emissão em contingência {tipoContingencia} para NF-e");

                // Alterar tipo de emissão na NFe
                nfe.Ide.tpEmis = (int)tipoContingencia;
                nfe.Ide.dhCont = DateTime.Now; // Data/hora da contingência
                nfe.Ide.xJust = "Indisponibilidade do sistema da SEFAZ"; // Justificativa

                switch (tipoContingencia)
                {
                    case TipoContingencia.EPEC:
                        return await EmitirEPEC(nfe);
                    
                    case TipoContingencia.FSDA:
                        return await EmitirFSDA(nfe);
                    
                    case TipoContingencia.SVCAN:
                        return await EmitirSVCAN(nfe);
                    
                    default:
                        throw new NotSupportedException($"Tipo de contingência {tipoContingencia} não suportado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro na emissão em contingência {tipoContingencia}");
                return (false, "", "");
            }
        }

        private async Task<(bool sucesso, string chaveAcesso, string protocolo)> EmitirEPEC(nfse_backend.Models.NFe.NFe nfe)
        {
            try
            {
                _logger.LogInformation("Emitindo EPEC - Evento Prévio de Emissão em Contingência");

                // 1. Gerar chave de acesso
                var chaveAcesso = GerarChaveAcesso(nfe);
                nfe.ChaveAcesso = chaveAcesso;

                // 2. Criar evento EPEC
                var xmlEvento = CriarEventoEPEC(nfe, chaveAcesso);

                // 3. Assinar evento
                var xmlEventoAssinado = _certificadoService.AssinarXml(xmlEvento, "infEvento");

                // 4. Enviar evento para SEFAZ
                var respostaEvento = await _webServiceClient.EnviarEvento(
                    xmlEventoAssinado, 
                    nfe.Emit.EnderEmit.UF, 
                    nfe.Ide.tpAmb == 2
                );

                // 5. Processar resposta
                var (sucessoEvento, protocoloEvento) = ProcessarRespostaEvento(respostaEvento);

                if (sucessoEvento)
                {
                    // 6. Salvar XML em contingência
                    await _armazenamentoService.SalvarXmlContingencia(
                        xmlEventoAssinado, 
                        chaveAcesso, 
                        nfe.Emit.CNPJ
                    );

                    _logger.LogInformation($"EPEC autorizado com protocolo: {protocoloEvento}");
                    return (true, chaveAcesso, protocoloEvento);
                }
                else
                {
                    _logger.LogError($"EPEC rejeitado: {respostaEvento}");
                    return (false, chaveAcesso, "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na emissão de EPEC");
                return (false, "", "");
            }
        }

        private async Task<(bool sucesso, string chaveAcesso, string protocolo)> EmitirFSDA(nfse_backend.Models.NFe.NFe nfe)
        {
            try
            {
                _logger.LogInformation("Emitindo em contingência FS-DA");

                // 1. Gerar chave de acesso
                var chaveAcesso = GerarChaveAcesso(nfe);
                nfe.ChaveAcesso = chaveAcesso;

                // 2. Para FS-DA, a NF-e é gerada localmente e será enviada quando a SEFAZ voltar
                // 3. Salvar XML em contingência
                var xmlNfe = GerarXmlNFe(nfe);
                var xmlAssinado = _certificadoService.AssinarXml(xmlNfe);
                
                await _armazenamentoService.SalvarXmlContingencia(
                    xmlAssinado, 
                    chaveAcesso, 
                    nfe.Emit.CNPJ
                );

                _logger.LogInformation($"NF-e salva em contingência FS-DA: {chaveAcesso}");
                
                // Protocolo fictício para contingência
                var protocoloContingencia = $"FSDA{DateTime.Now:yyyyMMddHHmmss}";
                
                return (true, chaveAcesso, protocoloContingencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na emissão FS-DA");
                return (false, "", "");
            }
        }

        private async Task<(bool sucesso, string chaveAcesso, string protocolo)> EmitirSVCAN(nfse_backend.Models.NFe.NFe nfe)
        {
            try
            {
                _logger.LogInformation("Emitindo em contingência SVC-AN");

                // SVC-AN usa os webservices de contingência dos Correios
                var chaveAcesso = GerarChaveAcesso(nfe);
                nfe.ChaveAcesso = chaveAcesso;

                // TODO: Implementar envio para SVC-AN
                // Por enquanto, funciona como FS-DA
                return await EmitirFSDA(nfe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na emissão SVC-AN");
                return (false, "", "");
            }
        }

        public async Task<List<string>> ProcessarFilaContingencia(string cnpjEmpresa)
        {
            try
            {
                _logger.LogInformation($"Processando fila de contingência para CNPJ: {cnpjEmpresa}");

                var arquivosProcessados = new List<string>();
                
                // 1. Buscar XMLs em contingência
                var xmlsContingencia = await _armazenamentoService.ListarXmlsContingencia(cnpjEmpresa);
                
                if (!xmlsContingencia.Any())
                {
                    _logger.LogInformation($"Nenhum XML em contingência encontrado para CNPJ: {cnpjEmpresa}");
                    return arquivosProcessados;
                }

                _logger.LogInformation($"Encontrados {xmlsContingencia.Count} XMLs em contingência para processamento");

                foreach (var xmlInfo in xmlsContingencia)
                {
                    try
                    {
                        // 2. Verificar se SEFAZ está disponível antes de tentar reenvio
                        var sefazDisponivel = await VerificarDisponibilidadeSefaz(xmlInfo.UF, xmlInfo.Homologacao);
                        
                        if (!sefazDisponivel)
                        {
                            _logger.LogWarning($"SEFAZ {xmlInfo.UF} ainda indisponível. Pulando XML: {xmlInfo.ChaveAcesso}");
                            continue;
                        }

                        // 3. Tentar reenviar para SEFAZ
                        var resultadoReenvio = await TentarReenviarXmlContingencia(xmlInfo);
                        
                        if (resultadoReenvio.Sucesso)
                        {
                            // 4. Mover para pasta de processados
                            await _armazenamentoService.MoverXmlContingenciaParaProcessado(
                                xmlInfo.CaminhoArquivo, 
                                xmlInfo.ChaveAcesso, 
                                cnpjEmpresa,
                                resultadoReenvio.Protocolo
                            );
                            
                            arquivosProcessados.Add(xmlInfo.ChaveAcesso);
                            _logger.LogInformation($"XML de contingência processado com sucesso: {xmlInfo.ChaveAcesso}");
                        }
                        else
                        {
                            _logger.LogWarning($"Falha no reenvio do XML de contingência: {xmlInfo.ChaveAcesso}. Motivo: {resultadoReenvio.Mensagem}");
                            
                            // Incrementar contador de tentativas
                            await _armazenamentoService.IncrementarTentativasContingencia(xmlInfo.CaminhoArquivo, resultadoReenvio.Mensagem);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao processar XML de contingência: {xmlInfo.ChaveAcesso}");
                    }
                }

                return arquivosProcessados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar fila de contingência");
                return new List<string>();
            }
        }

        private async Task<ResultadoReenvioContingencia> TentarReenviarXmlContingencia(XmlContingenciaInfo xmlInfo)
        {
            try
            {
                // Carregar conteúdo do XML
                var xmlContent = await File.ReadAllTextAsync(xmlInfo.CaminhoArquivo);
                
                // Determinar tipo de operação baseado no conteúdo
                if (xmlContent.Contains("enviNFe"))
                {
                    // XML de envio de lote
                    var resultado = await _webServiceClient.EnviarLoteNFe(xmlContent, xmlInfo.UF, xmlInfo.Homologacao);
                    return ProcessarResultadoReenvio(resultado, "ENVIO_LOTE");
                }
                else if (xmlContent.Contains("envEvento"))
                {
                    // XML de evento (EPEC)
                    var resultado = await _webServiceClient.EnviarEvento(xmlContent, xmlInfo.UF, xmlInfo.Homologacao);
                    return ProcessarResultadoReenvio(resultado, "EVENTO");
                }
                else
                {
                    return new ResultadoReenvioContingencia 
                    { 
                        Sucesso = false, 
                        Mensagem = "Tipo de XML não identificado para reenvio" 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResultadoReenvioContingencia 
                { 
                    Sucesso = false, 
                    Mensagem = $"Erro no reenvio: {ex.Message}" 
                };
            }
        }

        private ResultadoReenvioContingencia ProcessarResultadoReenvio(string resultado, string tipoOperacao)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(resultado);

                var nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");

                var cStatNode = doc.SelectSingleNode("//nfe:cStat", nsManager);
                var xMotivoNode = doc.SelectSingleNode("//nfe:xMotivo", nsManager);
                var protocoloNode = doc.SelectSingleNode("//nfe:nProt", nsManager) ?? 
                                   doc.SelectSingleNode("//nfe:nRec", nsManager);

                if (cStatNode != null)
                {
                    var cStat = cStatNode.InnerText;
                    var xMotivo = xMotivoNode?.InnerText ?? "";
                    var protocolo = protocoloNode?.InnerText ?? "";

                    // Códigos de sucesso
                    if (cStat == "100" || cStat == "103" || cStat == "135" || cStat == "136")
                    {
                        return new ResultadoReenvioContingencia
                        {
                            Sucesso = true,
                            Protocolo = protocolo,
                            CodigoStatus = cStat,
                            Mensagem = xMotivo
                        };
                    }
                    else
                    {
                        return new ResultadoReenvioContingencia
                        {
                            Sucesso = false,
                            CodigoStatus = cStat,
                            Mensagem = $"Código {cStat}: {xMotivo}"
                        };
                    }
                }

                return new ResultadoReenvioContingencia 
                { 
                    Sucesso = false, 
                    Mensagem = "Resposta da SEFAZ não contém código de status" 
                };
            }
            catch (Exception ex)
            {
                return new ResultadoReenvioContingencia 
                { 
                    Sucesso = false, 
                    Mensagem = $"Erro ao processar resposta: {ex.Message}" 
                };
            }
        }

        private bool AnaliserRespostaStatusServico(string respostaXml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(respostaXml);

                var nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");

                var cStatNode = doc.SelectSingleNode("//nfe:cStat", nsManager);
                if (cStatNode != null)
                {
                    var cStat = cStatNode.InnerText;
                    return cStat == "107"; // Serviço em operação
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar resposta do status do serviço");
                return false;
            }
        }

        private string CriarEventoEPEC(nfse_backend.Models.NFe.NFe nfe, string chaveAcesso)
        {
            var agora = DateTime.Now;
            var sequencia = 1;

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<envEvento xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""1.00"">
    <idLote>{agora:yyyyMMddHHmmss}</idLote>
    <evento versao=""1.00"">
        <infEvento Id=""ID110111{chaveAcesso}01"">
            <cOrgao>{nfe.Ide.cUF}</cOrgao>
            <tpAmb>{nfe.Ide.tpAmb}</tpAmb>
            <CNPJ>{nfe.Emit.CNPJ}</CNPJ>
            <chNFe>{chaveAcesso}</chNFe>
            <dhEvento>{agora:yyyy-MM-ddTHH:mm:sszzz}</dhEvento>
            <tpEvento>110111</tpEvento>
            <nSeqEvento>{sequencia}</nSeqEvento>
            <verEvento>1.00</verEvento>
            <detEvento versao=""1.00"">
                <descEvento>EPEC</descEvento>
                <cOrgaoAutor>91</cOrgaoAutor>
                <tpAutor>1</tpAutor>
                <verAplic>EPEC_1.00</verAplic>
                <dhEmi>{nfe.Ide.dhEmi:yyyy-MM-ddTHH:mm:sszzz}</dhEmi>
                <tpNF>{nfe.Ide.tpNF}</tpNF>
                <IE>{nfe.Emit.IE}</IE>
                <dest>
                    <UF>{nfe.Dest.EnderDest.UF}</UF>
                    <CNPJ>{nfe.Dest.CNPJ}</CNPJ>
                    <idEstrangeiro>{nfe.Dest.idEstrangeiro}</idEstrangeiro>
                    <IE>{nfe.Dest.IE}</IE>
                    <vNF>{nfe.Total.ICMSTot.vNF:F2}</vNF>
                    <vICMS>{nfe.Total.ICMSTot.vICMS:F2}</vICMS>
                    <vST>{nfe.Total.ICMSTot.vST:F2}</vST>
                </dest>
            </detEvento>
        </infEvento>
    </evento>
</envEvento>";
        }

        private (bool sucesso, string protocolo) ProcessarRespostaEvento(string respostaXml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(respostaXml);

                var nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");

                var cStatNode = doc.SelectSingleNode("//nfe:cStat", nsManager);
                var nProtNode = doc.SelectSingleNode("//nfe:nProt", nsManager);

                if (cStatNode != null && nProtNode != null)
                {
                    var cStat = cStatNode.InnerText;
                    var protocolo = nProtNode.InnerText;

                    // 136 = Evento registrado e vinculado a NF-e
                    return (cStat == "136", protocolo);
                }

                return (false, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar resposta do evento");
                return (false, "");
            }
        }

        private string GerarChaveAcesso(nfse_backend.Models.NFe.NFe nfe)
        {
            var cUF = nfe.Ide.cUF.ToString("00");
            var aamm = nfe.Ide.dhEmi?.ToString("yyMM") ?? "";
            var cnpj = nfe.Emit.CNPJ.PadLeft(14, '0');
            var mod = nfe.Ide.mod.ToString("00");
            var serie = nfe.Ide.serie.ToString("000");
            var numero = nfe.Ide.nNF.ToString("000000000");
            var tpEmis = nfe.Ide.tpEmis.ToString();
            var codigo = new Random().Next(1, 99999999).ToString("00000000");

            var chave = cUF + aamm + cnpj + mod + serie + numero + tpEmis + codigo;
            var dv = CalcularDigitoVerificador(chave);

            return chave + dv;
        }

        private int CalcularDigitoVerificador(string chave)
        {
            var sequencia = "4329876543298765432987654329876543298765432";
            var soma = 0;

            for (int i = 0; i < 43; i++)
            {
                soma += int.Parse(chave[i].ToString()) * int.Parse(sequencia[i].ToString());
            }

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        private string GerarXmlNFe(nfse_backend.Models.NFe.NFe nfe)
        {
            // TODO: Implementar geração completa do XML da NFe
            // Por enquanto, retorna um XML básico
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe Id=""NFe{nfe.ChaveAcesso}"" versao=""4.00"">
        <!-- XML completo da NFe seria gerado aqui -->
    </infNFe>
</NFe>";
        }
    }

    public class ContingenciaConfig
    {
        public bool HabilitarContingencia { get; set; } = true;
        public TipoContingencia TipoContingenciaPadrao { get; set; } = TipoContingencia.EPEC;
        public int TimeoutVerificacaoSefaz { get; set; } = 30; // segundos
        public int IntervaloProcessamentoFila { get; set; } = 300; // segundos (5 minutos)
        public bool ProcessarFilaAutomaticamente { get; set; } = true;
    }
}
