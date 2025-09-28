using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;

namespace nfse_backend.Services.Pdf
{
    public class Danfe2026Referencias : IComponent
    {
        private readonly List<ReferenciaDocumento> _referencias;

        public Danfe2026Referencias(List<ReferenciaDocumento> referencias)
        {
            _referencias = referencias;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Orange.Lighten4).Padding(2).Text("REFERÊNCIAS DE DOCUMENTOS FISCAIS - REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold().FontColor(Colors.Orange.Darken2);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // Item
                        columns.ConstantColumn(60);  // Tipo Doc
                        columns.ConstantColumn(120); // Chave Acesso
                        columns.ConstantColumn(40);  // Item Ref
                        columns.ConstantColumn(60);  // Data Emissão
                        columns.ConstantColumn(40);  // UF
                        columns.ConstantColumn(80);  // Valor
                        columns.RelativeColumn(2);   // Motivo
                    });

                    // Cabeçalho
                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ITEM").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("TIPO DOC").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CHAVE DE ACESSO").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ITEM REF").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("DATA EMISSÃO").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("UF").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("VALOR").FontSize(6).Bold();
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("MOTIVO").FontSize(6).Bold();
                    });

                    // Linhas de referências
                    foreach (var refDoc in _referencias)
                    {
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(refDoc.nItem.ToString()).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(refDoc.TipoDocumento ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(FormatarChaveAcesso(refDoc.ChaveAcessoReferenciada)).FontSize(6).FontFamily("Courier New");
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(refDoc.nItemReferenciado.ToString()).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(refDoc.DataEmissaoReferenciada.ToString("dd/MM/yyyy")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(refDoc.UFReferenciada ?? "").FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignRight().Text(refDoc.ValorReferenciado.ToString("N2")).FontSize(7);
                        table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(refDoc.MotivoReferencia ?? "").FontSize(7);
                    }
                });
            });
        }

        private string FormatarChaveAcesso(string chave)
        {
            if (string.IsNullOrEmpty(chave) || chave.Length != 44)
                return chave;

            return string.Join(" ", Enumerable.Range(0, 11)
                .Select(i => chave.Substring(i * 4, 4)));
        }
    }
}
