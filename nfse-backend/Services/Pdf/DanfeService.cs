using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using ZXing;
using ZXing.Common;
using QuestPDF.Drawing;
using SkiaSharp;

namespace nfse_backend.Services.Pdf
{
    public class DanfeService
    {
        private readonly string _fontFamily = "Arial";


        public byte[] GenerateDanfe(nfse_backend.Models.NFe.NFe nfe, bool isContingencia = false)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(5, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily(_fontFamily));

                    page.Content().Column(column =>
                    {
                        column.Spacing(2);

                        // Cabeçalho
                        column.Item().Component(new DanfeCabecalho(nfe, isContingencia));

                        // Destinatário/Remetente
                        column.Item().Component(new DanfeDestinatarioRemetente(nfe));

                        // Dados do Faturamento
                        if (nfe.Cobr != null && nfe.Cobr.Fat != null)
                        {
                            column.Item().Component(new DanfeFaturamento(nfe.Cobr));
                        }

                        // Cálculo do Imposto
                        column.Item().Component(new DanfeCalculoImposto(nfe.Total));

                        // Transportador/Volumes
                        column.Item().Component(new DanfeTransportador(nfe.Transp));

                        // Dados dos Produtos/Serviços
                        column.Item().Component(new DanfeProdutos(nfe.Det));

                        // ISSQN (se houver)
                        if (nfe.Total.ISSQNtot != null && nfe.Total.ISSQNtot.vServ.HasValue)
                        {
                            column.Item().Component(new DanfeISSQN(nfe.Total.ISSQNtot));
                        }

                        // Dados Adicionais
                        column.Item().Component(new DanfeInformacoesAdicionais(nfe.InfAdic));
                    });
                });
            });

            return document.GeneratePdf();
        }
    }

    public class DanfeCabecalho : IComponent
    {
        private readonly nfse_backend.Models.NFe.NFe _nfe;
        private readonly bool _isContingencia;

        public DanfeCabecalho(nfse_backend.Models.NFe.NFe nfe, bool isContingencia)
        {
            _nfe = nfe;
            _isContingencia = isContingencia;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Coluna 1 - Dados do Emitente
                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Row(innerRow =>
                        {
                            innerRow.RelativeItem(1).Text("RECEBEMOS DE " + _nfe.Emit.xNome.ToUpper()).FontSize(6);
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

                    // Coluna 2 - DANFE
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().AlignCenter().Text("DANFE").Bold().FontSize(10);
                        col.Item().AlignCenter().Text("DOCUMENTO AUXILIAR DA").FontSize(7);
                        col.Item().AlignCenter().Text("NOTA FISCAL ELETRÔNICA").FontSize(7);
                        col.Item().PaddingTop(5).AlignCenter().Text($"0 - ENTRADA").FontSize(7);
                        col.Item().AlignCenter().Text($"1 - SAÍDA").FontSize(7);
                        col.Item().AlignCenter().Border(1).Width(20).Height(15).AlignMiddle().Text(_nfe.Ide.tpNF.ToString()).FontSize(9);
                        col.Item().PaddingTop(5).AlignCenter().Text($"Nº {_nfe.Ide.nNF:000000000}").Bold().FontSize(10);
                        col.Item().AlignCenter().Text($"SÉRIE {_nfe.Ide.serie:000}").Bold().FontSize(9);
                    });

                    // Coluna 3 - Código de Barras
                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().AlignCenter().Element(container => GenerateBarcode(container, _nfe.ChaveAcesso));
                        col.Item().AlignCenter().Text($"CHAVE DE ACESSO").FontSize(6);
                        col.Item().AlignCenter().Text(FormatChaveAcesso(_nfe.ChaveAcesso)).FontSize(8).FontFamily("Courier New");
                        
                        if (_isContingencia)
                        {
                            col.Item().PaddingTop(5).AlignCenter().Background(Colors.Yellow.Lighten3).Padding(2)
                                .Text("EMITIDA EM CONTINGÊNCIA").FontSize(8).Bold();
                        }
                    });
                });

                // Segunda linha - Informações do Protocolo
                column.Item().Border(0.5f).Padding(2).Row(row =>
                {
                    row.RelativeItem(1).Text($"Consulta de autenticidade no portal nacional da NF-e www.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora").FontSize(7);
                });

                if (!string.IsNullOrEmpty(_nfe.Protocolo))
                {
                    column.Item().Border(0.5f).Padding(2).Row(row =>
                    {
                        row.RelativeItem(1).Text($"PROTOCOLO DE AUTORIZAÇÃO DE USO: {_nfe.Protocolo} - {_nfe.DataAutorizacao:dd/MM/yyyy HH:mm:ss}").FontSize(8).Bold();
                    });
                }

                // Terceira linha - Natureza da Operação
                column.Item().Border(0.5f).Padding(2).Column(col =>
                {
                    col.Item().Text("NATUREZA DA OPERAÇÃO").FontSize(6);
                    col.Item().Text(_nfe.Ide.natOp.ToUpper()).FontSize(9);
                });

                // Quarta linha - Inscrições
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO ESTADUAL").FontSize(6);
                        col.Item().Text(_nfe.Emit.IE ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO ESTADUAL DO SUBST. TRIB.").FontSize(6);
                        col.Item().Text(_nfe.Emit.IEST ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO MUNICIPAL").FontSize(6);
                        col.Item().Text(_nfe.Emit.IM ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CNPJ").FontSize(6);
                        col.Item().Text(FormatCNPJ(_nfe.Emit.CNPJ)).FontSize(9);
                    });
                });

                // Quinta linha - CNAE e Regime Tributário
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CNAE FISCAL").FontSize(6);
                        col.Item().Text(_nfe.Emit.CNAE ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CÓDIGO DE REGIME TRIBUTÁRIO").FontSize(6);
                        col.Item().Text(FormatarCRT(_nfe.Emit.CRT)).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CÓDIGO DO MUNICÍPIO DE OCORRÊNCIA").FontSize(6);
                        col.Item().Text(_nfe.Ide.cMunFG.ToString()).FontSize(9);
                    });
                });
            });
        }

        private void GenerateBarcode(IContainer container, string chaveAcesso)
        {
            try
            {
                // Gerar código de barras CODE-128C usando ZXing com SkiaSharp
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
                
                // Converter SKBitmap para bytes PNG e exibir
                using (var stream = new MemoryStream())
                {
                    barcodeBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                    var imageBytes = stream.ToArray();
                    container.Height(50).Width(250).AlignCenter().Image(imageBytes);
                }
            }
            catch (Exception ex)
            {
                // Fallback: renderiza apenas a chave como texto
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

    public class DanfeDestinatarioRemetente : IComponent
    {
        private readonly nfse_backend.Models.NFe.NFe _nfe;

        public DanfeDestinatarioRemetente(nfse_backend.Models.NFe.NFe nfe)
        {
            _nfe = nfe;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("DESTINATÁRIO/REMETENTE").FontSize(7).Bold();

                // Primeira linha - Nome e CNPJ
                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("NOME/RAZÃO SOCIAL").FontSize(6);
                        col.Item().Text(_nfe.Dest.xNome?.ToUpper() ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CNPJ/CPF").FontSize(6);
                        col.Item().Text(FormatDocumento(_nfe.Dest.CNPJ ?? _nfe.Dest.CPF)).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("DATA DA EMISSÃO").FontSize(6);
                        col.Item().Text(_nfe.Ide.dhEmi?.ToString("dd/MM/yyyy") ?? "").FontSize(9);
                    });
                });

                // Segunda linha - Endereço
                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("ENDEREÇO").FontSize(6);
                        var endereco = $"{_nfe.Dest.EnderDest.xLgr}, {_nfe.Dest.EnderDest.nro}";
                        if (!string.IsNullOrEmpty(_nfe.Dest.EnderDest.xCpl))
                            endereco += $" - {_nfe.Dest.EnderDest.xCpl}";
                        col.Item().Text(endereco.ToUpper()).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("BAIRRO/DISTRITO").FontSize(6);
                        col.Item().Text(_nfe.Dest.EnderDest.xBairro?.ToUpper() ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CEP").FontSize(6);
                        col.Item().Text(FormatCEP(_nfe.Dest.EnderDest.CEP)).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("DATA DA SAÍDA/ENTRADA").FontSize(6);
                        col.Item().Text(_nfe.Ide.dhSaiEnt?.ToString("dd/MM/yyyy") ?? "").FontSize(9);
                    });
                });

                // Terceira linha - Município, UF, IE
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("MUNICÍPIO").FontSize(6);
                        col.Item().Text(_nfe.Dest.EnderDest.xMun?.ToUpper() ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("FONE/FAX").FontSize(6);
                        col.Item().Text(FormatTelefone(_nfe.Dest.EnderDest.fone)).FontSize(9);
                    });

                    row.RelativeItem().Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("UF").FontSize(6);
                        col.Item().Text(_nfe.Dest.EnderDest.UF ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO ESTADUAL").FontSize(6);
                        col.Item().Text(_nfe.Dest.IE ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO MUNICIPAL").FontSize(6);
                        col.Item().Text(_nfe.Dest.IM ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("HORA DA SAÍDA").FontSize(6);
                        col.Item().Text(_nfe.Ide.dhSaiEnt?.ToString("HH:mm:ss") ?? (_nfe.Ide.tpNF == 1 ? DateTime.Now.ToString("HH:mm:ss") : "")).FontSize(9);
                    });
                });
            });
        }

        private string FormatDocumento(string? doc)
        {
            if (string.IsNullOrEmpty(doc))
                return "";

            if (doc.Length == 14) // CNPJ
                return $"{doc.Substring(0, 2)}.{doc.Substring(2, 3)}.{doc.Substring(5, 3)}/{doc.Substring(8, 4)}-{doc.Substring(12, 2)}";
            else if (doc.Length == 11) // CPF
                return $"{doc.Substring(0, 3)}.{doc.Substring(3, 3)}.{doc.Substring(6, 3)}-{doc.Substring(9, 2)}";

            return doc;
        }

        private string FormatCEP(string cep)
        {
            if (string.IsNullOrEmpty(cep) || cep.Length != 8)
                return cep;

            return $"{cep.Substring(0, 5)}-{cep.Substring(5, 3)}";
        }

        private string FormatTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone))
                return "";

            // Remove caracteres não numéricos
            telefone = System.Text.RegularExpressions.Regex.Replace(telefone, @"\D", "");

            if (telefone.Length == 11)
                return $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 5)}-{telefone.Substring(7, 4)}";
            else if (telefone.Length == 10)
                return $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 4)}-{telefone.Substring(6, 4)}";

            return telefone;
        }
    }

    public class DanfeFaturamento : IComponent
    {
        private readonly Cobranca _cobranca;

        public DanfeFaturamento(Cobranca cobranca)
        {
            _cobranca = cobranca;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("FATURA/DUPLICATAS").FontSize(7).Bold();

                if (_cobranca.Dup != null && _cobranca.Dup.Any())
                {
                    column.Item().Row(row =>
                    {
                        foreach (var dup in _cobranca.Dup.Take(6)) // Máximo 6 duplicatas na linha
                        {
                            row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem(1).Text($"Número: {dup.nDup}").FontSize(6);
                                    r.RelativeItem(1).Text($"Venc.: {dup.dVenc:dd/MM/yyyy}").FontSize(6);
                                    r.RelativeItem(1).Text($"Valor: {dup.vDup:N2}").FontSize(6);
                                });
                            });
                        }
                    });
                }
            });
        }
    }

    public class DanfeCalculoImposto : IComponent
    {
        private readonly TotalNFe _total;

        public DanfeCalculoImposto(TotalNFe total)
        {
            _total = total;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("CÁLCULO DO IMPOSTO").FontSize(7).Bold();

                // Primeira linha
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO ICMS").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vBC.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO ICMS").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vICMS.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO ICMS ST").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vBCST.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO ICMS ST").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vST.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO FCP").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vFCP.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR TOTAL DOS PRODUTOS").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vProd.ToString("N2")).FontSize(9).Bold();
                    });
                });

                // Segunda linha
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO FRETE").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vFrete.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO SEGURO").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vSeg.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("DESCONTO").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vDesc.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("OUTRAS DESPESAS ACESSÓRIAS").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vOutro.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO IPI").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vIPI.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR TOTAL DA NOTA").FontSize(6);
                        col.Item().Text(_total.ICMSTot.vNF.ToString("N2")).FontSize(10).Bold();
                    });
                });
            });
        }
    }

    public class DanfeTransportador : IComponent
    {
        private readonly Transporte _transporte;

        public DanfeTransportador(Transporte transporte)
        {
            _transporte = transporte;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("TRANSPORTADOR/VOLUMES TRANSPORTADOS").FontSize(7).Bold();

                // Primeira linha - Transportador
                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("RAZÃO SOCIAL").FontSize(6);
                        col.Item().Text(_transporte.Transporta?.xNome?.ToUpper() ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("FRETE POR CONTA").FontSize(6);
                        var fretePorConta = _transporte.modFrete switch
                        {
                            0 => "0-EMITENTE",
                            1 => "1-DESTINATÁRIO",
                            2 => "2-TERCEIROS",
                            3 => "3-PROP. REMETENTE",
                            4 => "4-PROP. DESTINATÁRIO",
                            9 => "9-SEM FRETE",
                            _ => ""
                        };
                        col.Item().Text(fretePorConta).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CÓDIGO ANTT").FontSize(6);
                        col.Item().Text(_transporte.VeicTransp?.RNTC ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("PLACA DO VEÍCULO").FontSize(6);
                        col.Item().Text(_transporte.VeicTransp?.placa ?? "").FontSize(9);
                    });

                    row.RelativeItem().Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("UF").FontSize(6);
                        col.Item().Text(_transporte.VeicTransp?.UF ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("CNPJ/CPF").FontSize(6);
                        col.Item().Text(FormatDocumento(_transporte.Transporta?.CNPJ ?? _transporte.Transporta?.CPF)).FontSize(9);
                    });
                });

                // Segunda linha - Endereço e IE
                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("ENDEREÇO").FontSize(6);
                        col.Item().Text(_transporte.Transporta?.xEnder?.ToUpper() ?? "").FontSize(9);
                    });

                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("MUNICÍPIO").FontSize(6);
                        col.Item().Text(_transporte.Transporta?.xMun?.ToUpper() ?? "").FontSize(9);
                    });

                    row.RelativeItem().Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("UF").FontSize(6);
                        col.Item().Text(_transporte.Transporta?.UF ?? "").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO ESTADUAL").FontSize(6);
                        col.Item().Text(_transporte.Transporta?.IE ?? "").FontSize(9);
                    });
                });

                // Terceira linha - Volumes
                if (_transporte.Vol != null && _transporte.Vol.Any())
                {
                    var vol = _transporte.Vol.First();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                        {
                            col.Item().Text("QUANTIDADE").FontSize(6);
                            col.Item().Text(vol.qVol?.ToString() ?? "").FontSize(9);
                        });

                        row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                        {
                            col.Item().Text("ESPÉCIE").FontSize(6);
                            col.Item().Text(vol.esp?.ToUpper() ?? "").FontSize(9);
                        });

                        row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                        {
                            col.Item().Text("MARCA").FontSize(6);
                            col.Item().Text(vol.marca?.ToUpper() ?? "").FontSize(9);
                        });

                        row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                        {
                            col.Item().Text("NUMERAÇÃO").FontSize(6);
                            col.Item().Text(vol.nVol ?? "").FontSize(9);
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                        {
                            col.Item().Text("PESO BRUTO").FontSize(6);
                            col.Item().Text(vol.pesoB?.ToString("N3") ?? "").FontSize(9);
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                        {
                            col.Item().Text("PESO LÍQUIDO").FontSize(6);
                            col.Item().Text(vol.pesoL?.ToString("N3") ?? "").FontSize(9);
                        });
                    });
                }
            });
        }

        private string FormatDocumento(string? doc)
        {
            if (string.IsNullOrEmpty(doc))
                return "";

            if (doc.Length == 14) // CNPJ
                return $"{doc.Substring(0, 2)}.{doc.Substring(2, 3)}.{doc.Substring(5, 3)}/{doc.Substring(8, 4)}-{doc.Substring(12, 2)}";
            else if (doc.Length == 11) // CPF
                return $"{doc.Substring(0, 3)}.{doc.Substring(3, 3)}.{doc.Substring(6, 3)}-{doc.Substring(9, 2)}";

            return doc;
        }
    }

    public class DanfeProdutos : IComponent
    {
        private readonly List<DetalheNFe> _produtos;

        public DanfeProdutos(List<DetalheNFe> produtos)
        {
            _produtos = produtos;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("DADOS DOS PRODUTOS/SERVIÇOS").FontSize(7).Bold();

                // Cabeçalho da tabela
                column.Item().Table(table =>
                {
                    // Define as colunas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // Código
                        columns.RelativeColumn(3);   // Descrição
                        columns.ConstantColumn(35);  // NCM
                        columns.ConstantColumn(25);  // CST
                        columns.ConstantColumn(30);  // CFOP
                        columns.ConstantColumn(25);  // UN
                        columns.ConstantColumn(40);  // Quant
                        columns.ConstantColumn(40);  // V.Unit
                        columns.ConstantColumn(40);  // V.Total
                        columns.ConstantColumn(35);  // BC ICMS
                        columns.ConstantColumn(30);  // V.ICMS
                        columns.ConstantColumn(30);  // V.FCP
                        columns.ConstantColumn(30);  // V.IPI
                        columns.ConstantColumn(25);  // %ICMS
                        columns.ConstantColumn(25);  // %IPI
                    });

                    // Cabeçalho
                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CÓDIGO").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("DESCRIÇÃO DO PRODUTO/SERVIÇO").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("NCM/SH").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CST").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CFOP").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("UN").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("QUANT").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.UNIT").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.TOTAL").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("BC ICMS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.ICMS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.FCP").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.IPI").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("%ICMS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("%IPI").FontSize(6);
                    });

                    // Linhas de produtos
                    foreach (var item in _produtos)
                    {
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.Prod.cProd ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(2).AlignLeft().Text(TruncarTexto(item.Prod.xProd ?? "", 40)).FontSize(7).FontFamily("Arial");
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.Prod.NCM ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(FormatarCSTComDescricao(item.Imposto.ICMS.CST ?? "", item.Imposto.ICMS.orig ?? "")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(FormatarCFOP(item.Prod.CFOP)).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.Prod.uCom ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(item.Prod.qCom.ToString("N4")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(item.Prod.vUnCom.ToString("N4")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(item.Prod.vProd.ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(item.Imposto.ICMS.vBC.ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(item.Imposto.ICMS.vICMS.ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item.Imposto.ICMS.vFCP > 0 ? item.Imposto.ICMS.vFCP.ToString("N2") : "")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item.Imposto.IPI?.vIPI > 0 ? item.Imposto.IPI.vIPI.Value.ToString("N2") : "")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.Imposto.ICMS.pICMS.ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text((item.Imposto.IPI?.pIPI ?? 0m).ToString("N2")).FontSize(7);
                    }
                });
            });
        }

        private string FormatarCST(string cst, string origem)
        {
            if (string.IsNullOrEmpty(cst))
                return "";

            // Para CST (Regime Normal) - formato: 000
            if (cst.Length == 2)
            {
                return $"{origem}{cst}";
            }
            
            // Para CSOSN (Simples Nacional) - formato: 102
            return cst;
        }

        private string FormatarCSTComDescricao(string cst, string origem)
        {
            if (string.IsNullOrEmpty(cst))
                return "";

            var cstCompleto = cst.Length == 2 ? $"{origem}{cst}" : cst;
            
            return cstCompleto switch
            {
                "000" => "00 - Tributada integralmente",
                "010" => "10 - Tributada e com cobrança do ICMS por substituição tributária",
                "020" => "20 - Com redução de base de cálculo",
                "030" => "30 - Isenta ou não tributada e com cobrança do ICMS por substituição tributária",
                "040" => "40 - Isenta",
                "041" => "41 - Não tributada",
                "050" => "50 - Suspensão",
                "051" => "51 - Diferimento",
                "060" => "60 - ICMS cobrado anteriormente por substituição tributária",
                "070" => "70 - Com redução de base de cálculo e cobrança do ICMS por substituição tributária",
                "090" => "90 - Outras",
                "101" => "101 - Tributada pelo Simples Nacional com permissão de crédito",
                "102" => "102 - Tributada pelo Simples Nacional sem permissão de crédito",
                "103" => "103 - Isenção",
                "201" => "201 - Tributada pelo Simples Nacional com permissão de crédito e com cobrança do ICMS por substituição tributária",
                "202" => "202 - Tributada pelo Simples Nacional sem permissão de crédito e com cobrança do ICMS por substituição tributária",
                "203" => "203 - Isenção e com cobrança do ICMS por substituição tributária",
                "300" => "300 - Imune",
                "400" => "400 - Não tributada pelo Simples Nacional",
                "500" => "500 - ICMS cobrado anteriormente por substituição tributária (substituído) ou por antecipação",
                "900" => "900 - Outras",
                _ => cstCompleto
            };
        }

        private string FormatarCFOP(int cfop)
        {
            // Garantir que o CFOP tenha 4 dígitos
            return cfop.ToString("0000");
        }

        private string TruncarTexto(string texto, int maxLength)
        {
            if (string.IsNullOrEmpty(texto))
                return "";

            if (texto.Length <= maxLength)
                return texto;

            return texto.Substring(0, maxLength - 3) + "...";
        }
    }

    public class DanfeISSQN : IComponent
    {
        private readonly ISSQNTotal _issqn;

        public DanfeISSQN(ISSQNTotal issqn)
        {
            _issqn = issqn;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("CÁLCULO DO ISSQN").FontSize(7).Bold();

                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INSCRIÇÃO MUNICIPAL").FontSize(6);
                        col.Item().Text("").FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR TOTAL DOS SERVIÇOS").FontSize(6);
                        col.Item().Text((_issqn.vServ ?? 0).ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO ISSQN").FontSize(6);
                        col.Item().Text((_issqn.vBC ?? 0).ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO ISSQN").FontSize(6);
                        col.Item().Text((_issqn.vISS ?? 0).ToString("N2")).FontSize(9);
                    });
                });
            });
        }
    }

    public class DanfeInformacoesAdicionais : IComponent
    {
        private readonly InformacaoAdicional _infAdic;

        public DanfeInformacoesAdicionais(InformacaoAdicional infAdic)
        {
            _infAdic = infAdic;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("DADOS ADICIONAIS").FontSize(7).Bold();

                column.Item().Row(row =>
                {
                    // Informações Complementares
                    row.RelativeItem(3).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("INFORMAÇÕES COMPLEMENTARES").FontSize(6);
                        col.Item().MinHeight(60).Text(_infAdic?.infCpl ?? "").FontSize(7);
                    });

                    // Reservado ao Fisco
                    row.RelativeItem(2).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("RESERVADO AO FISCO").FontSize(6);
                        col.Item().MinHeight(60).Text(_infAdic?.infAdFisco ?? "").FontSize(7);
                    });
                });
            });
        }
    }
}
