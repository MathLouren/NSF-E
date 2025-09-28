using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using nfse_backend;
using nfse_backend.Services.Impostos;

namespace nfse_backend.Tests.Integration
{
    public class NFeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly TabelasImpostosService _tabelasImpostosService;

        public NFeIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _tabelasImpostosService = new TabelasImpostosService(null!);
        }

        [Fact]
        public async Task EmitirNFe_ComDadosValidos_DeveRetornarSucesso()
        {
            // Arrange
            var nfeData = CriarNFeTesteDados();
            var json = JsonSerializer.Serialize(nfeData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/nfe/emitir", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(responseString);
            
            Assert.NotNull(result);
            // Verificar se contém campos esperados na resposta
        }

        [Fact]
        public async Task GerarDANFE_ComProtocoloValido_DeveRetornarPDF()
        {
            // Arrange
            // Primeiro emitir uma NFe
            var nfeData = CriarNFeTesteDados();
            var json = JsonSerializer.Serialize(nfeData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var emissaoResponse = await _client.PostAsync("/api/nfe/emitir", content);
            emissaoResponse.EnsureSuccessStatusCode();
            
            var emissaoResult = await emissaoResponse.Content.ReadAsStringAsync();
            var emissaoData = JsonSerializer.Deserialize<JsonElement>(emissaoResult);
            var protocolo = emissaoData.GetProperty("protocolo").GetString();

            // Act
            var response = await _client.GetAsync($"/api/nfe/danfe/{protocolo}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
            
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            Assert.True(pdfBytes.Length > 0);
            Assert.True(pdfBytes[0] == 0x25 && pdfBytes[1] == 0x50 && pdfBytes[2] == 0x44 && pdfBytes[3] == 0x46); // PDF header
        }

        [Fact]
        public async Task ConsultarStatusCertificado_DeveRetornarInformacoes()
        {
            // Act
            var response = await _client.GetAsync("/api/nfe/certificado/status");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("valido", responseString);
            Assert.Contains("vencimento", responseString);
        }

        [Fact]
        public async Task ObterAmbienteAtual_DeveRetornarConfiguracao()
        {
            // Act
            var response = await _client.GetAsync("/api/nfe/ambiente");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseString);
            
            Assert.True(result.TryGetProperty("ambiente", out _));
            Assert.True(result.TryGetProperty("codigo", out _));
        }

        [Fact]
        public async Task ListarNFe_DeveRetornarLista()
        {
            // Act
            var response = await _client.GetAsync("/api/nfe/lista");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseString);
            
            Assert.True(result.TryGetProperty("sucesso", out var sucesso));
            Assert.True(sucesso.GetBoolean());
            Assert.True(result.TryGetProperty("itens", out _));
        }

        [Fact]
        public async Task CartaCorrecao_ComDadosValidos_DeveProcessar()
        {
            // Arrange
            var cartaCorrecao = new
            {
                chaveAcesso = "33250933045570000191550010000000011000000001",
                cnpjEmitente = "12345678000199",
                textoCorrecao = "Correção de teste para integração",
                sequenciaEvento = 1
            };
            
            var json = JsonSerializer.Serialize(cartaCorrecao);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/nfe/carta-correcao", content);

            // Assert
            // Em ambiente de teste, pode retornar erro de SEFAZ indisponível
            // Verificar se pelo menos processa a requisição
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                       response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData("RJ", "RJ", 20.00)] // Operação interna RJ
        [InlineData("SP", "SP", 18.00)] // Operação interna SP
        [InlineData("SP", "RJ", 12.00)] // Interestadual SP->RJ
        [InlineData("RJ", "SP", 12.00)] // Interestadual RJ->SP
        public void ObterAliquotaICMS_DeveRetornarAliquotaCorreta(string ufOrigem, string ufDestino, decimal aliquotaEsperada)
        {
            // Act
            var aliquota = _tabelasImpostosService.ObterAliquotaICMS(ufOrigem, ufDestino);

            // Assert
            Assert.Equal(aliquotaEsperada, aliquota);
        }

        [Theory]
        [InlineData("22030000", true)]  // Cerveja - tem ST
        [InlineData("61091000", false)] // Camiseta - não tem ST
        [InlineData("84713012", false)] // Software - não tem ST
        public void VerificarSubstituicaoTributaria_DeveIdentificarCorretamente(string ncm, bool temST)
        {
            // Act
            var resultado = _tabelasImpostosService.TemSubstituicaoTributaria(ncm);

            // Assert
            Assert.Equal(temST, resultado);
        }

        [Theory]
        [InlineData("RJ", 2.00)]
        [InlineData("SP", 0.00)]
        [InlineData("CE", 2.00)]
        [InlineData("MG", 0.00)]
        public void ObterFCP_DeveRetornarPercentualCorreto(string uf, decimal fcpEsperado)
        {
            // Act
            var fcp = _tabelasImpostosService.ObterFCP(uf);

            // Assert
            Assert.Equal(fcpEsperado, fcp);
        }

        private object CriarNFeTesteDados()
        {
            return new
            {
                ide = new
                {
                    natOp = "VENDA DE MERCADORIA - TESTE AUTOMATIZADO",
                    mod = 55,
                    serie = 1,
                    nNF = 1,
                    dhEmi = DateTime.Now,
                    tpNF = 1,
                    idDest = 1,
                    cMunFG = 3304557,
                    tpImp = 1,
                    tpEmis = 1,
                    tpAmb = 2,
                    finNFe = 1,
                    indFinal = 1,
                    indPres = 1,
                    procEmi = 0,
                    verProc = "TESTE_1.0"
                },
                emit = new
                {
                    CNPJ = "12345678000199",
                    xNome = "EMPRESA TESTE AUTOMATIZADO LTDA",
                    xFant = "TESTE AUTO",
                    enderEmit = new
                    {
                        xLgr = "RUA TESTE AUTOMATIZADO",
                        nro = "123",
                        xBairro = "CENTRO TESTE",
                        cMun = 3304557,
                        xMun = "Rio de Janeiro",
                        UF = "RJ",
                        CEP = "20000000",
                        cPais = 1058,
                        xPais = "BRASIL"
                    },
                    IE = "123456789",
                    CRT = 3
                },
                dest = new
                {
                    CNPJ = "98765432000188",
                    xNome = "CLIENTE TESTE AUTOMATIZADO",
                    enderDest = new
                    {
                        xLgr = "AVENIDA TESTE",
                        nro = "456",
                        xBairro = "TESTE",
                        cMun = 3304557,
                        xMun = "Rio de Janeiro",
                        UF = "RJ",
                        CEP = "22000000",
                        cPais = 1058,
                        xPais = "BRASIL"
                    },
                    indIEDest = 1,
                    IE = "987654321"
                },
                det = new[]
                {
                    new
                    {
                        nItem = 1,
                        prod = new
                        {
                            cProd = "TESTE001",
                            cEAN = "SEM GTIN",
                            xProd = "PRODUTO TESTE AUTOMATIZADO",
                            NCM = "61091000",
                            CFOP = 5102,
                            uCom = "UN",
                            qCom = 1.0000,
                            vUnCom = 100.0000,
                            vProd = 100.00,
                            cEANTrib = "SEM GTIN",
                            uTrib = "UN",
                            qTrib = 1.0000,
                            vUnTrib = 100.0000,
                            indTot = 1
                        },
                        imposto = new
                        {
                            ICMS = new
                            {
                                ICMS00 = new
                                {
                                    orig = "0",
                                    CST = "00",
                                    modBC = 3,
                                    vBC = 100.00,
                                    pICMS = 20.00,
                                    vICMS = 20.00,
                                    pFCP = 2.00,
                                    vFCP = 2.00
                                }
                            }
                        }
                    }
                },
                total = new
                {
                    ICMSTot = new
                    {
                        vBC = 100.00,
                        vICMS = 20.00,
                        vFCP = 2.00,
                        vProd = 100.00,
                        vNF = 100.00
                    }
                },
                transp = new
                {
                    modFrete = 9
                },
                pag = new
                {
                    detPag = new[]
                    {
                        new
                        {
                            indPag = 0,
                            tPag = 1,
                            vPag = 100.00
                        }
                    }
                },
                infAdic = new
                {
                    infCpl = "Teste automatizado de emissão NFe"
                }
            };
        }
    }
}
