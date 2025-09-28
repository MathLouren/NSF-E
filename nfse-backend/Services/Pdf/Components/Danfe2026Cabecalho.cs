using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;
using ZXing;
using ZXing.Common;
using QuestPDF.Drawing;
using SkiaSharp;

namespace nfse_backend.Services.Pdf
{
    public class Danfe2026Cabecalho : IComponent
    {
        private readonly NFe2026 _nfe;
        private readonly bool _isContingencia;

        public Danfe2026Cabecalho(NFe2026 nfe, bool isContingencia)
        {
            _nfe = nfe;
            _isContingencia = isContingencia;
        }

        public void Compose(IContainer container)
        {
            if (_nfe == null)
            {
                container.Text("ERRO: Objeto NFe não foi fornecido").FontSize(12).FontColor(Colors.Red.Medium);
                return;
            }

            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Coluna 1 - Dados do Emitente
                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Row(innerRow =>
                        {
                            var emitenteNome = _nfe?.Emit?.xNome ?? "EMITENTE NÃO INFORMADO";
                            innerRow.RelativeItem(1).Text("RECEBEMOS DE " + emitenteNome.ToUpper()).FontSize(6);
                        });
                        col.Item().Text("OS PRODUTOS E/OU SERVIÇOS CONSTANTES DA NOTA FISCAL ELETRÔNICA INDICADA ABAIXO").FontSize(6);
                        col.Item().PaddingTop(5).Row(innerRow =>
                        {
                            innerRow.RelativeItem(1).Column(c =>
                            {
                                c.Item().Text("DATA DE RECEBIMENTO").FontSize(6);
                                c.Item().Height(15).Border(0.5f);
                            });
                            innerRow.RelativeItem(3).PaddingLeft(5).Column(c =>
                            {
                                c.Item().Text("IDENTIFICAÇÃO E ASSINATURA DO RECEBEDOR").FontSize(6);
                                c.Item().Height(15).Border(0.5f);
                            });
                        });
                    });

                    // Coluna 2 - DANFE 2026
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().AlignCenter().Text("DANFE").Bold().FontSize(10);
                        col.Item().AlignCenter().Text("DOCUMENTO AUXILIAR DA").FontSize(7);
                        col.Item().AlignCenter().Text("NOTA FISCAL ELETRÔNICA").FontSize(7);
                        col.Item().AlignCenter().Text("REFORMA TRIBUTÁRIA 2026").FontSize(6).Bold().FontColor(Colors.Red.Medium);
                        col.Item().PaddingTop(5).AlignCenter().Text($"0 - ENTRADA").FontSize(7);
                        col.Item().AlignCenter().Text($"1 - SAÍDA").FontSize(7);
                        col.Item().AlignCenter().Border(1).Width(20).Height(15).AlignMiddle().Text((_nfe?.Ide?.tpNF ?? 1).ToString()).FontSize(9);
                        col.Item().PaddingTop(5).AlignCenter().Text($"Nº {(_nfe?.Ide?.nNF ?? 1):000000000}").Bold().FontSize(10);
                        col.Item().AlignCenter().Text($"SÉRIE {(_nfe?.Ide?.serie ?? 1):000}").Bold().FontSize(9);
                        col.Item().AlignCenter().Text($"Layout: {_nfe?.Versao ?? "4.00"}").FontSize(6).FontColor(Colors.Blue.Medium);
                    });

                    // Coluna 3 - Código de Barras
                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().AlignCenter().Element(container => GenerateBarcode(container, _nfe?.ChaveAcesso ?? ""));
                        col.Item().AlignCenter().Text($"CHAVE DE ACESSO").FontSize(6);
                        col.Item().AlignCenter().Text(FormatChaveAcesso(_nfe?.ChaveAcesso ?? "")).FontSize(8).FontFamily("Courier New");
                        
                        if (_isContingencia)
                        {
                            col.Item().PaddingTop(5).AlignCenter().Background(Colors.Yellow.Lighten3).Padding(2)
                                .Text("EMITIDA EM CONTINGÊNCIA").FontSize(8).Bold();
                        }

                        // Indicador de Reforma Tributária
                        col.Item().PaddingTop(5).AlignCenter().Background(Colors.Green.Lighten4).Padding(2)
                            .Text("REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold().FontColor(Colors.Green.Darken2);
                    });
                });

                // Segunda linha - Informações do Protocolo
                column.Item().Border(0.5f).Padding(2).Row(row =>
                {
                    row.RelativeItem(1).Text($"Consulta de autenticidade no portal nacional da NF-e www.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora").FontSize(7);
                });

                if (!string.IsNullOrEmpty(_nfe?.Protocolo))
                {
                    column.Item().Border(0.5f).Padding(2).Row(row =>
                    {
                        var dataAuth = _nfe?.DataAutorizacao?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
                        row.RelativeItem(1).Text($"PROTOCOLO DE AUTORIZAÇÃO DE USO: {_nfe?.Protocolo} - {dataAuth}").FontSize(8).Bold();
                    });
                }

                // Terceira linha - Natureza da Operação
                column.Item().Border(0.5f).Padding(2).Column(col =>
                {
                    col.Item().Text("NATUREZA DA OPERAÇÃO").FontSize(6);
                    col.Item().Text((_nfe?.Ide?.natOp ?? "VENDA").ToUpper()).FontSize(9);
                });

                // Quarta linha - Inscrições
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO ESTADUAL").FontSize(6);
                        col.Item().Text(_nfe?.Emit?.IE ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO ESTADUAL DO SUBST. TRIB.").FontSize(6);
                        col.Item().Text(_nfe?.Emit?.IEST ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO MUNICIPAL").FontSize(6);
                        col.Item().Text(_nfe?.Emit?.IM ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CNPJ").FontSize(6);
                        col.Item().Text(FormatCNPJ(_nfe?.Emit?.CNPJ ?? "")).FontSize(9);
                    });
                });

                // Quinta linha - CNAE e Regime Tributário
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CNAE FISCAL").FontSize(6);
                        col.Item().Text(_nfe?.Emit?.CNAE ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CÓDIGO DE REGIME TRIBUTÁRIO").FontSize(6);
                        col.Item().Text(FormatarCRT(_nfe?.Emit?.CRT ?? 3)).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CÓDIGO DO MUNICÍPIO DE OCORRÊNCIA").FontSize(6);
                        col.Item().Text((_nfe?.Ide?.cMunFG ?? 3550308).ToString()).FontSize(9);
                    });
                });
            });
        }

        private void GenerateBarcode(IContainer container, string chaveAcesso)
        {
            try
            {
                var writer = new BarcodeWriter<SKBitmap>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 50,
                        Width = 250,
                        Margin = 2
                    }
                };

                var barcodeBitmap = writer.Write(chaveAcesso);
                
                using (var stream = new MemoryStream())
                {
                    barcodeBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                    var imageBytes = stream.ToArray();
                    container.Height(50).Width(250).AlignCenter().Image(imageBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar código de barras: {ex.Message}");
                container.Height(50).AlignCenter().AlignMiddle().Text(chaveAcesso).FontSize(6).FontFamily("Courier New");
            }
        }

        private string FormatChaveAcesso(string chave)
        {
            if (string.IsNullOrEmpty(chave) || chave.Length != 44)
                return chave;

            return string.Join(" ", Enumerable.Range(0, 11)
                .Select(i => chave.Substring(i * 4, 4)));
        }

        private string FormatCNPJ(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14)
                return cnpj;

            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        private string FormatarCRT(int crt)
        {
            return crt switch
            {
                1 => "1 - Microempresa Municipal",
                2 => "2 - Estimativa",
                3 => "3 - Sociedade de Profissionais",
                4 => "4 - Cooperativa",
                5 => "5 - Microempresário Individual (MEI)",
                6 => "6 - Microempresário e Empresa de Pequeno Porte (ME/EPP)",
                _ => crt.ToString()
            };
        }
    }
}
