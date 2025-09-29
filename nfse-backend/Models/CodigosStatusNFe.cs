using System.Collections.Generic;

namespace nfse_backend.Models
{
    /// <summary>
    /// Códigos de status oficiais da NF-e conforme documentação da Receita Federal
    /// Baseado no Portal Nacional da NF-e e documentação oficial consultada via Context7
    /// </summary>
    public static class CodigosStatusNFe
    {
        #region Códigos de Sucesso

        /// <summary>
        /// NF-e autorizada
        /// </summary>
        public const string AUTORIZADA = "100";

        /// <summary>
        /// Lote de evento processado
        /// </summary>
        public const string LOTE_PROCESSADO = "128";

        /// <summary>
        /// Evento registrado e vinculado à NF-e
        /// </summary>
        public const string EVENTO_REGISTRADO = "135";

        /// <summary>
        /// Documento(s) localizado(s) - Distribuição DFe
        /// </summary>
        public const string DOCUMENTO_LOCALIZADO = "138";

        /// <summary>
        /// Serviço em Operação
        /// </summary>
        public const string SERVICO_EM_OPERACAO = "107";

        #endregion

        #region Códigos de Rejeição Comuns

        /// <summary>
        /// Duplicidade de NF-e (chave já existe)
        /// </summary>
        public const string DUPLICIDADE = "204";

        /// <summary>
        /// Falha no schema XML
        /// </summary>
        public const string FALHA_SCHEMA = "215";

        /// <summary>
        /// Data de emissão muito superior/inferior à data atual
        /// </summary>
        public const string DATA_EMISSAO_INVALIDA = "301";

        /// <summary>
        /// Evento não atende o Schema XML específico
        /// </summary>
        public const string EVENTO_SCHEMA_INVALIDO = "493";

        /// <summary>
        /// Chave de Acesso inexistente
        /// </summary>
        public const string CHAVE_INEXISTENTE = "494";

        /// <summary>
        /// Conteúdo do XML difere do schema / campo ausente
        /// </summary>
        public const string SCHEMA_XML_INVALIDO = "509";

        /// <summary>
        /// CNPJ/CPF do interessado não possui permissão para consultar esta NF-e
        /// </summary>
        public const string SEM_PERMISSAO_CONSULTA = "640";

        /// <summary>
        /// ICMS / Alíquota incompatível
        /// </summary>
        public const string ICMS_ALIQUOTA_INCOMPATIVEL = "878";

        #endregion

        #region Métodos Utilitários

        /// <summary>
        /// Verifica se o código indica sucesso
        /// </summary>
        public static bool IsSucesso(string codigo)
        {
            return codigo switch
            {
                AUTORIZADA => true,
                LOTE_PROCESSADO => true,
                EVENTO_REGISTRADO => true,
                DOCUMENTO_LOCALIZADO => true,
                SERVICO_EM_OPERACAO => true,
                _ => false
            };
        }

        /// <summary>
        /// Verifica se o código indica rejeição
        /// </summary>
        public static bool IsRejeicao(string codigo)
        {
            return !IsSucesso(codigo) && !string.IsNullOrEmpty(codigo);
        }

        /// <summary>
        /// Obtém descrição do código de status
        /// </summary>
        public static string ObterDescricao(string codigo)
        {
            return codigo switch
            {
                AUTORIZADA => "NF-e autorizada",
                LOTE_PROCESSADO => "Lote de evento processado",
                EVENTO_REGISTRADO => "Evento registrado e vinculado à NF-e",
                DOCUMENTO_LOCALIZADO => "Documento(s) localizado(s)",
                SERVICO_EM_OPERACAO => "Serviço em Operação",
                DUPLICIDADE => "Duplicidade de NF-e (chave já existe)",
                FALHA_SCHEMA => "Falha no schema XML",
                DATA_EMISSAO_INVALIDA => "Data de emissão muito superior/inferior à data atual",
                EVENTO_SCHEMA_INVALIDO => "Evento não atende o Schema XML específico",
                CHAVE_INEXISTENTE => "Chave de Acesso inexistente",
                SCHEMA_XML_INVALIDO => "Conteúdo do XML difere do schema / campo ausente",
                SEM_PERMISSAO_CONSULTA => "CNPJ/CPF do interessado não possui permissão para consultar esta NF-e",
                ICMS_ALIQUOTA_INCOMPATIVEL => "ICMS / Alíquota incompatível",
                _ => $"Código não catalogado: {codigo}"
            };
        }

        /// <summary>
        /// Verifica se o erro é recuperável (pode tentar novamente)
        /// </summary>
        public static bool IsErroRecuperavel(string codigo)
        {
            return codigo switch
            {
                // Erros temporários que podem ser tentados novamente
                "108" => true, // Serviço Paralisado Momentaneamente
                "109" => true, // Serviço Paralisado sem Previsão
                "999" => true, // Erro interno do servidor
                _ => false
            };
        }

        /// <summary>
        /// Verifica se o erro requer ação do usuário
        /// </summary>
        public static bool RequerAcaoUsuario(string codigo)
        {
            return codigo switch
            {
                FALHA_SCHEMA => true,
                DATA_EMISSAO_INVALIDA => true,
                EVENTO_SCHEMA_INVALIDO => true,
                SCHEMA_XML_INVALIDO => true,
                ICMS_ALIQUOTA_INCOMPATIVEL => true,
                _ => false
            };
        }

        /// <summary>
        /// Obtém categoria do erro
        /// </summary>
        public static CategoriaStatus ObterCategoria(string codigo)
        {
            if (IsSucesso(codigo))
                return CategoriaStatus.Sucesso;

            return codigo switch
            {
                DUPLICIDADE => CategoriaStatus.RejeicaoNegocio,
                CHAVE_INEXISTENTE => CategoriaStatus.RejeicaoNegocio,
                SEM_PERMISSAO_CONSULTA => CategoriaStatus.RejeicaoSeguranca,
                FALHA_SCHEMA => CategoriaStatus.RejeicaoTecnica,
                EVENTO_SCHEMA_INVALIDO => CategoriaStatus.RejeicaoTecnica,
                SCHEMA_XML_INVALIDO => CategoriaStatus.RejeicaoTecnica,
                DATA_EMISSAO_INVALIDA => CategoriaStatus.RejeicaoValidacao,
                ICMS_ALIQUOTA_INCOMPATIVEL => CategoriaStatus.RejeicaoValidacao,
                _ => CategoriaStatus.Desconhecida
            };
        }

        #endregion

        #region Códigos Específicos por Operação

        /// <summary>
        /// Códigos específicos para Autorização
        /// </summary>
        public static class Autorizacao
        {
            public const string LOTE_RECEBIDO = "103";
            public const string LOTE_PROCESSADO = "104";
            public const string LOTE_EM_PROCESSAMENTO = "105";
        }

        /// <summary>
        /// Códigos específicos para Eventos
        /// </summary>
        public static class Eventos
        {
            public const string CANCELAMENTO_REGISTRADO = "135";
            public const string CCE_REGISTRADA = "135";
            public const string EPEC_REGISTRADO = "135";
        }

        /// <summary>
        /// Códigos específicos para Consulta
        /// </summary>
        public static class Consulta
        {
            public const string NFE_CANCELADA = "101";
            public const string NFE_DENEGADA = "110";
        }

        #endregion

        /// <summary>
        /// Dicionário completo de códigos e descrições
        /// Baseado na documentação oficial consultada
        /// </summary>
        public static readonly Dictionary<string, string> TodosCodigos = new()
        {
            // Sucessos
            [AUTORIZADA] = "NF-e autorizada",
            ["101"] = "Cancelamento homologado",
            ["103"] = "Lote recebido com sucesso",
            ["104"] = "Lote processado",
            ["105"] = "Lote em processamento",
            [SERVICO_EM_OPERACAO] = "Serviço em Operação",
            ["110"] = "Uso Denegado",
            [LOTE_PROCESSADO] = "Lote de Evento Processado",
            [EVENTO_REGISTRADO] = "Evento registrado e vinculado à NF-e",
            [DOCUMENTO_LOCALIZADO] = "Documento(s) localizado(s)",

            // Rejeições
            [DUPLICIDADE] = "Duplicidade de NF-e",
            [FALHA_SCHEMA] = "Falha no schema XML",
            [DATA_EMISSAO_INVALIDA] = "Data de emissão inválida",
            [EVENTO_SCHEMA_INVALIDO] = "Evento não atende o Schema XML específico",
            [CHAVE_INEXISTENTE] = "Chave de Acesso inexistente",
            [SCHEMA_XML_INVALIDO] = "Conteúdo do XML difere do schema",
            [SEM_PERMISSAO_CONSULTA] = "CNPJ/CPF sem permissão para consultar esta NF-e",
            [ICMS_ALIQUOTA_INCOMPATIVEL] = "ICMS/Alíquota incompatível"
        };
    }

    /// <summary>
    /// Categorias de status
    /// </summary>
    public enum CategoriaStatus
    {
        Sucesso,
        RejeicaoTecnica,
        RejeicaoValidacao,
        RejeicaoNegocio,
        RejeicaoSeguranca,
        Desconhecida
    }
}
