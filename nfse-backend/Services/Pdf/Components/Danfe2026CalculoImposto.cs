using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models.NFe;
using System;
using System.Linq;

namespace nfse_backend.Services.Pdf
{
    public class Danfe2026CalculoImposto : IComponent
    {
        private readonly NFe2026 _nfe;

        public Danfe2026CalculoImposto(NFe2026 nfe)
        {
            _nfe = nfe;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                // Cálculo do Imposto - Layout 2026
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("CÁLCULO DO IMPOSTO - REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold();

                // Primeira linha - ICMS (mantido para compatibilidade)
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO ICMS").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vBC.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO ICMS").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vICMS.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO ICMS ST").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vBCST.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO ICMS ST").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vST.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO FCP").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vFCP.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR TOTAL DOS PRODUTOS").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vProd.ToString("N2")).FontSize(9).Bold();
                    });
                });

                // Segunda linha - IBS (Imposto sobre Bens e Serviços)
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Blue.Lighten5).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO IBS").FontSize(6).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(_nfe.TotaisIBS?.vBCIBS.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Blue.Lighten5).Column(col =>
                    {
                        col.Item().Text("VALOR DO IBS").FontSize(6).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(_nfe.TotaisIBS?.vIBS.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Blue.Lighten5).Column(col =>
                    {
                        col.Item().Text("VALOR DO IBS ST").FontSize(6).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(_nfe.TotaisIBS?.vIBSST.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Blue.Lighten5).Column(col =>
                    {
                        col.Item().Text("VALOR DO FCP IBS").FontSize(6).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(_nfe.TotaisIBS?.vFCPIBS.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Blue.Lighten5).Column(col =>
                    {
                        col.Item().Text("IBS DEVOLVIDO").FontSize(6).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(_nfe.TotaisIBS?.vIBSDevolvido.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Blue.Lighten5).Column(col =>
                    {
                        col.Item().Text("IBS RETIDO").FontSize(6).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(_nfe.TotaisIBS?.vIBSRetido.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });
                });

                // Terceira linha - CBS (Contribuição sobre Bens e Serviços)
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Green.Lighten5).Column(col =>
                    {
                        col.Item().Text("BASE DE CÁLCULO DO CBS").FontSize(6).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text(_nfe.TotaisCBS?.vBCCBS.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Green.Lighten5).Column(col =>
                    {
                        col.Item().Text("VALOR DO CBS").FontSize(6).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text(_nfe.TotaisCBS?.vCBS.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Green.Lighten5).Column(col =>
                    {
                        col.Item().Text("VALOR DO CBS ST").FontSize(6).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text(_nfe.TotaisCBS?.vCBSST.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Green.Lighten5).Column(col =>
                    {
                        col.Item().Text("VALOR DO FCP CBS").FontSize(6).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text(_nfe.TotaisCBS?.vFCPCBS.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Green.Lighten5).Column(col =>
                    {
                        col.Item().Text("CBS DEVOLVIDO").FontSize(6).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text(_nfe.TotaisCBS?.vCBSDevolvido.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Green.Lighten5).Column(col =>
                    {
                        col.Item().Text("CBS RETIDO").FontSize(6).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Text(_nfe.TotaisCBS?.vCBSRetido.ToString("N2") ?? "0,00").FontSize(9).Bold();
                    });
                });

                // Quarta linha - IS (Imposto Seletivo) - quando aplicável
                if (_nfe.TotaisIS != null && (_nfe.TotaisIS.vIS > 0 || _nfe.TotaisIS.vBCIS > 0))
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Orange.Lighten5).Column(col =>
                        {
                            col.Item().Text("BASE DE CÁLCULO DO IS").FontSize(6).Bold().FontColor(Colors.Orange.Darken2);
                            col.Item().Text(_nfe.TotaisIS.vBCIS.ToString("N2")).FontSize(9).Bold();
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Orange.Lighten5).Column(col =>
                        {
                            col.Item().Text("VALOR DO IS").FontSize(6).Bold().FontColor(Colors.Orange.Darken2);
                            col.Item().Text(_nfe.TotaisIS.vIS.ToString("N2")).FontSize(9).Bold();
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Orange.Lighten5).Column(col =>
                        {
                            col.Item().Text("VALOR DO IS ST").FontSize(6).Bold().FontColor(Colors.Orange.Darken2);
                            col.Item().Text(_nfe.TotaisIS.vISST.ToString("N2")).FontSize(9).Bold();
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Orange.Lighten5).Column(col =>
                        {
                            col.Item().Text("VALOR DO FCP IS").FontSize(6).Bold().FontColor(Colors.Orange.Darken2);
                            col.Item().Text(_nfe.TotaisIS.vFCPIS.ToString("N2")).FontSize(9).Bold();
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Orange.Lighten5).Column(col =>
                        {
                            col.Item().Text("IS DEVOLVIDO").FontSize(6).Bold().FontColor(Colors.Orange.Darken2);
                            col.Item().Text(_nfe.TotaisIS.vISDevolvido.ToString("N2")).FontSize(9).Bold();
                        });

                        row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Orange.Lighten5).Column(col =>
                        {
                            col.Item().Text("IS RETIDO").FontSize(6).Bold().FontColor(Colors.Orange.Darken2);
                            col.Item().Text(_nfe.TotaisIS.vISRetido.ToString("N2")).FontSize(9).Bold();
                        });
                    });
                }

                // Quinta linha - Outros valores (frete, seguro, etc.)
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO FRETE").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vFrete.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO SEGURO").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vSeg.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("DESCONTO").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vDesc.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("OUTRAS DESPESAS ACESSÓRIAS").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vOutro.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Column(col =>
                    {
                        col.Item().Text("VALOR DO IPI").FontSize(6);
                        col.Item().Text(_nfe.Total.ICMSTot.vIPI.ToString("N2")).FontSize(9);
                    });

                    row.RelativeItem(1).Border(0.5f).Padding(2).Background(Colors.Red.Lighten4).Column(col =>
                    {
                        col.Item().Text("VALOR TOTAL DA NOTA").FontSize(6).Bold();
                        col.Item().Text(_nfe.Total.ICMSTot.vNF.ToString("N2")).FontSize(10).Bold();
                    });
                });
            });
        }
    }
}
