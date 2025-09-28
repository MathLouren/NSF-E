using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;
using System.Collections.Generic;

namespace nfse_backend.Services.Pdf
{
    /// <summary>
    /// Componente simplificado para tabela de produtos DANFE 2026
    /// Evita problemas de layout com muitas colunas
    /// </summary>
    public class Danfe2026ProdutosSimples : IComponent
    {
        private readonly List<DetalheNFe> _produtos;
        private readonly List<GrupoIBS> _gruposIBS;
        private readonly List<GrupoCBS> _gruposCBS;
        private readonly List<GrupoIS> _gruposIS;
        private readonly List<RastreabilidadeItem> _rastreabilidade;

        public Danfe2026ProdutosSimples(List<DetalheNFe> produtos, List<GrupoIBS> gruposIBS, 
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

                // Tabela principal simplificada
                column.Item().Table(table =>
                {
                    // Colunas principais apenas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // Item
                        columns.RelativeColumn(2);   // Descrição
                        columns.ConstantColumn(40);  // NCM
                        columns.ConstantColumn(30);  // CFOP
                        columns.ConstantColumn(30);  // Quant
                        columns.ConstantColumn(40);  // V.Unit
                        columns.ConstantColumn(40);  // V.Total
                        columns.ConstantColumn(30);  // V.ICMS
                        columns.ConstantColumn(30);  // V.IBS
                        columns.ConstantColumn(30);  // V.CBS
                        columns.ConstantColumn(30);  // V.IS
                    });

                    // Cabeçalho
                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ITEM").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("DESCRIÇÃO").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("NCM").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CFOP").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("QUANT").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.UNIT").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.TOTAL").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("V.ICMS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("V.IBS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("V.CBS").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("V.IS").FontSize(6);
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

                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(itemNumber.ToString()).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(TruncarTexto(item?.Prod?.xProd ?? "", 30)).FontSize(6);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item?.Prod?.NCM ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(FormatarCFOP(item?.Prod?.CFOP ?? 0)).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Prod?.qCom ?? 0m).ToString("N4")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Prod?.vUnCom ?? 0m).ToString("N4")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Prod?.vProd ?? 0m).ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text((item?.Imposto?.ICMS?.vICMS ?? 0m).ToString("N2")).FontSize(7);
                        
                        // Campos IBS
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5)
                            .Text((ibsItem?.vIBS ?? 0m).ToString("N2")).FontSize(7);
                        
                        // Campos CBS
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5)
                            .Text((cbsItem?.vCBS ?? 0m).ToString("N2")).FontSize(7);
                        
                        // Campos IS
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5)
                            .Text((isItem?.vIS ?? 0m).ToString("N2")).FontSize(7);
                    }
                });

                // Seção de detalhes fiscais adicionais
                if (_gruposIBS?.Any() == true || _gruposCBS?.Any() == true || _gruposIS?.Any() == true)
                {
                    column.Item().PaddingTop(5).Component(new Danfe2026DetalhesFiscais(_gruposIBS, _gruposCBS, _gruposIS));
                }

                // Seção de rastreabilidade
                if (_rastreabilidade?.Any() == true)
                {
                    column.Item().PaddingTop(5).Component(new Danfe2026Rastreabilidade(_rastreabilidade));
                }
            });
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
