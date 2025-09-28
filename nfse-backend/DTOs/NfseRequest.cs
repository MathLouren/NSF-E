using nfse_backend.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace nfse_backend.DTOs;

public class NfseRequest
{
    public int Numero { get; set; }
    public string Serie { get; set; } = string.Empty;
    
    [JsonPropertyName("data_emissao")]
    public DateTime DataEmissao { get; set; }
    public string Status { get; set; } = string.Empty;

    public Prestador Prestador { get; set; } = new Prestador();
    public Tomador Tomador { get; set; } = new Tomador();
    public List<ServicoItem> Servicos { get; set; } = new List<ServicoItem>();
    
    [JsonPropertyName("informacoesAdicionais")]
    public InformacoesAdicionais InformacoesAdicionais { get; set; } = new InformacoesAdicionais();
}