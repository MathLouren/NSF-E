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
