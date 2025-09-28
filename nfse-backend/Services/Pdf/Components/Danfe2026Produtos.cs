using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;
using System.Collections.Generic;

namespace nfse_backend.Services.Pdf
{
    public class Danfe2026Produtos : IComponent
    {
        private readonly List<DetalheNFe> _produtos;
        private readonly List<GrupoIBS> _gruposIBS;
        private readonly List<GrupoCBS> _gruposCBS;
        private readonly List<GrupoIS> _gruposIS;
        private readonly List<RastreabilidadeItem> _rastreabilidade;

        public Danfe2026Produtos(List<DetalheNFe> produtos, List<GrupoIBS> gruposIBS, 
            List<GrupoCBS> gruposCBS, List<GrupoIS> gruposIS, List<RastreabilidadeItem> rastreabilidade)
        {
            _produtos = produtos;
            _gruposIBS = gruposIBS;
            _gruposCBS = gruposCBS;
            _gruposIS = gruposIS;
            _rastreabilidade = rastreabilidade;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("DADOS DOS PRODUTOS/SERVIÇOS - REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold();

                // Tabela principal de produtos
                column.Item().Table(table =>
                {
                    // Define as colunas para layout 2026 - ajustadas para evitar overflow
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);  // Item
                        columns.ConstantColumn(35);  // Código
                        columns.RelativeColumn(1);   // Descrição (ajustada para evitar overflow)
                        columns.ConstantColumn(30);  // NCM
                        columns.ConstantColumn(20);  // CST
                        columns.ConstantColumn(25);  // CFOP
                        columns.ConstantColumn(20);  // UN
                        columns.ConstantColumn(35);  // Quant
                        columns.ConstantColumn(35);  // V.Unit
                        columns.ConstantColumn(35);  // V.Total
                        columns.ConstantColumn(30);  // BC ICMS
                        columns.ConstantColumn(25);  // V.ICMS
                        columns.ConstantColumn(25);  // V.FCP
                        columns.ConstantColumn(25);  // V.IPI
                        columns.ConstantColumn(20);  // %ICMS
                        columns.ConstantColumn(20);  // %IPI
                        columns.ConstantColumn(25);  // BC IBS
                        columns.ConstantColumn(25);  // V.IBS
                        columns.ConstantColumn(25);  // BC CBS
                        columns.ConstantColumn(25);  // V.CBS
                        columns.ConstantColumn(25);  // BC IS
                        columns.ConstantColumn(25);  // V.IS
                        columns.ConstantColumn(40);  // GTIN
                    });

                    // Cabeçalho
                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ITEM").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CÓDIGO").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("DESCRIÇÃO").FontSize(6).Bold();
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
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("BC IBS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("V.IBS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("BC CBS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("V.CBS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("BC IS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("V.IS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Purple.Lighten5).AlignCenter().Text("GTIN").FontSize(6);
                    });

                    // Linhas de produtos
                    for (int i = 0; i < _produtos.Count; i++)
                    {
                        var item = _produtos[i];
                        var itemNumber = i + 1;
                        
                        // Buscar dados IBS/CBS/IS para este item
                        var ibsItem = _gruposIBS?.FirstOrDefault(x => x.nItem == itemNumber);
                        var cbsItem = _gruposCBS?.FirstOrDefault(x => x.nItem == itemNumber);
                        var isItem = _gruposIS?.FirstOrDefault(x => x.nItem == itemNumber);
                        var rastreamentoItem = _rastreabilidade?.FirstOrDefault(x => x.nItem == itemNumber);

                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(itemNumber.ToString()).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item?.Prod?.cProd ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(TruncarTexto(item?.Prod?.xProd ?? "", 25)).FontSize(6).FontFamily("Arial");
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item?.Prod?.NCM ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(FormatarCSTComDescricao(item?.Imposto?.ICMS?.CST ?? "", item?.Imposto?.ICMS?.orig ?? "")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(FormatarCFOP(item?.Prod?.CFOP ?? 0)).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item?.Prod?.uCom ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Prod?.qCom ?? 0m).ToString("N4")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Prod?.vUnCom ?? 0m).ToString("N4")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Prod?.vProd ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Imposto?.ICMS?.vBC ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Imposto?.ICMS?.vICMS ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(((item?.Imposto?.ICMS?.vFCP ?? 0m) > 0 ? (item?.Imposto?.ICMS?.vFCP ?? 0m).ToString("N2") : "")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(((item?.Imposto?.IPI?.vIPI ?? 0m) > 0 ? (item?.Imposto?.IPI?.vIPI ?? 0m).ToString("N2") : "")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text((item?.Imposto?.ICMS?.pICMS ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text((item?.Imposto?.IPI?.pIPI ?? 0m).ToString("N2")).FontSize(7);
                        
                        // Campos IBS
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5)
                            .Text((ibsItem?.vBCIBS ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5)
                            .Text((ibsItem?.vIBS ?? 0m).ToString("N2")).FontSize(7);
                        
                        // Campos CBS
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5)
                            .Text((cbsItem?.vBCCBS ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5)
                            .Text((cbsItem?.vCBS ?? 0m).ToString("N2")).FontSize(7);
                        
                        // Campos IS
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5)
                            .Text((isItem?.vBCIS ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5)
                            .Text((isItem?.vIS ?? 0m).ToString("N2")).FontSize(7);
                        
                        // GTIN
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Background(Colors.Purple.Lighten5)
                            .Text(rastreamentoItem?.GTIN ?? "").FontSize(7);

                        // Linha adicional com observações fiscais especiais (quando aplicável)
                        var observacoes = new List<string>();

                        if (ibsItem != null || cbsItem != null || isItem != null)
                        {
                            // Monofasia
                            if ((ibsItem?.gIBSCBSMono ?? false) || (cbsItem?.gIBSCBSMono ?? false) || (isItem?.gIBSCBSMono ?? false))
                                observacoes.Add("Monofasia");

                            // Crédito Presumido (CBS)
                            if (cbsItem?.gCBSCredPres ?? false)
                                observacoes.Add("Crédito Presumido (CBS)");

                            // Diferimento (pDif)
                            var pDifs = new List<decimal?> { ibsItem?.pDif, cbsItem?.pDif, isItem?.pDif }
                                .Where(x => x.HasValue && x.Value > 0).Select(x => x!.Value).ToList();
                            if (pDifs.Any())
                                observacoes.Add($"Diferimento {pDifs.Max():0.##}%");

                            // Devolução tributária (vDevTrib)
                            var vDevTribs = new List<decimal?> { ibsItem?.vDevTrib, cbsItem?.vDevTrib, isItem?.vDevTrib }
                                .Where(x => x.HasValue && x.Value > 0).Select(x => x!.Value).ToList();
                            if (vDevTribs.Any())
                                observacoes.Add($"Devolução Trib. {vDevTribs.Sum():N2}");

                            // Alíquotas (informativas)
                            var aliquotas = new List<string>();
                            if ((ibsItem?.pIBS ?? 0m) > 0) aliquotas.Add($"IBS {ibsItem?.pIBS:0.##}%");
                            if ((cbsItem?.pCBS ?? 0m) > 0) aliquotas.Add($"CBS {cbsItem?.pCBS:0.##}%");
                            if ((isItem?.pIS ?? 0m) > 0) aliquotas.Add($"IS {isItem?.pIS:0.##}%");
                            if (aliquotas.Any())
                                observacoes.Add("Alíquotas: " + string.Join(", ", aliquotas));

                            // Benefício/Código
                            var beneficios = new List<string>();
                            if (!string.IsNullOrWhiteSpace(ibsItem?.CodigoBeneficio)) beneficios.Add($"Benefício IBS: {ibsItem.CodigoBeneficio}");
                            if (!string.IsNullOrWhiteSpace(cbsItem?.CodigoBeneficio)) beneficios.Add($"Benefício CBS: {cbsItem.CodigoBeneficio}");
                            if (!string.IsNullOrWhiteSpace(isItem?.CodigoBeneficio)) beneficios.Add($"Benefício IS: {isItem.CodigoBeneficio}");
                            if (beneficios.Any())
                                observacoes.Add(string.Join("; ", beneficios));
                        }

                        if (observacoes.Any())
                        {
                            table.Cell().ColumnSpan(23).Border(0.5f).Padding(1).Background(Colors.Grey.Lighten5)
                                .Text("Obs. Fiscais: " + string.Join("; ", observacoes)).FontSize(6).FontColor(Colors.Grey.Darken2);
                        }
                    }
                });

                // Seção de rastreabilidade detalhada (quando aplicável)
                if (_rastreabilidade?.Any() == true)
                {
                    column.Item().PaddingTop(5).Component(new Danfe2026Rastreabilidade(_rastreabilidade));
                }
            });
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
}
