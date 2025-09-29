using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace nfse_backend.Services.Validation
{
    /// <summary>
    /// Validador de Schema aprimorado baseado na documentação oficial da NF-e
    /// Implementa validações rigorosas conforme Portal Nacional da NF-e
    /// </summary>
    public class SchemaValidatorAprimorado
    {
        private readonly ILogger<SchemaValidatorAprimorado> _logger;
        private readonly string _schemasPath;
        private readonly Dictionary<string, XmlSchemaSet> _schemaCache;

        public SchemaValidatorAprimorado(ILogger<SchemaValidatorAprimorado> logger)
        {
            _logger = logger;
            _schemasPath = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "NFe", "v4.00");
            _schemaCache = new Dictionary<string, XmlSchemaSet>();
        }

        /// <summary>
        /// Valida e limpa campos do XML conforme documentação oficial
        /// Remove espaços em branco que causam rejeições (código 215, 493)
        /// </summary>
        public ValidationResult ValidarELimparXml(string xmlContent, string tipoDocumento = "NFe")
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                // 1. Limpar espaços em branco (problema comum identificado na documentação)
                LimparEspacosEmBranco(xmlDoc);

                // 2. Validar contra XSD oficial
                var validationErrors = ValidarContraXSD(xmlDoc, tipoDocumento);

                // 3. Validações específicas baseadas na documentação oficial
                var customErrors = ValidacoesCustomizadas(xmlDoc, tipoDocumento);

                var allErrors = new List<string>();
                allErrors.AddRange(validationErrors);
                allErrors.AddRange(customErrors);

                return new ValidationResult
                {
                    IsValid = allErrors.Count == 0,
                    Errors = allErrors,
                    XmlLimpo = xmlDoc.OuterXml
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante validação do XML");
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { $"Erro de validação: {ex.Message}" },
                    XmlLimpo = xmlContent
                };
            }
        }

        /// <summary>
        /// Remove espaços em branco que causam falhas de schema XML
        /// Baseado no exemplo da documentação oficial (código 215)
        /// </summary>
        private void LimparEspacosEmBranco(XmlDocument xmlDoc)
        {
            var textNodes = xmlDoc.SelectNodes("//text()");
            if (textNodes == null) return;

            foreach (XmlNode node in textNodes)
            {
                if (node.Value != null)
                {
                    var originalValue = node.Value;
                    var trimmedValue = originalValue.Trim();
                    
                    if (originalValue != trimmedValue)
                    {
                        node.Value = trimmedValue;
                        _logger.LogDebug($"Removidos espaços em branco do campo: {node.ParentNode?.Name}");
                    }
                }
            }

            // Limpeza específica para campos críticos identificados na documentação
            var camposCriticos = new[] { "xJust", "xMotivo", "xCorrecao", "xCondUso" };
            foreach (var campo in camposCriticos)
            {
                var nodes = xmlDoc.SelectNodes($"//{campo}");
                if (nodes == null) continue;

                foreach (XmlNode node in nodes)
                {
                    if (node.InnerText != null)
                    {
                        node.InnerText = node.InnerText.Trim();
                    }
                }
            }
        }

        /// <summary>
        /// Valida XML contra XSD oficial
        /// </summary>
        private List<string> ValidarContraXSD(XmlDocument xmlDoc, string tipoDocumento)
        {
            var errors = new List<string>();

            try
            {
                var schemaSet = ObterSchemaSet(tipoDocumento);
                xmlDoc.Schemas = schemaSet;

                xmlDoc.Validate((sender, e) =>
                {
                    var errorMessage = $"Erro XSD: {e.Message}";
                    if (e.Exception?.LineNumber > 0)
                    {
                        errorMessage += $" (Linha: {e.Exception.LineNumber})";
                    }
                    errors.Add(errorMessage);
                    _logger.LogWarning(errorMessage);
                });
            }
            catch (Exception ex)
            {
                var error = $"Erro ao carregar schema XSD: {ex.Message}";
                errors.Add(error);
                _logger.LogError(ex, error);
            }

            return errors;
        }

        /// <summary>
        /// Validações customizadas baseadas na documentação oficial
        /// </summary>
        private List<string> ValidacoesCustomizadas(XmlDocument xmlDoc, string tipoDocumento)
        {
            var errors = new List<string>();

            try
            {
                // Validação de data/hora conforme documentação oficial
                ValidarFormatoDataHora(xmlDoc, errors);

                // Validação de chave de acesso
                ValidarChaveAcesso(xmlDoc, errors);

                // Validação específica por tipo de documento
                switch (tipoDocumento.ToUpper())
                {
                    case "NFE":
                        ValidarNFe(xmlDoc, errors);
                        break;
                    case "EVENTO":
                        ValidarEvento(xmlDoc, errors);
                        break;
                    case "INUTILIZACAO":
                        ValidarInutilizacao(xmlDoc, errors);
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Erro nas validações customizadas: {ex.Message}");
                _logger.LogError(ex, "Erro nas validações customizadas");
            }

            return errors;
        }

        /// <summary>
        /// Valida formato de data/hora conforme padrão oficial: AAAA-MM-DDTHH:MM:SS-03:00
        /// </summary>
        private void ValidarFormatoDataHora(XmlDocument xmlDoc, List<string> errors)
        {
            var camposDataHora = new[] { "dhEmi", "dhEvento", "dhRegEvento", "dhResp" };
            
            foreach (var campo in camposDataHora)
            {
                var nodes = xmlDoc.SelectNodes($"//{campo}");
                if (nodes == null) continue;

                foreach (XmlNode node in nodes)
                {
                    if (!string.IsNullOrEmpty(node.InnerText))
                    {
                        if (!DateTime.TryParse(node.InnerText, out _))
                        {
                            errors.Add($"Formato de data/hora inválido no campo {campo}: {node.InnerText}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Valida chave de acesso (44 dígitos)
        /// </summary>
        private void ValidarChaveAcesso(XmlDocument xmlDoc, List<string> errors)
        {
            var camposChave = new[] { "chNFe", "chCTe" };
            
            foreach (var campo in camposChave)
            {
                var nodes = xmlDoc.SelectNodes($"//{campo}");
                if (nodes == null) continue;

                foreach (XmlNode node in nodes)
                {
                    var chave = node.InnerText;
                    if (!string.IsNullOrEmpty(chave))
                    {
                        if (chave.Length != 44 || !long.TryParse(chave, out _))
                        {
                            errors.Add($"Chave de acesso inválida no campo {campo}: {chave}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validações específicas para NFe
        /// </summary>
        private void ValidarNFe(XmlDocument xmlDoc, List<string> errors)
        {
            // Validar se existe pelo menos um item
            var itens = xmlDoc.SelectNodes("//det");
            if (itens == null || itens.Count == 0)
            {
                errors.Add("NFe deve conter pelo menos um item (det)");
            }

            // Validar totais
            var vNF = xmlDoc.SelectSingleNode("//total/ICMSTot/vNF")?.InnerText;
            if (string.IsNullOrEmpty(vNF) || !decimal.TryParse(vNF, out var valorNF) || valorNF <= 0)
            {
                errors.Add("Valor total da NFe deve ser maior que zero");
            }
        }

        /// <summary>
        /// Validações específicas para Eventos
        /// </summary>
        private void ValidarEvento(XmlDocument xmlDoc, List<string> errors)
        {
            // Validar tipo de evento
            var tpEvento = xmlDoc.SelectSingleNode("//tpEvento")?.InnerText;
            if (string.IsNullOrEmpty(tpEvento))
            {
                errors.Add("Tipo de evento é obrigatório");
            }

            // Validar sequência do evento
            var nSeqEvento = xmlDoc.SelectSingleNode("//nSeqEvento")?.InnerText;
            if (string.IsNullOrEmpty(nSeqEvento) || !int.TryParse(nSeqEvento, out var seq) || seq <= 0)
            {
                errors.Add("Sequência do evento deve ser um número maior que zero");
            }
        }

        /// <summary>
        /// Validações específicas para Inutilização
        /// </summary>
        private void ValidarInutilizacao(XmlDocument xmlDoc, List<string> errors)
        {
            // Validar justificativa (mínimo 15 caracteres conforme documentação)
            var xJust = xmlDoc.SelectSingleNode("//xJust")?.InnerText;
            if (string.IsNullOrEmpty(xJust) || xJust.Trim().Length < 15)
            {
                errors.Add("Justificativa deve ter pelo menos 15 caracteres");
            }

            // Validar numeração
            var nNFIni = xmlDoc.SelectSingleNode("//nNFIni")?.InnerText;
            var nNFFin = xmlDoc.SelectSingleNode("//nNFFin")?.InnerText;
            
            if (int.TryParse(nNFIni, out var numIni) && int.TryParse(nNFFin, out var numFin))
            {
                if (numIni > numFin)
                {
                    errors.Add("Número inicial não pode ser maior que o número final");
                }
            }
        }

        /// <summary>
        /// Obtém ou carrega o schema set do cache
        /// </summary>
        private XmlSchemaSet ObterSchemaSet(string tipoDocumento)
        {
            if (_schemaCache.ContainsKey(tipoDocumento))
            {
                return _schemaCache[tipoDocumento];
            }

            var schemaSet = new XmlSchemaSet();
            var schemaFile = GetSchemaFileName(tipoDocumento);
            var schemaPath = Path.Combine(_schemasPath, schemaFile);

            if (File.Exists(schemaPath))
            {
                schemaSet.Add("http://www.portalfiscal.inf.br/nfe", schemaPath);
                _schemaCache[tipoDocumento] = schemaSet;
            }
            else
            {
                _logger.LogWarning($"Schema XSD não encontrado: {schemaPath}");
            }

            return schemaSet;
        }

        /// <summary>
        /// Obtém o nome do arquivo XSD baseado no tipo de documento
        /// </summary>
        private string GetSchemaFileName(string tipoDocumento)
        {
            return tipoDocumento.ToUpper() switch
            {
                "NFE" => "nfe_v4.00.xsd",
                "EVENTO" => "eventoNFe_v1.00.xsd",
                "INUTILIZACAO" => "inutNFe_v4.00.xsd",
                "CONSULTA" => "consSitNFe_v4.00.xsd",
                _ => "nfe_v4.00.xsd"
            };
        }
    }

    /// <summary>
    /// Resultado da validação
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string XmlLimpo { get; set; } = string.Empty;
    }
}
