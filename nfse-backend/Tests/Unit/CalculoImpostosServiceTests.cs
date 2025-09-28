using Xunit;
using nfse_backend.Services.Impostos;
using nfse_backend.Models.NFe;
using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace nfse_backend.Tests.Unit
{
    public class CalculoImpostosServiceTests
    {
        private readonly CalculoImpostosService _calculoImpostosService;
        private readonly TabelasImpostosService _tabelasImpostosService;

        public CalculoImpostosServiceTests()
        {
            var mockLogger = new Mock<ILogger<TabelasImpostosService>>();
            _tabelasImpostosService = new TabelasImpostosService(mockLogger.Object);
            _calculoImpostosService = new CalculoImpostosService(_tabelasImpostosService);
        }

        [Fact]
        public void CalcularICMS_OperacaoInterna_RJ_DeveCalcularCorretamente()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m,
                NCM = "6109100000",
                CFOP = 5102
            };

            // Act
            var icms = _calculoImpostosService.CalcularICMS(produto, "RJ", "RJ", 3, true, false);

            // Assert
            Assert.Equal("00", icms.CST);
            Assert.Equal(20.00m, icms.pICMS); // RJ tem 20% interno
            Assert.Equal(20.00m, icms.vICMS);
            Assert.Equal(2.00m, icms.pFCP); // FCP RJ = 2%
            Assert.Equal(2.00m, icms.vFCP);
        }

        [Fact]
        public void CalcularICMS_OperacaoInterestadual_ConsumidorFinal_DeveCalcularDIFAL()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m,
                NCM = "6109100000",
                CFOP = 6102
            };

            // Act
            var icms = _calculoImpostosService.CalcularICMS(produto, "SP", "RJ", 3, true, false);

            // Assert
            Assert.Equal("00", icms.CST);
            Assert.Equal(12.00m, icms.pICMS); // Interestadual SP->RJ = 12%
            Assert.Equal(12.00m, icms.vICMS);
            Assert.Equal(8.00m, icms.vICMSUFDest); // DIFAL = 20% - 12% = 8%
            Assert.Equal(0m, icms.vICMSUFRemet);
            Assert.Equal(2.00m, icms.pFCP); // FCP RJ = 2%
            Assert.Equal(2.00m, icms.vFCP);
        }

        [Fact]
        public void CalcularICMS_SimplesNacional_DeveUsarCSOSN()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m,
                NCM = "6109100000",
                CFOP = 5102
            };

            // Act
            var icms = _calculoImpostosService.CalcularICMS(produto, "RJ", "RJ", 1, true, false);

            // Assert
            Assert.Equal("102", icms.CST); // CSOSN para Simples
            Assert.Equal(0m, icms.pICMS);
            Assert.Equal(0m, icms.vICMS);
        }

        [Fact]
        public void CalcularICMS_ComSubstituicaoTributaria_DeveCalcularST()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m,
                NCM = "22030000", // Cerveja - sujeita a ST
                CFOP = 5102
            };

            // Act
            var icms = _calculoImpostosService.CalcularICMS(produto, "RJ", "SP", 3, false, true);

            // Assert
            Assert.Equal("10", icms.CST); // Tributada com ST
            Assert.True(icms.vBCST > 0); // Deve ter base de cálculo ST
            Assert.True(icms.vICMSST >= 0); // Deve ter ICMS ST
        }

        [Fact]
        public void CalcularIPI_ProdutoTributado_DeveCalcularCorretamente()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m,
                NCM = "84713012" // Sujeito a IPI
            };

            // Act
            var ipi = _calculoImpostosService.CalcularIPI(produto, "84713012", true);

            // Assert
            Assert.Equal("00", ipi.CST);
            Assert.True(ipi.pIPI > 0);
            Assert.True(ipi.vIPI > 0);
        }

        [Fact]
        public void CalcularPIS_RegimeCumulativo_DeveUsarAliquotaCorreta()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m
            };

            // Act
            var pis = _calculoImpostosService.CalcularPIS(produto, 3, false); // Regime cumulativo

            // Assert
            Assert.Equal("01", pis.CST);
            Assert.Equal(0.65m, pis.pPIS);
            Assert.Equal(0.65m, pis.vPIS);
        }

        [Fact]
        public void CalcularCOFINS_RegimeNaoCumulativo_DeveUsarAliquotaCorreta()
        {
            // Arrange
            var produto = new ProdutoNFe
            {
                vProd = 100.00m
            };

            // Act
            var cofins = _calculoImpostosService.CalcularCOFINS(produto, 3, true); // Regime não cumulativo

            // Assert
            Assert.Equal("01", cofins.CST);
            Assert.Equal(7.60m, cofins.pCOFINS);
            Assert.Equal(7.60m, cofins.vCOFINS);
        }

        [Fact]
        public void CalcularTotaisNFe_DeveCalcularCorretamente()
        {
            // Arrange
            var nfe = new nfse_backend.Models.NFe.NFe
            {
                Det = new List<DetalheNFe>
                {
                    new DetalheNFe
                    {
                        Prod = new ProdutoNFe { vProd = 100.00m, indTot = 1 },
                        Imposto = new ImpostoNFe
                        {
                            ICMS = new ICMS { vBC = 100.00m, vICMS = 18.00m, vFCP = 2.00m },
                            IPI = new IPI { vIPI = 5.00m },
                            PIS = new PIS { vPIS = 0.65m },
                            COFINS = new COFINS { vCOFINS = 3.00m },
                            vTotTrib = 28.65m
                        }
                    }
                },
                Total = new TotalNFe { ICMSTot = new ICMSTotal() }
            };

            // Act
            _calculoImpostosService.CalcularTotaisNFe(nfe);

            // Assert
            Assert.Equal(100.00m, nfe.Total.ICMSTot.vProd);
            Assert.Equal(18.00m, nfe.Total.ICMSTot.vICMS);
            Assert.Equal(2.00m, nfe.Total.ICMSTot.vFCP);
            Assert.Equal(5.00m, nfe.Total.ICMSTot.vIPI);
            Assert.Equal(105.00m, nfe.Total.ICMSTot.vNF); // vProd + vIPI
        }
    }
}
