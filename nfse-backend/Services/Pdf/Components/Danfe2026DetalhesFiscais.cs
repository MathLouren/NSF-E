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
    /// Componente para detalhes fiscais adicionais da reforma tributária 2026
    /// </summary>
    public class Danfe2026DetalhesFiscais : IComponent
    {
        private readonly List<GrupoIBS> _gruposIBS;
        private readonly List<GrupoCBS> _gruposCBS;
        private readonly List<GrupoIS> _gruposIS;

        public Danfe2026DetalhesFiscais(List<GrupoIBS> gruposIBS, List<GrupoCBS> gruposCBS, List<GrupoIS> gruposIS)
        {
            _gruposIBS = gruposIBS;
            _gruposCBS = gruposCBS;
            _gruposIS = gruposIS;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Padding(2).Text("DETALHES FISCAIS - REFORMA TRIBUTÁRIA 2026").FontSize(7).Bold();

                // IBS
                if (_gruposIBS?.Any() == true)
                {
                    column.Item().PaddingTop(2).Component(new Danfe2026GrupoImposto("IBS - IMPOSTO SOBRE BENS E SERVIÇOS", _gruposIBS.Cast<object>().ToList(), Colors.Blue.Lighten5));
                }

                // CBS
                if (_gruposCBS?.Any() == true)
                {
                    column.Item().PaddingTop(2).Component(new Danfe2026GrupoImposto("CBS - CONTRIBUIÇÃO SOBRE BENS E SERVIÇOS", _gruposCBS.Cast<object>().ToList(), Colors.Green.Lighten5));
                }

                // IS
                if (_gruposIS?.Any() == true)
                {
                    column.Item().PaddingTop(2).Component(new Danfe2026GrupoImposto("IS - IMPOSTO SELETIVO", _gruposIS.Cast<object>().ToList(), Colors.Orange.Lighten5));
                }
            });
        }
    }

    /// <summary>
    /// Componente genérico para grupos de impostos
    /// </summary>
    public class Danfe2026GrupoImposto : IComponent
    {
        private readonly string _titulo;
        private readonly List<object> _grupos;
        private readonly string _corFundo;

        public Danfe2026GrupoImposto(string titulo, List<object> grupos, string corFundo)
        {
            _titulo = titulo;
            _grupos = grupos;
            _corFundo = corFundo;
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background(_corFundo).Padding(1).Text(_titulo).FontSize(6).Bold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // Item
                        columns.ConstantColumn(40);  // CST
                        columns.ConstantColumn(50);  // Base
                        columns.ConstantColumn(40);  // Alíquota
                        columns.ConstantColumn(50);  // Valor
                        columns.RelativeColumn(1);   // Observações
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ITEM").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("CST").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("BASE").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("ALÍQ").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("VALOR").FontSize(6);
                        header.Cell().Border(0.5f).Padding(1).AlignCenter().Text("OBSERVAÇÕES").FontSize(6);
                    });

                    foreach (var grupo in _grupos)
                    {
                        var observacoes = new List<string>();

                        if (grupo is GrupoIBS ibs)
                        {
                            if (ibs.gIBSCBSMono) observacoes.Add("Monofasia");
                            if (ibs.pDif.HasValue && ibs.pDif > 0) observacoes.Add($"Diferimento {ibs.pDif:0.##}%");
                            if (ibs.vDevTrib.HasValue && ibs.vDevTrib > 0) observacoes.Add($"Devolução {ibs.vDevTrib:N2}");
                            if (!string.IsNullOrWhiteSpace(ibs.CodigoBeneficio)) observacoes.Add($"Benefício: {ibs.CodigoBeneficio}");

                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(ibs.nItem.ToString()).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(ibs.ClassificacaoTributaria).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(ibs.vBCIBS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(ibs.pIBS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(ibs.vIBS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(string.Join("; ", observacoes)).FontSize(5);
                        }
                        else if (grupo is GrupoCBS cbs)
                        {
                            if (cbs.gIBSCBSMono) observacoes.Add("Monofasia");
                            if (cbs.gCBSCredPres) observacoes.Add("Crédito Presumido");
                            if (cbs.pDif.HasValue && cbs.pDif > 0) observacoes.Add($"Diferimento {cbs.pDif:0.##}%");
                            if (cbs.vDevTrib.HasValue && cbs.vDevTrib > 0) observacoes.Add($"Devolução {cbs.vDevTrib:N2}");
                            if (!string.IsNullOrWhiteSpace(cbs.CodigoBeneficio)) observacoes.Add($"Benefício: {cbs.CodigoBeneficio}");

                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(cbs.nItem.ToString()).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(cbs.ClassificacaoTributaria).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(cbs.vBCCBS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(cbs.pCBS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(cbs.vCBS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(string.Join("; ", observacoes)).FontSize(5);
                        }
                        else if (grupo is GrupoIS isItem)
                        {
                            if (isItem.gIBSCBSMono) observacoes.Add("Monofasia");
                            if (isItem.pDif.HasValue && isItem.pDif > 0) observacoes.Add($"Diferimento {isItem.pDif:0.##}%");
                            if (isItem.vDevTrib.HasValue && isItem.vDevTrib > 0) observacoes.Add($"Devolução {isItem.vDevTrib:N2}");
                            if (!string.IsNullOrWhiteSpace(isItem.CodigoBeneficio)) observacoes.Add($"Benefício: {isItem.CodigoBeneficio}");

                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(isItem.nItem.ToString()).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignCenter().Text(isItem.ClassificacaoTributaria).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(isItem.vBCIS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(isItem.pIS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignRight().Text(isItem.vIS.ToString("N2")).FontSize(6);
                            table.Cell().Border(0.5f).Padding(1).AlignLeft().Text(string.Join("; ", observacoes)).FontSize(5);
                        }
                    }
                });
            });
        }
    }
}
