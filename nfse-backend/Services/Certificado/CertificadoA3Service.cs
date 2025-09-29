using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace nfse_backend.Services.Certificado
{
    /// <summary>
    /// Serviço especializado para gerenciamento de certificados A3 (token/cartão/HSM)
    /// </summary>
    public class CertificadoA3Service
    {
        private readonly ILogger<CertificadoA3Service> _logger;
        private readonly Dictionary<string, X509Certificate2> _cacheTokens;
        private readonly object _lockCache = new object();

        public CertificadoA3Service(ILogger<CertificadoA3Service> logger)
        {
            _logger = logger;
            _cacheTokens = new Dictionary<string, X509Certificate2>();
        }

        /// <summary>
        /// Lista todos os certificados A3 disponíveis nos tokens/cartões conectados
        /// </summary>
        public List<CertificadoA3Info> ListarCertificadosA3Disponiveis()
        {
            var certificados = new List<CertificadoA3Info>();

            try
            {
                _logger.LogInformation("Listando certificados A3 disponíveis...");

                // Verificar Personal Store (MY) - onde ficam os certificados A3 quando inseridos
                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    certificados.AddRange(ProcessarCertificadosStore(store, "CurrentUser\\My"));
                }

                // Verificar Machine Store
                using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);
                    certificados.AddRange(ProcessarCertificadosStore(store, "LocalMachine\\My"));
                }

                _logger.LogInformation($"Encontrados {certificados.Count} certificados A3");
                
                return certificados.Where(c => c.IsA3).OrderByDescending(c => c.ValidoAte).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar certificados A3");
                return new List<CertificadoA3Info>();
            }
        }

        /// <summary>
        /// Carrega um certificado A3 específico por thumbprint
        /// </summary>
        public X509Certificate2? CarregarCertificadoA3(string thumbprint, bool forcarRecarregar = false)
        {
            try
            {
                // Verificar cache primeiro
                if (!forcarRecarregar)
                {
                    lock (_lockCache)
                    {
                        if (_cacheTokens.ContainsKey(thumbprint))
                        {
                            var certCache = _cacheTokens[thumbprint];
                            if (ValidarCertificadoAtivo(certCache))
                            {
                                _logger.LogDebug($"Certificado A3 carregado do cache: {thumbprint}");
                                return certCache;
                            }
                            else
                            {
                                // Remover do cache se inválido
                                _cacheTokens.Remove(thumbprint);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Carregando certificado A3: {thumbprint}");

                // Buscar em todas as stores
                var stores = new[]
                {
                    new { Store = new X509Store(StoreName.My, StoreLocation.CurrentUser), Nome = "CurrentUser\\My" },
                    new { Store = new X509Store(StoreName.My, StoreLocation.LocalMachine), Nome = "LocalMachine\\My" }
                };

                foreach (var storeInfo in stores)
                {
                    try
                    {
                        storeInfo.Store.Open(OpenFlags.ReadOnly);
                        
                        var certificado = storeInfo.Store.Certificates
                            .Cast<X509Certificate2>()
                            .FirstOrDefault(c => c.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase));

                        if (certificado != null)
                        {
                            if (ValidarCertificadoA3(certificado))
                            {
                                // Adicionar ao cache
                                lock (_lockCache)
                                {
                                    _cacheTokens[thumbprint] = certificado;
                                }

                                _logger.LogInformation($"Certificado A3 carregado com sucesso de {storeInfo.Nome}");
                                return certificado;
                            }
                            else
                            {
                                _logger.LogWarning($"Certificado encontrado mas não é válido para A3: {thumbprint}");
                            }
                        }
                    }
                    finally
                    {
                        storeInfo.Store.Close();
                    }
                }

                _logger.LogError($"Certificado A3 não encontrado: {thumbprint}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao carregar certificado A3: {thumbprint}");
                return null;
            }
        }

        /// <summary>
        /// Verifica se um token/cartão está conectado e acessível
        /// </summary>
        public bool VerificarTokenConectado(string thumbprint)
        {
            try
            {
                var certificado = CarregarCertificadoA3(thumbprint, forcarRecarregar: true);
                
                if (certificado == null)
                    return false;

                // Tentar acessar a chave privada para confirmar que o token está acessível
                try
                {
                    var rsa = certificado.GetRSAPrivateKey();
                    if (rsa == null)
                        return false;

                    // Teste simples de assinatura para verificar se a chave privada está acessível
                    var testData = System.Text.Encoding.UTF8.GetBytes("test");
                    var signature = rsa.SignData(testData, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                    
                    return signature != null && signature.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Monitora mudanças nos tokens conectados (inserção/remoção)
        /// </summary>
        public void MonitorarMudancasTokens(Action<string> onTokenInserido, Action<string> onTokenRemovido)
        {
            // Implementação básica - em produção usar SystemEvents ou polling
            var tokensAtuais = ListarCertificadosA3Disponiveis().Select(c => c.Thumbprint).ToHashSet();
            
            System.Threading.Tasks.Task.Run(async () =>
            {
                var tokensAnteriores = new HashSet<string>(tokensAtuais);
                
                while (true)
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(5000); // Verificar a cada 5 segundos
                        
                        var tokensNovos = ListarCertificadosA3Disponiveis().Select(c => c.Thumbprint).ToHashSet();
                        
                        // Tokens inseridos
                        var inseridos = tokensNovos.Except(tokensAnteriores);
                        foreach (var token in inseridos)
                        {
                            _logger.LogInformation($"Token A3 inserido: {token}");
                            onTokenInserido?.Invoke(token);
                        }
                        
                        // Tokens removidos
                        var removidos = tokensAnteriores.Except(tokensNovos);
                        foreach (var token in removidos)
                        {
                            _logger.LogWarning($"Token A3 removido: {token}");
                            onTokenRemovido?.Invoke(token);
                            
                            // Remover do cache
                            lock (_lockCache)
                            {
                                _cacheTokens.Remove(token);
                            }
                        }
                        
                        tokensAnteriores = tokensNovos;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao monitorar mudanças nos tokens A3");
                    }
                }
            });
        }

        private List<CertificadoA3Info> ProcessarCertificadosStore(X509Store store, string nomeStore)
        {
            var certificados = new List<CertificadoA3Info>();

            foreach (X509Certificate2 cert in store.Certificates)
            {
                try
                {
                    var info = new CertificadoA3Info
                    {
                        Thumbprint = cert.Thumbprint,
                        Subject = cert.Subject,
                        Issuer = cert.Issuer,
                        ValidoDe = cert.NotBefore,
                        ValidoAte = cert.NotAfter,
                        SerialNumber = cert.SerialNumber,
                        IsA3 = IdentificarCertificadoA3(cert),
                        TemChavePrivada = cert.HasPrivateKey,
                        Store = nomeStore,
                        CnpjTitular = ExtrairCnpjDoCertificado(cert),
                        NomeTitular = ExtrairNomeTitular(cert),
                        TipoUso = DeterminarTipoUso(cert),
                        IsICPBrasil = VerificarICPBrasil(cert),
                        IsValido = ValidarCertificadoAtivo(cert)
                    };

                    certificados.Add(info);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Erro ao processar certificado: {cert.Thumbprint}");
                }
            }

            return certificados;
        }

        private bool IdentificarCertificadoA3(X509Certificate2 certificado)
        {
            try
            {
                // Certificados A3 geralmente não têm a chave privada exportável
                if (!certificado.HasPrivateKey)
                    return false;

                // Verificar se a chave privada está em hardware (não exportável)
                var rsa = certificado.GetRSAPrivateKey();
                if (rsa is System.Security.Cryptography.RSACng rsaCng)
                {
                    return !rsaCng.Key.IsEphemeral; // Chaves em hardware não são efêmeras
                }

                // Verificar extensões específicas de tokens/cartões
                foreach (var extension in certificado.Extensions)
                {
                    if (extension.Oid?.Value == "1.3.6.1.4.1.311.20.2") // Microsoft Smart Card Logon
                        return true;
                }

                // Heurística: certificados A3 geralmente têm issuer diferentes dos A1
                var issuer = certificado.Issuer.ToUpper();
                if (issuer.Contains("SMART CARD") || issuer.Contains("TOKEN") || issuer.Contains("CARD"))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidarCertificadoA3(X509Certificate2 certificado)
        {
            return IdentificarCertificadoA3(certificado) && 
                   ValidarCertificadoAtivo(certificado) && 
                   VerificarICPBrasil(certificado);
        }

        private bool ValidarCertificadoAtivo(X509Certificate2 certificado)
        {
            var agora = DateTime.Now;
            return certificado.NotBefore <= agora && certificado.NotAfter >= agora;
        }

        private bool VerificarICPBrasil(X509Certificate2 certificado)
        {
            foreach (var extension in certificado.Extensions)
            {
                if (extension.Oid?.Value == "2.5.29.32") // Certificate Policies
                {
                    var asndata = new System.Security.Cryptography.AsnEncodedData(extension.Oid, extension.RawData);
                    if (asndata.Format(false).Contains("2.16.76.1.2.")) // OID ICP-Brasil
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string ExtrairCnpjDoCertificado(X509Certificate2 certificado)
        {
            try
            {
                // CNPJ geralmente está no Subject como OID 2.16.76.1.3.3
                var subject = certificado.Subject;
                var regex = new System.Text.RegularExpressions.Regex(@"OID\.2\.16\.76\.1\.3\.3=(\d{14})");
                var match = regex.Match(subject);
                
                if (match.Success)
                    return match.Groups[1].Value;

                // Fallback: procurar por padrão de CNPJ no Subject
                var cnpjRegex = new System.Text.RegularExpressions.Regex(@"(\d{14})");
                var cnpjMatch = cnpjRegex.Match(subject);
                
                return cnpjMatch.Success ? cnpjMatch.Groups[1].Value : "";
            }
            catch
            {
                return "";
            }
        }

        private string ExtrairNomeTitular(X509Certificate2 certificado)
        {
            try
            {
                // Extrair CN (Common Name)
                var subject = certificado.Subject;
                var regex = new System.Text.RegularExpressions.Regex(@"CN=([^,]+)");
                var match = regex.Match(subject);
                
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }
            catch
            {
                return "";
            }
        }

        private string DeterminarTipoUso(X509Certificate2 certificado)
        {
            try
            {
                // Verificar Key Usage extension
                var keyUsageExt = certificado.Extensions["2.5.29.15"] as X509KeyUsageExtension;
                if (keyUsageExt != null)
                {
                    var keyUsage = keyUsageExt.KeyUsages;
                    
                    if (keyUsage.HasFlag(X509KeyUsageFlags.DigitalSignature) && 
                        keyUsage.HasFlag(X509KeyUsageFlags.NonRepudiation))
                    {
                        return "PESSOA_FISICA";
                    }
                    else if (keyUsage.HasFlag(X509KeyUsageFlags.DigitalSignature))
                    {
                        return "PESSOA_JURIDICA";
                    }
                }

                // Fallback baseado no Subject
                var subject = certificado.Subject.ToUpper();
                if (subject.Contains("PF") || subject.Contains("PESSOA FISICA"))
                    return "PESSOA_FISICA";
                else if (subject.Contains("PJ") || subject.Contains("PESSOA JURIDICA"))
                    return "PESSOA_JURIDICA";

                return "DESCONHECIDO";
            }
            catch
            {
                return "DESCONHECIDO";
            }
        }
    }

    /// <summary>
    /// Informações sobre um certificado A3
    /// </summary>
    public class CertificadoA3Info
    {
        public string Thumbprint { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Issuer { get; set; } = "";
        public DateTime ValidoDe { get; set; }
        public DateTime ValidoAte { get; set; }
        public string SerialNumber { get; set; } = "";
        public bool IsA3 { get; set; }
        public bool TemChavePrivada { get; set; }
        public string Store { get; set; } = "";
        public string CnpjTitular { get; set; } = "";
        public string NomeTitular { get; set; } = "";
        public string TipoUso { get; set; } = "";
        public bool IsICPBrasil { get; set; }
        public bool IsValido { get; set; }

        public int DiasParaVencer => (ValidoAte - DateTime.Now).Days;
        public bool VenceEm30Dias => DiasParaVencer <= 30 && DiasParaVencer > 0;
        public bool Vencido => DiasParaVencer < 0;
    }
}
