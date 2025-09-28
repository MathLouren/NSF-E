using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;

namespace nfse_backend.Services.Pdf
{
    public class Danfe2026Rastreabilidade : IComponent
    {
        private readonly List<RastreabilidadeItem> _rastreabilidade;

        public Danfe2026Rastreabilidade(List<RastreabilidadeItem> rastreabilidade)
        {
            _rastreabilidade = rastreabilidade;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Purple.Lighten4).Padding(2).Text("RASTREABILIDADE - GTIN E INFORMAÇÕES DE LOTE").FontSize(7).Bold().FontColor(Colors.Purple.Darken2);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // Item
                        columns.ConstantColumn(80);  // GTIN
                        columns.ConstantColumn(60);  // Tipo
                        columns.ConstantColumn(60);  // Lote
                        columns.ConstantColumn(60);  // Fabricação
                        columns.ConstantColumn(60);  // Validade
                        columns.RelativeColumn(2);   // Informações Adicionais
                    });

                    // Cabeçalho
                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ITEM").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("GTIN").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("TIPO").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("LOTE").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("FABRICAÇÃO").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("VALIDADE").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("INFORMAÇÕES ADICIONAIS").FontSize(6);
                    });

                    // Linhas de rastreabilidade
                    foreach (var item in _rastreabilidade)
                    {
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.nItem.ToString()).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.GTIN ?? "").FontSize(7).FontFamily("Courier New");
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.TipoRastreabilidade ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.NumeroLote ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.DataFabricacao?.ToString("dd/MM/yyyy") ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(item.DataValidade?.ToString("dd/MM/yyyy") ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(item.InformacoesAdicionais ?? "").FontSize(7);
                    }
                });
            });
        }
    }
}
