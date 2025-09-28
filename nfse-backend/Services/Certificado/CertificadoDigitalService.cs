using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace nfse_backend.Services.Certificado
{
    public class CertificadoDigitalService
    {
        private X509Certificate2? _certificado;
        private readonly string? _certificadoPath;
        private readonly string? _certificadoSenha;

        public CertificadoDigitalService(string? certificadoPath = null, string? certificadoSenha = null)
        {
            _certificadoPath = certificadoPath ?? Environment.GetEnvironmentVariable("CERTIFICADO_PATH");
            _certificadoSenha = certificadoSenha ?? Environment.GetEnvironmentVariable("CERTIFICADO_SENHA");
        }

        public void CarregarCertificadoA1(string caminhoArquivo, string senha)
        {
            try
            {
                _certificado = new X509Certificate2(caminhoArquivo, senha, 
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                
                ValidarCertificado();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao carregar certificado A1: {ex.Message}", ex);
            }
        }

        public void CarregarCertificadoA1Bytes(byte[] certificadoBytes, string senha)
        {
            try
            {
                _certificado = new X509Certificate2(certificadoBytes, senha,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                ValidarCertificado();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao carregar certificado A1 (bytes): {ex.Message}", ex);
            }
        }

        public void CarregarCertificadoA3(string? serialNumber = null, string? thumbprint = null)
        {
            try
            {
                Console.WriteLine("Iniciando busca por certificado A3/Token/HSM");

                X509Store? store = null;
                try
                {
                    // Tentar Personal primeiro (mais comum)
                    store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly);
                    
                    var certificado = BuscarCertificadoNaStore(store, serialNumber, thumbprint);
                    if (certificado != null)
                    {
                        _certificado = certificado;
                        ValidarCertificado();
                        Console.WriteLine($"Certificado A3 encontrado no CurrentUser. Válido até: {_certificado.NotAfter}");
                        return;
                    }
                }
                finally
                {
                    store?.Close();
                }

                // Tentar LocalMachine se não encontrou no CurrentUser
                try
                {
                    store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);
                    
                    var certificado = BuscarCertificadoNaStore(store, serialNumber, thumbprint);
                    if (certificado != null)
                    {
                        _certificado = certificado;
                        ValidarCertificado();
                        Console.WriteLine($"Certificado A3 encontrado no LocalMachine. Válido até: {_certificado.NotAfter}");
                        return;
                    }
                }
                finally
                {
                    store?.Close();
                }

                // Se não encontrou, listar certificados disponíveis
                ListarCertificadosDisponiveis();
                throw new Exception("Certificado A3/Token não encontrado. Verifique se o token está conectado e o driver instalado.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao carregar certificado A3: {ex.Message}", ex);
            }
        }

        private X509Certificate2? BuscarCertificadoNaStore(X509Store store, string? serialNumber, string? thumbprint)
        {
            foreach (X509Certificate2 cert in store.Certificates)
            {
                try
                {
                    // Verificar se é certificado ICP-Brasil válido
                    if (!cert.HasPrivateKey)
                        continue;

                    // Verificar se não está expirado
                    if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
                        continue;

                    // Verificar se é certificado de pessoa jurídica (ICP-Brasil)
                    var subject = cert.Subject;
                    if (!subject.Contains("ICP-Brasil") && !IsICPBrasilCertificate(cert))
                        continue;

                    // Se foi especificado serial number ou thumbprint, verificar
                    if (!string.IsNullOrEmpty(serialNumber) && cert.SerialNumber != serialNumber)
                        continue;

                    if (!string.IsNullOrEmpty(thumbprint) && cert.Thumbprint != thumbprint)
                        continue;

                    Console.WriteLine($"Certificado encontrado: {cert.Subject}");
                    return cert;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao verificar certificado: {cert.Subject} - {ex.Message}");
                    continue;
                }
            }

            return null;
        }

        private bool IsICPBrasilCertificate(X509Certificate2 certificate)
        {
            try
            {
                // Verificar pela extensão de políticas de certificado
                foreach (X509Extension extension in certificate.Extensions)
                {
                    if (extension.Oid?.Value == "2.5.29.32") // Certificate Policies
                    {
                        var rawData = extension.RawData;
                        var policyString = System.Text.Encoding.ASCII.GetString(rawData);
                        
                        // OIDs comuns do ICP-Brasil
                        if (policyString.Contains("2.16.76.1.2.1") ||  // A1 PJ
                            policyString.Contains("2.16.76.1.2.2") ||  // A3 PJ
                            policyString.Contains("2.16.76.1.3.1") ||  // A1 PF
                            policyString.Contains("2.16.76.1.3.2"))    // A3 PF
                        {
                            return true;
                        }
                    }
                }

                // Verificar pelo issuer
                var issuer = certificate.Issuer;
                return issuer.Contains("ICP-Brasil") || 
                       issuer.Contains("Autoridade Certificadora") ||
                       issuer.Contains("AC ") ||
                       issuer.Contains("Certisign") ||
                       issuer.Contains("Serasa") ||
                       issuer.Contains("Valid");
            }
            catch
            {
                return false;
            }
        }

        public void ListarCertificadosDisponiveis()
        {
            try
            {
                Console.WriteLine("=== CERTIFICADOS DISPONÍVEIS ===");
                
                ListarCertificadosStore(StoreName.My, StoreLocation.CurrentUser, "CurrentUser\\Personal");
                ListarCertificadosStore(StoreName.My, StoreLocation.LocalMachine, "LocalMachine\\Personal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar certificados: {ex.Message}");
            }
        }

        private void ListarCertificadosStore(StoreName storeName, StoreLocation storeLocation, string storeDescription)
        {
            X509Store? store = null;
            try
            {
                store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);
                
                Console.WriteLine($"\n--- {storeDescription} ---");
                
                foreach (X509Certificate2 cert in store.Certificates)
                {
                    try
                    {
                        var temChavePrivada = cert.HasPrivateKey ? "SIM" : "NÃO";
                        var valido = (DateTime.Now >= cert.NotBefore && DateTime.Now <= cert.NotAfter) ? "VÁLIDO" : "EXPIRADO";
                        var icpBrasil = IsICPBrasilCertificate(cert) ? "ICP-Brasil" : "Outro";
                        
                        Console.WriteLine($"  Subject: {cert.Subject}");
                        Console.WriteLine($"  Serial: {cert.SerialNumber}");
                        Console.WriteLine($"  Thumbprint: {cert.Thumbprint}");
                        Console.WriteLine($"  Válido: {cert.NotBefore:dd/MM/yyyy} até {cert.NotAfter:dd/MM/yyyy} ({valido})");
                        Console.WriteLine($"  Chave Privada: {temChavePrivada}");
                        Console.WriteLine($"  Tipo: {icpBrasil}");
                        Console.WriteLine($"  ---");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao listar certificado: {cert.Subject} - {ex.Message}");
                    }
                }
            }
            finally
            {
                store?.Close();
            }
        }

        private void ValidarCertificado()
        {
            var certificado = _certificado ?? throw new Exception("Certificado não carregado.");

            // Verifica se é um certificado ICP-Brasil
            bool isICPBrasil = false;
            foreach (var extension in certificado.Extensions)
            {
                if (extension.Oid != null && extension.Oid.Value == "2.5.29.32") // Certificate Policies
                {
                    var asndata = new System.Security.Cryptography.AsnEncodedData(extension.Oid, extension.RawData);
                    if (asndata.Format(false).Contains("2.16.76.1.2.")) // OID ICP-Brasil
                    {
                        isICPBrasil = true;
                        break;
                    }
                }
            }

            if (!isICPBrasil)
            {
                throw new Exception("O certificado não é um certificado ICP-Brasil válido.");
            }

            // Verifica validade
            if (DateTime.Now < certificado.NotBefore || DateTime.Now > certificado.NotAfter)
            {
                throw new Exception($"Certificado fora do período de validade. Válido de {certificado.NotBefore} até {certificado.NotAfter}");
            }

            // Verifica se tem chave privada
            if (!certificado.HasPrivateKey)
            {
                throw new Exception("O certificado não possui chave privada.");
            }
        }

        public string AssinarXml(string xmlContent, string tagParaAssinar = "infNFe")
        {
            if (_certificado == null)
            {
                // Tenta carregar o certificado automaticamente se configurado
                if (!string.IsNullOrEmpty(_certificadoPath) && !string.IsNullOrEmpty(_certificadoSenha))
                {
                    CarregarCertificadoA1(_certificadoPath, _certificadoSenha);
                }
                else
                {
                    throw new Exception("Certificado digital não carregado.");
                }
            }

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(xmlContent);

                // Remove declaração XML se existir (será adicionada depois)
                if (xmlDoc.FirstChild is XmlDeclaration)
                {
                    xmlDoc.RemoveChild(xmlDoc.FirstChild);
                }

                // Localiza o elemento a ser assinado
                XmlNodeList nodeList = xmlDoc.GetElementsByTagName(tagParaAssinar);
                if (nodeList.Count == 0)
                {
                    throw new Exception($"Tag '{tagParaAssinar}' não encontrada no XML.");
                }

                foreach (XmlElement element in nodeList)
                {
                    var certificado = _certificado ?? throw new Exception("Certificado digital não carregado.");
                    // Verifica se o elemento tem o atributo Id
                    string id = element.GetAttribute("Id");
                    if (string.IsNullOrEmpty(id))
                    {
                        throw new Exception($"O elemento '{tagParaAssinar}' não possui o atributo 'Id'.");
                    }

                    // Cria a assinatura
                    SignedXml signedXml = new SignedXml(xmlDoc);
                    var rsa = certificado.GetRSAPrivateKey() ?? throw new Exception("Certificado sem chave privada RSA.");
                    signedXml.SigningKey = rsa;
                    var signedInfo = signedXml.SignedInfo ?? throw new Exception("SignedInfo não disponível.");
                    signedInfo.CanonicalizationMethod = SignedXml.XmlDsigCanonicalizationUrl;
                    signedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

                    // Cria a referência
                    Reference reference = new Reference();
                    reference.Uri = $"#{id}";
                    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                    reference.AddTransform(new XmlDsigC14NTransform());
                    reference.DigestMethod = SignedXml.XmlDsigSHA256Url;
                    signedXml.AddReference(reference);

                    // Adiciona KeyInfo
                    KeyInfo keyInfo = new KeyInfo();
                    keyInfo.AddClause(new KeyInfoX509Data(certificado));
                    signedXml.KeyInfo = keyInfo;

                    // Computa a assinatura
                    signedXml.ComputeSignature();

                    // Obtém o XML da assinatura
                    XmlElement xmlSignature = signedXml.GetXml();

                    // Adiciona a assinatura ao documento
                    var parent = element.ParentNode;
                    if (parent is null)
                    {
                        throw new Exception("Elemento não possui nó pai para anexar assinatura.");
                    }
                    parent.AppendChild(xmlSignature);
                }

                // Adiciona a declaração XML
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

                // Retorna o XML assinado
                using (StringWriter sw = new StringWriter())
                {
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
                    xmlDoc.WriteTo(xmlTextWriter);
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao assinar XML: {ex.Message}", ex);
            }
        }

        public bool VerificarAssinatura(string xmlAssinado)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(xmlAssinado);

                // Localiza as assinaturas
                XmlNodeList signatureNodes = xmlDoc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);
                if (signatureNodes.Count == 0)
                {
                    return false;
                }

                foreach (XmlElement signatureElement in signatureNodes)
                {
                    SignedXml signedXml = new SignedXml(xmlDoc);
                    signedXml.LoadXml(signatureElement);

                    // Verifica a assinatura
                    if (!signedXml.CheckSignature())
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public X509Certificate2 ObterCertificado()
        {
            return _certificado ?? throw new Exception("Certificado digital não carregado.");
        }

        public string ObterDadosCertificado()
        {
            if (_certificado == null)
            {
                return "Certificado não carregado.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Titular: {_certificado.Subject}");
            sb.AppendLine($"Emissor: {_certificado.Issuer}");
            sb.AppendLine($"Número de Série: {_certificado.SerialNumber}");
            sb.AppendLine($"Válido de: {_certificado.NotBefore:dd/MM/yyyy}");
            sb.AppendLine($"Válido até: {_certificado.NotAfter:dd/MM/yyyy}");
            sb.AppendLine($"Thumbprint: {_certificado.Thumbprint}");

            // Extrai CNPJ do certificado
            string? cnpj = ExtrairCNPJCertificado();
            if (!string.IsNullOrEmpty(cnpj))
            {
                sb.AppendLine($"CNPJ: {cnpj}");
            }

            return sb.ToString();
        }

        public Dictionary<string, object> ObterInformacoesCertificado()
        {
            var informacoes = new Dictionary<string, object>();
            
            if (_certificado == null)
            {
                informacoes["loaded"] = false;
                informacoes["error"] = "Certificado não carregado";
                return informacoes;
            }

            try
            {
                informacoes["loaded"] = true;
                informacoes["subject"] = _certificado.Subject;
                informacoes["issuer"] = _certificado.Issuer;
                informacoes["serialNumber"] = _certificado.SerialNumber;
                informacoes["thumbprint"] = _certificado.Thumbprint;
                informacoes["notBefore"] = _certificado.NotBefore;
                informacoes["notAfter"] = _certificado.NotAfter;
                informacoes["hasPrivateKey"] = _certificado.HasPrivateKey;

                // Calcular dias até expirar
                var diasParaExpirar = (_certificado.NotAfter - DateTime.Now).Days;
                informacoes["daysUntilExpiry"] = diasParaExpirar;
                informacoes["isExpired"] = diasParaExpirar <= 0;
                informacoes["isExpiringSoon"] = diasParaExpirar <= 30;

                // Extrair CNPJ
                string? cnpj = ExtrairCNPJCertificado();
                if (!string.IsNullOrEmpty(cnpj))
                {
                    informacoes["cnpj"] = cnpj;
                }

                // Verificar se é ICP-Brasil
                informacoes["isICPBrasil"] = IsICPBrasilCertificate(_certificado);
                
                return informacoes;
            }
            catch (Exception ex)
            {
                informacoes["loaded"] = false;
                informacoes["error"] = ex.Message;
                return informacoes;
            }
        }

        private string? ExtrairCNPJCertificado()
        {
            if (_certificado == null)
                return null;

            var certificado = _certificado;
            foreach (var extension in certificado.Extensions)
            {
                if (extension.Oid != null && extension.Oid.Value == "2.5.29.17") // Subject Alternative Name
                {
                    var asndata = new System.Security.Cryptography.AsnEncodedData(extension.Oid, extension.RawData);
                    var texto = asndata.Format(false);
                    
                    // Procura por padrão de CNPJ
                    var match = System.Text.RegularExpressions.Regex.Match(texto, @"\d{14}");
                    if (match.Success)
                    {
                        return match.Value;
                    }
                }
            }

            // Tenta extrair do Subject
            var subject = certificado.Subject;
            var cnMatch = System.Text.RegularExpressions.Regex.Match(subject, @"CN=.*?:(\d{14})");
            if (cnMatch.Success)
            {
                return cnMatch.Groups[1].Value;
            }

            return null;
        }

        public void Dispose()
        {
            _certificado?.Dispose();
        }
    }
}
