using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace nfse_backend.Services.Security
{
    /// <summary>
    /// Serviço para gerenciamento seguro de segredos e configurações sensíveis
    /// Suporta múltiplos provedores: arquivo criptografado, Azure Key Vault, AWS Secrets Manager, etc.
    /// </summary>
    public class SecretsManagerService
    {
        private readonly ILogger<SecretsManagerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _secretsCache;
        private readonly object _lockCache = new object();
        private readonly string _secretsPath;
        private readonly byte[] _encryptionKey;

        public SecretsManagerService(ILogger<SecretsManagerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _secretsCache = new Dictionary<string, string>();
            _secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets", "encrypted_secrets.dat");
            _encryptionKey = DerivarChaveCriptografia();
            
            InicializarArmazenamentoSegredos();
        }

        /// <summary>
        /// Obtém um segredo de forma segura
        /// </summary>
        public async Task<string?> ObterSegredo(string chave)
        {
            try
            {
                // Verificar cache primeiro
                lock (_lockCache)
                {
                    if (_secretsCache.ContainsKey(chave))
                    {
                        return _secretsCache[chave];
                    }
                }

                // Tentar obter de diferentes provedores na ordem de prioridade
                string? valor = null;

                // 1. Variáveis de ambiente (maior prioridade)
                valor = Environment.GetEnvironmentVariable($"NFE_SECRET_{chave.ToUpper()}");
                if (!string.IsNullOrEmpty(valor))
                {
                    _logger.LogDebug($"Segredo obtido de variável de ambiente: {chave}");
                    AdicionarAoCache(chave, valor);
                    return valor;
                }

                // 2. Azure Key Vault (se configurado)
                valor = await ObterDeAzureKeyVault(chave);
                if (!string.IsNullOrEmpty(valor))
                {
                    _logger.LogDebug($"Segredo obtido do Azure Key Vault: {chave}");
                    AdicionarAoCache(chave, valor);
                    return valor;
                }

                // 3. Arquivo criptografado local
                valor = await ObterDeArquivoCriptografado(chave);
                if (!string.IsNullOrEmpty(valor))
                {
                    _logger.LogDebug($"Segredo obtido de arquivo criptografado: {chave}");
                    AdicionarAoCache(chave, valor);
                    return valor;
                }

                _logger.LogWarning($"Segredo não encontrado: {chave}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao obter segredo: {chave}");
                return null;
            }
        }

        /// <summary>
        /// Armazena um segredo de forma segura
        /// </summary>
        public async Task<bool> ArmazenarSegredo(string chave, string valor)
        {
            try
            {
                // Armazenar em arquivo criptografado
                var sucesso = await ArmazenarEmArquivoCriptografado(chave, valor);
                
                if (sucesso)
                {
                    // Atualizar cache
                    AdicionarAoCache(chave, valor);
                    _logger.LogInformation($"Segredo armazenado com sucesso: {chave}");
                }

                return sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao armazenar segredo: {chave}");
                return false;
            }
        }

        /// <summary>
        /// Remove um segredo
        /// </summary>
        public async Task<bool> RemoverSegredo(string chave)
        {
            try
            {
                var sucesso = await RemoverDeArquivoCriptografado(chave);
                
                if (sucesso)
                {
                    // Remover do cache
                    lock (_lockCache)
                    {
                        _secretsCache.Remove(chave);
                    }
                    
                    _logger.LogInformation($"Segredo removido: {chave}");
                }

                return sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover segredo: {chave}");
                return false;
            }
        }

        /// <summary>
        /// Lista todas as chaves de segredos disponíveis (sem os valores)
        /// </summary>
        public async Task<List<string>> ListarChavesSegredos()
        {
            try
            {
                var segredos = await CarregarSegredosDoArquivo();
                return new List<string>(segredos.Keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar chaves de segredos");
                return new List<string>();
            }
        }

        /// <summary>
        /// Verifica se um segredo existe
        /// </summary>
        public async Task<bool> SegredoExiste(string chave)
        {
            var valor = await ObterSegredo(chave);
            return !string.IsNullOrEmpty(valor);
        }

        /// <summary>
        /// Rotaciona a chave de criptografia (re-criptografa todos os segredos)
        /// </summary>
        public async Task<bool> RotacionarChaveCriptografia()
        {
            try
            {
                _logger.LogInformation("Iniciando rotação da chave de criptografia");

                // Carregar todos os segredos com a chave atual
                var segredosAtuais = await CarregarSegredosDoArquivo();
                
                // Gerar nova chave
                var novaChave = GerarNovaChaveCriptografia();
                
                // Backup do arquivo atual
                var backupPath = $"{_secretsPath}.backup.{DateTime.Now:yyyyMMddHHmmss}";
                if (File.Exists(_secretsPath))
                {
                    File.Copy(_secretsPath, backupPath);
                }

                try
                {
                    // Re-criptografar com nova chave
                    await SalvarSegredosNoArquivo(segredosAtuais, novaChave);
                    
                    // Atualizar chave em uso
                    await SalvarChaveCriptografia(novaChave);
                    
                    // Limpar cache
                    lock (_lockCache)
                    {
                        _secretsCache.Clear();
                    }

                    _logger.LogInformation("Rotação da chave de criptografia concluída com sucesso");
                    return true;
                }
                catch
                {
                    // Restaurar backup em caso de erro
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, _secretsPath, true);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na rotação da chave de criptografia");
                return false;
            }
        }

        #region Métodos Privados

        private void InicializarArmazenamentoSegredos()
        {
            try
            {
                var diretorio = Path.GetDirectoryName(_secretsPath);
                if (!string.IsNullOrEmpty(diretorio))
                {
                    Directory.CreateDirectory(diretorio);
                }

                // Criar arquivo vazio se não existir
                if (!File.Exists(_secretsPath))
                {
                    var segredosVazios = new Dictionary<string, string>();
                    SalvarSegredosNoArquivo(segredosVazios, _encryptionKey).Wait();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar armazenamento de segredos");
            }
        }

        private byte[] DerivarChaveCriptografia()
        {
            try
            {
                // Tentar carregar chave existente
                var keyPath = Path.Combine(Path.GetDirectoryName(_secretsPath) ?? "", ".master.key");
                
                if (File.Exists(keyPath))
                {
                    var keyData = File.ReadAllText(keyPath);
                    return Convert.FromBase64String(keyData);
                }
                else
                {
                    // Gerar nova chave
                    var novaChave = GerarNovaChaveCriptografia();
                    SalvarChaveCriptografia(novaChave).Wait();
                    return novaChave;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao derivar chave de criptografia");
                
                // Fallback: gerar chave baseada em dados da máquina
                var machineData = Environment.MachineName + Environment.UserName + Environment.OSVersion.ToString();
                using (var sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineData));
                }
            }
        }

        private byte[] GerarNovaChaveCriptografia()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }

        private async Task SalvarChaveCriptografia(byte[] chave)
        {
            var keyPath = Path.Combine(Path.GetDirectoryName(_secretsPath) ?? "", ".master.key");
            var keyData = Convert.ToBase64String(chave);
            await File.WriteAllTextAsync(keyPath, keyData);
            
            // Tornar arquivo oculto e somente leitura
            if (File.Exists(keyPath))
            {
                File.SetAttributes(keyPath, FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
        }

        private void AdicionarAoCache(string chave, string valor)
        {
            lock (_lockCache)
            {
                _secretsCache[chave] = valor;
            }
        }

        private async Task<string?> ObterDeAzureKeyVault(string chave)
        {
            // TODO: Implementar integração com Azure Key Vault
            // Exemplo usando Azure.Security.KeyVault.Secrets
            /*
            var keyVaultUrl = _configuration["AzureKeyVault:VaultUrl"];
            if (string.IsNullOrEmpty(keyVaultUrl))
                return null;

            try
            {
                var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                var secret = await client.GetSecretAsync(chave);
                return secret.Value.Value;
            }
            catch
            {
                return null;
            }
            */
            
            return null; // Por enquanto não implementado
        }

        private async Task<string?> ObterDeArquivoCriptografado(string chave)
        {
            try
            {
                var segredos = await CarregarSegredosDoArquivo();
                return segredos.ContainsKey(chave) ? segredos[chave] : null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> ArmazenarEmArquivoCriptografado(string chave, string valor)
        {
            try
            {
                var segredos = await CarregarSegredosDoArquivo();
                segredos[chave] = valor;
                await SalvarSegredosNoArquivo(segredos, _encryptionKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> RemoverDeArquivoCriptografado(string chave)
        {
            try
            {
                var segredos = await CarregarSegredosDoArquivo();
                var removido = segredos.Remove(chave);
                
                if (removido)
                {
                    await SalvarSegredosNoArquivo(segredos, _encryptionKey);
                }
                
                return removido;
            }
            catch
            {
                return false;
            }
        }

        private async Task<Dictionary<string, string>> CarregarSegredosDoArquivo()
        {
            try
            {
                if (!File.Exists(_secretsPath))
                {
                    return new Dictionary<string, string>();
                }

                var dadosCriptografados = await File.ReadAllBytesAsync(_secretsPath);
                var dadosDescriptografados = DescriptografarDados(dadosCriptografados, _encryptionKey);
                var json = Encoding.UTF8.GetString(dadosDescriptografados);
                
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar segredos do arquivo");
                return new Dictionary<string, string>();
            }
        }

        private async Task SalvarSegredosNoArquivo(Dictionary<string, string> segredos, byte[] chave)
        {
            try
            {
                var json = JsonSerializer.Serialize(segredos, new JsonSerializerOptions { WriteIndented = false });
                var dados = Encoding.UTF8.GetBytes(json);
                var dadosCriptografados = CriptografarDados(dados, chave);
                
                await File.WriteAllBytesAsync(_secretsPath, dadosCriptografados);
                
                // Definir permissões restritivas
                if (File.Exists(_secretsPath))
                {
                    File.SetAttributes(_secretsPath, FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar segredos no arquivo");
                throw;
            }
        }

        private byte[] CriptografarDados(byte[] dados, byte[] chave)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = chave;
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

        private byte[] DescriptografarDados(byte[] dadosCriptografados, byte[] chave)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = chave;
                
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

        #endregion
    }

    /// <summary>
    /// Chaves padronizadas para segredos do sistema NFe
    /// </summary>
    public static class SecretKeys
    {
        public const string CERTIFICADO_SENHA = "CERTIFICADO_SENHA";
        public const string CERTIFICADO_PATH = "CERTIFICADO_PATH";
        public const string DATABASE_PASSWORD = "DATABASE_PASSWORD";
        public const string JWT_SECRET = "JWT_SECRET";
        public const string AZURE_KEYVAULT_URL = "AZURE_KEYVAULT_URL";
        public const string SMTP_PASSWORD = "SMTP_PASSWORD";
        public const string API_KEY_EXTERNAL = "API_KEY_EXTERNAL";
    }
}
