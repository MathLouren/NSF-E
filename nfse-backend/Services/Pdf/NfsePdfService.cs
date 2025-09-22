using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using nfse_backend.Models;

namespace nfse_backend.Services.Pdf
{
    public class NfsePdfService
    {
        public byte[] GenerateNfsePdf(Nfse nfse)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"DANFE - NFS-e Nº {nfse.Numero}")
                        .SemiBold().FontSize(16).AlignCenter();

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item().Text("Dados do Prestador").SemiBold().FontSize(12);
                            column.Item().Text($"CNPJ: {nfse.Prestador.Cnpj}");
                            column.Item().Text($"Razão Social: {nfse.Prestador.RazaoSocial}");
                            column.Item().Text($"Endereço: {nfse.Prestador.Logradouro}, {nfse.Prestador.Numero} - {nfse.Prestador.Bairro}");
                            column.Item().Text($"Município: {nfse.Prestador.Municipio}/{nfse.Prestador.Uf} - CEP: {nfse.Prestador.Cep}");
                            column.Item().Text($"Inscrição Municipal: {nfse.Prestador.InscricaoMunicipal}");
                            column.Item().Text($"Código do Município: {nfse.Prestador.CodigoMunicipio}");
                            column.Item().Text($"Telefone: {nfse.Prestador.Telefone}");
                            column.Item().Text($"Email: {nfse.Prestador.Email}");

                            column.Item().PaddingTop(10).Text("Dados do Tomador").SemiBold().FontSize(12);
                            column.Item().Text($"CPF/CNPJ: {nfse.Tomador.CpfCnpj}");
                            column.Item().Text($"Nome/Razão Social: {nfse.Tomador.NomeRazaoSocial}");
                            column.Item().Text($"Endereço: {nfse.Tomador.Logradouro}, {nfse.Tomador.Numero} - {nfse.Tomador.Bairro}");
                            column.Item().Text($"Município: {nfse.Tomador.Municipio}/{nfse.Tomador.Uf} - CEP: {nfse.Tomador.Cep}");
                            column.Item().Text($"Telefone: {nfse.Tomador.Telefone}");
                            column.Item().Text($"Email: {nfse.Tomador.Email}");

                            column.Item().PaddingTop(10).Text("Dados dos Serviços").SemiBold().FontSize(12);
                            column.Item().Table(table =>
                            {
                                // Definição das colunas
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50); // Código
                                    columns.RelativeColumn(3f);  // Descrição
                                    columns.RelativeColumn(0.8f);  // Quantidade
                                    columns.RelativeColumn(1f);  // Valor Unit.
                                    columns.RelativeColumn(1f);  // Valor Total
                                    columns.RelativeColumn(1f);  // Desconto
                                    columns.RelativeColumn(1f);  // Alíquota IBS
                                    columns.RelativeColumn(1f);  // Base Calc ISS
                                    columns.RelativeColumn(1f);  // Valor ISS
                                });

                                // Cabeçalho
                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Código").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Descrição").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Quant.").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Vlr Unit.").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Vlr Total").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Desconto").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("IBS %").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("BC ISS").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Vlr ISS").SemiBold();
                                });

                                // Linhas de dados
                                foreach (var servico in nfse.Servicos)
                                {
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.Codigo);
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.Descricao);
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.Quantidade.ToString());
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.ValorUnitario.ToString("F2"));
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.ValorTotal.ToString("F2"));
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.Desconto.ToString("F2"));
                                    table.Cell().BorderBottom(1).Padding(5).Text($"{servico.AliquotaIbs}%");
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.BaseCalculoIss?.ToString("F2") ?? "N/A");
                                    table.Cell().BorderBottom(1).Padding(5).Text(servico.ValorIss?.ToString("F2") ?? "N/A");
                                }
                            });

                            column.Item().PaddingTop(10).Text("Informações Adicionais").SemiBold().FontSize(12);
                            column.Item().Text($"Forma de Pagamento: {nfse.InformacoesAdicionais.FormaPagamento}");
                            column.Item().Text($"Observações: {nfse.InformacoesAdicionais.Observacoes}");
                            column.Item().Text($"Município do Prestador: {nfse.InformacoesAdicionais.MunicipioPrestador}");
                            column.Item().Text($"Código de Tributação Municipal: {nfse.InformacoesAdicionais.CodigoTributacaoMunicipal}");
                            column.Item().Text($"Protocolo de Envio: {nfse.InformacoesAdicionais.ProtocoloEnvio}");
                            column.Item().PaddingTop(10).Text($"Data de Emissão: {nfse.DataEmissao:dd/MM/yyyy HH:mm:ss}");
                            column.Item().Text($"Status: {nfse.Status}");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ").FontSize(10);
                            x.CurrentPageNumber().FontSize(10);
                            x.Span(" de ").FontSize(10);
                            x.TotalPages().FontSize(10);
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
