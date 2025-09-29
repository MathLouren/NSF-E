using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using nfse_backend.Services.Configuracao;
using nfse_backend.Models.Contingencia;
using Microsoft.Extensions.Logging;

namespace nfse_backend.Services.Armazenamento
{
    public class ArmazenamentoSeguroService
    {
        private readonly ILogger<ArmazenamentoSeguroService> _logger;
        private readonly ConfiguracaoNFeService _configuracaoService;
        private readonly string _basePath;
        private readonly byte[] _encryptionKey;

        public ArmazenamentoSeguroService(
            ILogger<ArmazenamentoSeguroService> logger,
            ConfiguracaoNFeService configuracaoService)
        {
            _logger = logger;
            _configuracaoService = configuracaoService;
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NFeStorage");
            
            // Gerar ou carregar chave de criptografia
            _encryptionKey = GerarOuCarregarChaveCriptografia();
            
            InicializarDiretorios();
        }

        private void InicializarDiretorios()
        {
            var config = _configuracaoService.ObterConfiguracoesArmazenamento();
            
            // Criar diretórios principais
            Directory.CreateDirectory(_basePath);
            Directory.CreateDirectory(Path.Combine(_basePath, config.DiretorioXmlAutorizado));
            Directory.CreateDirectory(Path.Combine(_basePath, config.DiretorioXmlRejeitado));
            Directory.CreateDirectory(Path.Combine(_basePath, config.DiretorioXmlContingencia));
            Directory.CreateDirectory(Path.Combine(_basePath, config.DiretorioPDF));
            Directory.CreateDirectory(Path.Combine(_basePath, config.DiretorioBackup));
            Directory.CreateDirectory(Path.Combine(_basePath, "Certificados"));
            Directory.CreateDirectory(Path.Combine(_basePath, "Logs"));
        }

        private byte[] GerarOuCarregarChaveCriptografia()
        {
            var keyPath = Path.Combine(_basePath, ".encryption.key");
            
            if (File.Exists(keyPath))
            {
                // fallback simples sem DPAPI em ambientes cross-plataforma: chave em base64 (não ideal para produção)
                var b64 = File.ReadAllText(keyPath);
                return Convert.FromBase64String(b64);
            }
            else
            {
                // Gerar nova chave
                using (var aes = Aes.Create())
                {
                    aes.GenerateKey();
                    var key = aes.Key;
                    
                    // Salvar em base64 (para produção, usar DPAPI/HSM/KeyVault)
                    File.WriteAllText(keyPath, Convert.ToBase64String(key));
                    
                    return key;
                }
            }
        }

        public async Task<string> SalvarXmlAutorizado(string xml, string chaveAcesso, string cnpjEmitente)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var ano = chaveAcesso.Substring(2, 2);
                var mes = chaveAcesso.Substring(4, 2);
                
                // Estrutura: XMLs/Autorizados/CNPJ/AAMM/chaveAcesso.xml
                var diretorio = Path.Combine(
                    _basePath,
                    config.DiretorioXmlAutorizado,
                    cnpjEmitente,
                    $"20{ano}{mes}"
                );
                
                Directory.CreateDirectory(diretorio);
                
                var nomeArquivo = $"{chaveAcesso}-nfe.xml";
                var caminhoCompleto = Path.Combine(diretorio, nomeArquivo);
                
                // Salvar XML
                await File.WriteAllTextAsync(caminhoCompleto, xml, Encoding.UTF8);
                
                // Criar backup compactado se configurado
                if (config.CompactarArquivos)
                {
                    await CriarBackupCompactado(caminhoCompleto, chaveAcesso);
                }
                
                _logger.LogInformation($"XML autorizado salvo: {caminhoCompleto}");
                
                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar XML autorizado");
                throw;
            }
        }

        public async Task<string> SalvarXmlRejeitado(string xml, string numeroLote, string cnpjEmitente, string motivo)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var dataHora = DateTime.Now;
                
                // Estrutura: XMLs/Rejeitados/CNPJ/AAMM/
                var diretorio = Path.Combine(
                    _basePath,
                    config.DiretorioXmlRejeitado,
                    cnpjEmitente,
                    dataHora.ToString("yyyyMM")
                );
                
                Directory.CreateDirectory(diretorio);
                
                var nomeArquivo = $"{numeroLote}-{dataHora:yyyyMMddHHmmss}-rejeitado.xml";
                var caminhoCompleto = Path.Combine(diretorio, nomeArquivo);
                
                // Adicionar motivo da rejeição ao XML como comentário
                var xmlComMotivo = $"<!-- Motivo da rejeição: {motivo} -->\n{xml}";
                
                await File.WriteAllTextAsync(caminhoCompleto, xmlComMotivo, Encoding.UTF8);
                
                _logger.LogWarning($"XML rejeitado salvo: {caminhoCompleto} - Motivo: {motivo}");
                
                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar XML rejeitado");
                throw;
            }
        }

        public async Task<string> SalvarXmlContingencia(string xml, string chaveAcesso, string cnpjEmitente)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var dataHora = DateTime.Now;
                
                var diretorio = Path.Combine(
                    _basePath,
                    config.DiretorioXmlContingencia,
                    cnpjEmitente,
                    dataHora.ToString("yyyyMM")
                );
                
                Directory.CreateDirectory(diretorio);
                
                var nomeArquivo = $"{chaveAcesso}-contingencia.xml";
                var caminhoCompleto = Path.Combine(diretorio, nomeArquivo);
                
                await File.WriteAllTextAsync(caminhoCompleto, xml, Encoding.UTF8);
                
                _logger.LogInformation($"XML de contingência salvo: {caminhoCompleto}");
                
                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar XML de contingência");
                throw;
            }
        }

        public async Task<string> SalvarPDF(byte[] pdfBytes, string chaveAcesso, string cnpjEmitente)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var ano = chaveAcesso.Substring(2, 2);
                var mes = chaveAcesso.Substring(4, 2);
                
                var diretorio = Path.Combine(
                    _basePath,
                    config.DiretorioPDF,
                    cnpjEmitente,
                    $"20{ano}{mes}"
                );
                
                Directory.CreateDirectory(diretorio);
                
                var nomeArquivo = $"{chaveAcesso}-danfe.pdf";
                var caminhoCompleto = Path.Combine(diretorio, nomeArquivo);
                
                await File.WriteAllBytesAsync(caminhoCompleto, pdfBytes);
                
                _logger.LogInformation($"PDF DANFE salvo: {caminhoCompleto}");
                
                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar PDF");
                throw;
            }
        }

        public async Task<string> SalvarCertificadoSeguro(byte[] certificadoBytes, string senha, string cnpjEmpresa)
        {
            try
            {
                var diretorio = Path.Combine(_basePath, "Certificados", cnpjEmpresa);
                Directory.CreateDirectory(diretorio);
                
                var nomeArquivo = $"{cnpjEmpresa}_cert.pfx.enc";
                var caminhoCompleto = Path.Combine(diretorio, nomeArquivo);
                
                // Criptografar certificado
                var certificadoCriptografado = CriptografarDados(certificadoBytes);
                await File.WriteAllBytesAsync(caminhoCompleto, certificadoCriptografado);
                
                // Salvar senha criptografada separadamente
                var senhaCriptografada = CriptografarTexto(senha);
                var caminhoSenha = Path.Combine(diretorio, $"{cnpjEmpresa}_cert.key");
                await File.WriteAllTextAsync(caminhoSenha, senhaCriptografada);
                
                _logger.LogInformation($"Certificado salvo com segurança para CNPJ: {cnpjEmpresa}");
                
                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar certificado");
                throw;
            }
        }

        public async Task<(byte[] certificado, string senha)> CarregarCertificadoSeguro(string cnpjEmpresa)
        {
            try
            {
                var diretorio = Path.Combine(_basePath, "Certificados", cnpjEmpresa);
                var caminhoCertificado = Path.Combine(diretorio, $"{cnpjEmpresa}_cert.pfx.enc");
                var caminhoSenha = Path.Combine(diretorio, $"{cnpjEmpresa}_cert.key");
                
                if (!File.Exists(caminhoCertificado) || !File.Exists(caminhoSenha))
                {
                    throw new FileNotFoundException("Certificado não encontrado");
                }
                
                // Descriptografar certificado
                var certificadoCriptografado = await File.ReadAllBytesAsync(caminhoCertificado);
                var certificado = DescriptografarDados(certificadoCriptografado);
                
                // Descriptografar senha
                var senhaCriptografada = await File.ReadAllTextAsync(caminhoSenha);
                var senha = DescriptografarTexto(senhaCriptografada);
                
                return (certificado, senha);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar certificado");
                throw;
            }
        }

        private async Task CriarBackupCompactado(string caminhoArquivo, string identificador)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var dataBackup = DateTime.Now;
                
                var diretorioBackup = Path.Combine(
                    _basePath,
                    config.DiretorioBackup,
                    dataBackup.ToString("yyyyMM")
                );
                
                Directory.CreateDirectory(diretorioBackup);
                
                var nomeBackup = $"backup_{identificador}_{dataBackup:yyyyMMddHHmmss}.zip";
                var caminhoBackup = Path.Combine(diretorioBackup, nomeBackup);
                
                using (var zip = ZipFile.Open(caminhoBackup, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(caminhoArquivo, Path.GetFileName(caminhoArquivo));
                }
                
                _logger.LogDebug($"Backup criado: {caminhoBackup}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup");
                // Não propagar erro de backup
            }
        }

        public async Task LimparArquivosAntigos()
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var dataLimite = DateTime.Now.AddDays(-config.TempoRetencao);
                
                _logger.LogInformation($"Iniciando limpeza de arquivos anteriores a {dataLimite:dd/MM/yyyy}");
                
                // Limpar XMLs autorizados antigos
                await LimparDiretorio(Path.Combine(_basePath, config.DiretorioXmlAutorizado), dataLimite);
                
                // Limpar PDFs antigos
                await LimparDiretorio(Path.Combine(_basePath, config.DiretorioPDF), dataLimite);
                
                // Limpar backups antigos (manter por mais tempo)
                var dataLimiteBackup = DateTime.Now.AddDays(-config.TempoRetencao * 2);
                await LimparDiretorio(Path.Combine(_basePath, config.DiretorioBackup), dataLimiteBackup);
                
                _logger.LogInformation("Limpeza de arquivos concluída");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza de arquivos");
            }
        }

        private async Task LimparDiretorio(string diretorio, DateTime dataLimite)
        {
            if (!Directory.Exists(diretorio))
                return;
            
            var arquivos = Directory.GetFiles(diretorio, "*", SearchOption.AllDirectories)
                .Where(f => File.GetCreationTime(f) < dataLimite)
                .ToList();
            
            foreach (var arquivo in arquivos)
            {
                try
                {
                    File.Delete(arquivo);
                    _logger.LogDebug($"Arquivo removido: {arquivo}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Não foi possível remover arquivo: {arquivo}");
                }
            }
            
            // Remover diretórios vazios
            RemoverDiretoriosVazios(diretorio);
        }

        private void RemoverDiretoriosVazios(string diretorio)
        {
            foreach (var subDir in Directory.GetDirectories(diretorio))
            {
                RemoverDiretoriosVazios(subDir);
                
                if (!Directory.EnumerateFileSystemEntries(subDir).Any())
                {
                    try
                    {
                        Directory.Delete(subDir);
                        _logger.LogDebug($"Diretório vazio removido: {subDir}");
                    }
                    catch { }
                }
            }
        }

        private byte[] CriptografarDados(byte[] dados)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.GenerateIV();
                
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // Escrever IV no início
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(dados, 0, dados.Length);
                        cs.FlushFinalBlock();
                    }
                    
                    return ms.ToArray();
                }
            }
        }

        private byte[] DescriptografarDados(byte[] dadosCriptografados)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                
                // Extrair IV do início
                var iv = new byte[aes.IV.Length];
                Array.Copy(dadosCriptografados, 0, iv, 0, iv.Length);
                aes.IV = iv;
                
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(dadosCriptografados, iv.Length, dadosCriptografados.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var output = new MemoryStream())
                {
                    cs.CopyTo(output);
                    return output.ToArray();
                }
            }
        }

        private string CriptografarTexto(string texto)
        {
            var bytes = Encoding.UTF8.GetBytes(texto);
            var criptografado = CriptografarDados(bytes);
            return Convert.ToBase64String(criptografado);
        }

        private string DescriptografarTexto(string textoCriptografado)
        {
            var bytes = Convert.FromBase64String(textoCriptografado);
            var descriptografado = DescriptografarDados(bytes);
            return Encoding.UTF8.GetString(descriptografado);
        }

        public async Task<List<ArquivoInfo>> ListarArquivos(string cnpj, TipoArquivo tipo, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var config = _configuracaoService.ObterConfiguracoesArmazenamento();
            var diretorio = tipo switch
            {
                TipoArquivo.XmlAutorizado => Path.Combine(_basePath, config.DiretorioXmlAutorizado, cnpj),
                TipoArquivo.XmlRejeitado => Path.Combine(_basePath, config.DiretorioXmlRejeitado, cnpj),
                TipoArquivo.XmlContingencia => Path.Combine(_basePath, config.DiretorioXmlContingencia, cnpj),
                TipoArquivo.PDF => Path.Combine(_basePath, config.DiretorioPDF, cnpj),
                _ => throw new ArgumentException("Tipo de arquivo inválido")
            };

            if (!Directory.Exists(diretorio))
                return new List<ArquivoInfo>();

            var arquivos = Directory.GetFiles(diretorio, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .Where(f => 
                    (!dataInicio.HasValue || f.CreationTime >= dataInicio.Value) &&
                    (!dataFim.HasValue || f.CreationTime <= dataFim.Value))
                .Select(f => new ArquivoInfo
                {
                    NomeArquivo = f.Name,
                    CaminhoCompleto = f.FullName,
                    Tamanho = f.Length,
                    DataCriacao = f.CreationTime,
                    DataModificacao = f.LastWriteTime
                })
                .OrderByDescending(f => f.DataCriacao)
                .ToList();

            return arquivos;
        }

        #region Métodos de Contingência

        public async Task<List<XmlContingenciaInfo>> ListarXmlsContingencia(string cnpjEmitente)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var diretorioContingencia = Path.Combine(_basePath, config.DiretorioXmlContingencia, cnpjEmitente);
                
                var xmlsContingencia = new List<XmlContingenciaInfo>();
                
                if (!Directory.Exists(diretorioContingencia))
                {
                    return xmlsContingencia;
                }

                var arquivosXml = Directory.GetFiles(diretorioContingencia, "*.xml", SearchOption.AllDirectories)
                    .Where(f => f.Contains("-contingencia.xml"))
                    .OrderBy(f => File.GetCreationTime(f));

                foreach (var arquivo in arquivosXml)
                {
                    try
                    {
                        var info = await ExtrairInformacoesXmlContingencia(arquivo);
                        if (info != null)
                        {
                            xmlsContingencia.Add(info);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Erro ao processar arquivo de contingência: {arquivo}");
                    }
                }

                return xmlsContingencia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar XMLs de contingência");
                return new List<XmlContingenciaInfo>();
            }
        }

        public async Task MoverXmlContingenciaParaProcessado(string caminhoArquivo, string chaveAcesso, string cnpjEmitente, string protocolo)
        {
            try
            {
                var config = _configuracaoService.ObterConfiguracoesArmazenamento();
                var dataHora = DateTime.Now;
                
                // Diretório de processados
                var diretorioProcessados = Path.Combine(
                    _basePath,
                    config.DiretorioXmlAutorizado,
                    cnpjEmitente,
                    dataHora.ToString("yyyyMM")
                );
                
                Directory.CreateDirectory(diretorioProcessados);
                
                var nomeArquivoProcessado = $"{chaveAcesso}-{protocolo}-autorizado.xml";
                var caminhoProcessado = Path.Combine(diretorioProcessados, nomeArquivoProcessado);
                
                // Ler conteúdo original e adicionar informações de processamento
                var xmlOriginal = await File.ReadAllTextAsync(caminhoArquivo);
                var xmlProcessado = $@"<!-- 
    Processado de Contingência em: {dataHora:yyyy-MM-dd HH:mm:ss}
    Protocolo de Autorização: {protocolo}
    Arquivo Original: {Path.GetFileName(caminhoArquivo)}
-->
{xmlOriginal}";

                await File.WriteAllTextAsync(caminhoProcessado, xmlProcessado, Encoding.UTF8);
                
                // Remover arquivo de contingência
                File.Delete(caminhoArquivo);
                
                _logger.LogInformation($"XML de contingência movido para processados: {chaveAcesso}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao mover XML de contingência para processados: {chaveAcesso}");
                throw;
            }
        }

        public async Task IncrementarTentativasContingencia(string caminhoArquivo, string ultimoErro)
        {
            try
            {
                var conteudo = await File.ReadAllTextAsync(caminhoArquivo);
                
                // Extrair número atual de tentativas
                var regexTentativas = new System.Text.RegularExpressions.Regex(@"Tentativas de Reenvio: (\d+)");
                var match = regexTentativas.Match(conteudo);
                
                int tentativasAtuais = 0;
                if (match.Success)
                {
                    tentativasAtuais = int.Parse(match.Groups[1].Value);
                }
                
                tentativasAtuais++;
                
                // Atualizar conteúdo
                var dataHora = DateTime.Now;
                var novoConteudo = conteudo;
                
                if (match.Success)
                {
                    novoConteudo = regexTentativas.Replace(novoConteudo, $"Tentativas de Reenvio: {tentativasAtuais}");
                }
                
                // Adicionar informações da última tentativa
                var infoTentativa = $@"
    Última Tentativa: {dataHora:yyyy-MM-dd HH:mm:ss}
    Último Erro: {ultimoErro}";
                
                var indiceFimComentario = novoConteudo.IndexOf("-->");
                if (indiceFimComentario > 0)
                {
                    novoConteudo = novoConteudo.Insert(indiceFimComentario, infoTentativa);
                }
                
                await File.WriteAllTextAsync(caminhoArquivo, novoConteudo, Encoding.UTF8);
                
                _logger.LogWarning($"Tentativas de reenvio incrementadas para {tentativasAtuais}: {Path.GetFileName(caminhoArquivo)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao incrementar tentativas de contingência: {caminhoArquivo}");
            }
        }

        private async Task<XmlContingenciaInfo?> ExtrairInformacoesXmlContingencia(string caminhoArquivo)
        {
            try
            {
                var nomeArquivo = Path.GetFileName(caminhoArquivo);
                var fileInfo = new FileInfo(caminhoArquivo);
                
                // Extrair chave de acesso do nome do arquivo
                var partesNome = nomeArquivo.Split('-');
                if (partesNome.Length < 2)
                {
                    return null;
                }
                
                var chaveAcesso = partesNome[0];
                
                // Ler conteúdo para extrair metadados
                var conteudo = await File.ReadAllTextAsync(caminhoArquivo);
                
                // Extrair informações dos comentários
                var tentativas = ExtrairTentativasDoComentario(conteudo);
                var ultimoErro = ExtrairUltimoErroDoComentario(conteudo);
                var ultimaTentativa = ExtrairUltimaTentativaDoComentario(conteudo);
                
                // Determinar UF e tipo de contingência do XML
                var (uf, homologacao) = ExtrairUFEAmbienteDoXml(conteudo);
                var tipoContingencia = DeterminarTipoContingencia(conteudo);
                
                return new XmlContingenciaInfo
                {
                    ChaveAcesso = chaveAcesso,
                    CaminhoArquivo = caminhoArquivo,
                    UF = uf,
                    Homologacao = homologacao,
                    DataCriacao = fileInfo.CreationTime,
                    TentativasReenvio = tentativas,
                    UltimaTentativa = ultimaTentativa,
                    UltimoErro = ultimoErro,
                    TipoContingencia = tipoContingencia,
                    TamanhoArquivo = fileInfo.Length,
                    CnpjEmitente = ExtrairCnpjDoArquivo(caminhoArquivo)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao extrair informações do XML de contingência: {caminhoArquivo}");
                return null;
            }
        }

        private int ExtrairTentativasDoComentario(string conteudo)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"Tentativas de Reenvio: (\d+)");
            var match = regex.Match(conteudo);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private string? ExtrairUltimoErroDoComentario(string conteudo)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"Último Erro: (.+)");
            var match = regex.Match(conteudo);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private DateTime? ExtrairUltimaTentativaDoComentario(string conteudo)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"Última Tentativa: ([\d\-\s:]+)");
            var match = regex.Match(conteudo);
            
            if (match.Success && DateTime.TryParse(match.Groups[1].Value.Trim(), out var data))
            {
                return data;
            }
            
            return null;
        }

        private (string uf, bool homologacao) ExtrairUFEAmbienteDoXml(string conteudo)
        {
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(conteudo);
                
                // Tentar extrair UF do emitente
                var ufNode = doc.SelectSingleNode("//enderEmit/UF") ?? doc.SelectSingleNode("//UF");
                var uf = ufNode?.InnerText ?? "RJ"; // Default para RJ
                
                // Tentar extrair ambiente
                var ambienteNode = doc.SelectSingleNode("//tpAmb");
                var ambiente = ambienteNode?.InnerText ?? "2"; // Default homologação
                
                return (uf, ambiente == "2");
            }
            catch
            {
                return ("RJ", true); // Default seguro
            }
        }

        private string DeterminarTipoContingencia(string conteudo)
        {
            if (conteudo.Contains("tpEmis>8<"))
                return "EPEC";
            else if (conteudo.Contains("tpEmis>2<"))
                return "FSDA";
            else if (conteudo.Contains("tpEmis>6<"))
                return "SVCAN";
            else
                return "DESCONHECIDO";
        }

        private string ExtrairCnpjDoArquivo(string caminhoArquivo)
        {
            // Extrair CNPJ do caminho do arquivo
            var partes = caminhoArquivo.Split(Path.DirectorySeparatorChar);
            var indiceCnpj = Array.FindIndex(partes, p => p.Length == 14 && p.All(char.IsDigit));
            
            return indiceCnpj >= 0 ? partes[indiceCnpj] : "";
        }

        #endregion
    }

    public enum TipoArquivo
    {
        XmlAutorizado,
        XmlRejeitado,
        XmlContingencia,
        PDF
    }

    public class ArquivoInfo
    {
        public string NomeArquivo { get; set; }
        public string CaminhoCompleto { get; set; }
        public long Tamanho { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}
