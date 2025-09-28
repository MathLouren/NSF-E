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
using System.Collections.Generic;

namespace nfse_backend.Services.Pdf
{
    /// <summary>
    /// Serviço de geração de DANFE para NF-e 2026 conforme reforma tributária
    /// Implementa os novos campos obrigatórios: IBS, CBS, IS, GTIN, separação UF/Município
    /// </summary>
    public class Danfe2026Service
    {
        private readonly string _fontFamily = "Arial";

        public byte[] GenerateDanfe2026(NFe2026 nfe, bool isContingencia = false)
        {
            // Enriquecer o modelo com totais e agregações quando não fornecidos
            EnrichNfe2026ComputedTotals(nfe);

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

                        // Cabeçalho 2026
                        column.Item().Component(new Danfe2026Cabecalho(nfe, isContingencia));

                        // Destinatário/Remetente
                        column.Item().Component(new DanfeDestinatarioRemetente(nfe));

                        // Dados do Faturamento
                        if (nfe.Cobr != null && nfe.Cobr.Fat != null)
                        {
                            column.Item().Component(new DanfeFaturamento(nfe.Cobr));
                        }

                        // Cálculo do Imposto 2026 (IBS/CBS/IS)
                        column.Item().Component(new Danfe2026CalculoImposto(nfe));

                        // Transportador/Volumes
                        column.Item().Component(new DanfeTransportador(nfe.Transp));

                        // Dados dos Produtos/Serviços 2026 (versão simplificada para evitar problemas de layout)
                        column.Item().Component(new Danfe2026ProdutosSimples(nfe.Det, nfe.GruposIBS, nfe.GruposCBS, nfe.GruposIS, nfe.Rastreabilidade));

                        // Totais por UF/Município
                        if (nfe.TotaisPorUF?.Any() == true || nfe.TotaisPorMunicipio?.Any() == true)
                        {
                            column.Item().Component(new Danfe2026TotaisPorUFMunicipio(nfe.TotaisPorUF ?? new List<TotalPorUF>(), nfe.TotaisPorMunicipio ?? new List<TotalPorMunicipio>()));
                        }

                        // Referências de Documentos
                        if (nfe.Referencias?.Any() == true)
                        {
                            column.Item().Component(new Danfe2026Referencias(nfe.Referencias));
                        }

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

        /// <summary>
        /// Preenche totais IBS/CBS/IS e agregações por UF/Município quando não fornecidas no payload,
        /// bem como rastreabilidade básica a partir do GTIN do produto.
        /// </summary>
        private void EnrichNfe2026ComputedTotals(NFe2026 nfe)
        {
            if (nfe == null)
                return;

            // Totais IBS
            if (nfe.TotaisIBS == null)
                nfe.TotaisIBS = new TotalIBS();

            if ((nfe.TotaisIBS.vBCIBS == 0 && nfe.TotaisIBS.vIBS == 0) && (nfe.GruposIBS?.Any() == true))
            {
                nfe.TotaisIBS.vBCIBS = nfe.GruposIBS.Sum(x => x.vBCIBS);
                nfe.TotaisIBS.vIBS = nfe.GruposIBS.Sum(x => x.vIBS);
            }

            // Totais CBS
            if (nfe.TotaisCBS == null)
                nfe.TotaisCBS = new TotalCBS();

            if ((nfe.TotaisCBS.vBCCBS == 0 && nfe.TotaisCBS.vCBS == 0) && (nfe.GruposCBS?.Any() == true))
            {
                nfe.TotaisCBS.vBCCBS = nfe.GruposCBS.Sum(x => x.vBCCBS);
                nfe.TotaisCBS.vCBS = nfe.GruposCBS.Sum(x => x.vCBS);
            }

            // Totais IS
            if (nfe.TotaisIS == null)
                nfe.TotaisIS = new TotalIS();

            if ((nfe.TotaisIS.vBCIS == 0 && nfe.TotaisIS.vIS == 0) && (nfe.GruposIS?.Any() == true))
            {
                nfe.TotaisIS.vBCIS = nfe.GruposIS.Sum(x => x.vBCIS);
                nfe.TotaisIS.vIS = nfe.GruposIS.Sum(x => x.vIS);
            }

            // Agregações por UF
            if ((nfe.TotaisPorUF == null || nfe.TotaisPorUF.Count == 0) &&
                ((nfe.GruposIBS?.Any() == true) || (nfe.GruposCBS?.Any() == true) || (nfe.GruposIS?.Any() == true)))
            {
                var totaisPorUF = new Dictionary<string, TotalPorUF>();

                void AcumularUF(string uf, decimal vbcIbs, decimal vIbs, decimal vbcCbs, decimal vCbs, decimal vbcIs, decimal vIs)
                {
                    if (string.IsNullOrWhiteSpace(uf)) uf = "--";
                    if (!totaisPorUF.TryGetValue(uf, out var total))
                    {
                        total = new TotalPorUF { UF = uf, QuantidadeItens = 0 };
                        totaisPorUF[uf] = total;
                    }
                    total.vBCIBS += vbcIbs;
                    total.vIBS += vIbs;
                    total.vBCCBS += vbcCbs;
                    total.vCBS += vCbs;
                    total.vBCIS += vbcIs;
                    total.vIS += vIs;
                    total.QuantidadeItens += 1;
                }

                foreach (var g in nfe.GruposIBS ?? new List<GrupoIBS>())
                    AcumularUF(g.UF, g.vBCIBS, g.vIBS, 0, 0, 0, 0);

                foreach (var g in nfe.GruposCBS ?? new List<GrupoCBS>())
                    AcumularUF(g.UF, 0, 0, g.vBCCBS, g.vCBS, 0, 0);

                foreach (var g in nfe.GruposIS ?? new List<GrupoIS>())
                    AcumularUF(g.UF, 0, 0, 0, 0, g.vBCIS, g.vIS);

                nfe.TotaisPorUF = totaisPorUF.Values.ToList();
            }

            // Agregações por Município
            if ((nfe.TotaisPorMunicipio == null || nfe.TotaisPorMunicipio.Count == 0) &&
                ((nfe.GruposIBS?.Any() == true) || (nfe.GruposCBS?.Any() == true) || (nfe.GruposIS?.Any() == true)))
            {
                var totaisPorMun = new Dictionary<(string uf, int cod, string nome), TotalPorMunicipio>();

                TotalPorMunicipio GetMun(string ufValue, int codValue, string nomeValue)
                {
                    var key = (uf: ufValue ?? "--", cod: codValue, nome: nomeValue ?? "");
                    if (!totaisPorMun.TryGetValue(key, out var total))
                    {
                        total = new TotalPorMunicipio { UF = key.uf, CodigoMunicipio = key.cod, NomeMunicipio = key.nome };
                        totaisPorMun[key] = total;
                    }
                    return total;
                }

                foreach (var g in nfe.GruposIBS ?? new List<GrupoIBS>())
                {
                    var t = GetMun(g.UF, g.CodigoMunicipio, g.NomeMunicipio);
                    t.vBCIBS += g.vBCIBS; t.vIBS += g.vIBS; t.QuantidadeItens += 1;
                }
                foreach (var g in nfe.GruposCBS ?? new List<GrupoCBS>())
                {
                    var t = GetMun(g.UF, g.CodigoMunicipio, g.NomeMunicipio);
                    t.vBCCBS += g.vBCCBS; t.vCBS += g.vCBS; t.QuantidadeItens += 1;
                }
                foreach (var g in nfe.GruposIS ?? new List<GrupoIS>())
                {
                    var t = GetMun(g.UF, g.CodigoMunicipio, g.NomeMunicipio);
                    t.vBCIS += g.vBCIS; t.vIS += g.vIS; t.QuantidadeItens += 1;
                }

                nfe.TotaisPorMunicipio = totaisPorMun.Values.ToList();
            }

            // Rastreabilidade básica a partir do GTIN (prod.cEAN) quando não fornecida
            if ((nfe.Rastreabilidade == null || nfe.Rastreabilidade.Count == 0) && (nfe.Det?.Any() == true))
            {
                nfe.Rastreabilidade = new List<RastreabilidadeItem>();
                for (int i = 0; i < nfe.Det.Count; i++)
                {
                    var det = nfe.Det[i];
                    var gtin = det?.Prod?.cEAN ?? det?.Prod?.cEANTrib;
                    if (!string.IsNullOrWhiteSpace(gtin))
                    {
                        nfe.Rastreabilidade.Add(new RastreabilidadeItem
                        {
                            nItem = i + 1,
                            GTIN = gtin,
                            TipoRastreabilidade = "GTIN",
                        });
                    }
                }
            }
        }
    }
}
