using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;

namespace nfse_backend.Services.Pdf
{
    public class Danfe2026TotaisPorUFMunicipio : IComponent
    {
        private readonly List<TotalPorUF> _totaisPorUF;
        private readonly List<TotalPorMunicipio> _totaisPorMunicipio;

        public Danfe2026TotaisPorUFMunicipio(List<TotalPorUF> totaisPorUF, List<TotalPorMunicipio> totaisPorMunicipio)
        {
            _totaisPorUF = totaisPorUF;
            _totaisPorMunicipio = totaisPorMunicipio;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                // Totais por UF
                if (_totaisPorUF?.Any() == true)
                {
                    column.Item().Background(Colors.Blue.Lighten4).Padding(2).Text("TOTAIS POR UF - REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);  // UF
                            columns.ConstantColumn(60);  // Qtd Itens
                            columns.ConstantColumn(80);  // BC IBS
                            columns.ConstantColumn(80);  // V. IBS
                            columns.ConstantColumn(80);  // BC CBS
                            columns.ConstantColumn(80);  // V. CBS
                            columns.ConstantColumn(80);  // BC IS
                            columns.ConstantColumn(80);  // V. IS
                        });

                        // Cabeçalho
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("UF").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("QTD ITENS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("BC IBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("V. IBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("BC CBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("V. CBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("BC IS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("V. IS").FontSize(6).Bold();
                        });

                        // Linhas de totais por UF
                        foreach (var total in _totaisPorUF)
                        {
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(total.UF).FontSize(7).Bold();
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(total.QuantidadeItens.ToString()).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5).Text(total.vBCIBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5).Text(total.vIBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5).Text(total.vBCCBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5).Text(total.vCBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5).Text(total.vBCIS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5).Text(total.vIS.ToString("N2")).FontSize(7);
                        }
                    });
                }

                // Totais por Município
                if (_totaisPorMunicipio?.Any() == true)
                {
                    column.Item().PaddingTop(5).Background(Colors.Green.Lighten4).Padding(2).Text("TOTAIS POR MUNICÍPIO - REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold().FontColor(Colors.Green.Darken2);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // UF
                            columns.ConstantColumn(80);  // Município
                            columns.ConstantColumn(50);  // Qtd Itens
                            columns.ConstantColumn(70);  // BC IBS
                            columns.ConstantColumn(70);  // V. IBS
                            columns.ConstantColumn(70);  // BC CBS
                            columns.ConstantColumn(70);  // V. CBS
                            columns.ConstantColumn(70);  // BC IS
                            columns.ConstantColumn(70);  // V. IS
                        });

                        // Cabeçalho
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("UF").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("MUNICÍPIO").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("QTD ITENS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("BC IBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Blue.Lighten5).AlignCenter().Text("V. IBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("BC CBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Green.Lighten5).AlignCenter().Text("V. CBS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("BC IS").FontSize(6).Bold();
                            header.Cell().Border(0.5f).Padding(1).Background(Colors.Orange.Lighten5).AlignCenter().Text("V. IS").FontSize(6).Bold();
                        });

                        // Linhas de totais por município
                        foreach (var total in _totaisPorMunicipio)
                        {
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(total.UF).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(TruncarTexto(total.NomeMunicipio, 20)).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(total.QuantidadeItens.ToString()).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5).Text(total.vBCIBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Blue.Lighten5).Text(total.vIBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5).Text(total.vBCCBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Green.Lighten5).Text(total.vCBS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5).Text(total.vBCIS.ToString("N2")).FontSize(7);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Background(Colors.Orange.Lighten5).Text(total.vIS.ToString("N2")).FontSize(7);
                        }
                    });
                }
            });
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
