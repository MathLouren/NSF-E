using nfse_backend.Models;
using System.Collections.Generic;

namespace nfse_backend.Models;

public class Nfse
{
    public Guid Id { get; set; }
    public int Numero { get; set; } // Corresponds to the sequential number
    public string Serie { get; set; } = "1"; // Default serie
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Default status

    public Prestador Prestador { get; set; } = new Prestador();
    public Tomador Tomador { get; set; } = new Tomador();
    public List<ServicoItem> Servicos { get; set; } = new List<ServicoItem>();
    public InformacoesAdicionais InformacoesAdicionais { get; set; } = new InformacoesAdicionais();

    // Remove old properties
    // public int SequentialNumber { get; set; }
    // public string ProviderCnpj { get; set; } = string.Empty;
    // public string ProviderLegalName { get; set; } = string.Empty;
    // public string RecipientDoc { get; set; } = string.Empty;
    // public string RecipientLegalName { get; set; } = string.Empty;
    // public string ServiceCode { get; set; } = string.Empty;
    // public string ServiceDescription { get; set; } = string.Empty;
    // public decimal ServiceValue { get; set; }
    // public int ServiceQuantity { get; set; }
    // public decimal IBSRate { get; set; }
    // public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    // public string Status { get; set; } = "Pending"; // Default status
}