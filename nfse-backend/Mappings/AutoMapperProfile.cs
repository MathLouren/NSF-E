using AutoMapper;
using nfse_backend.Models;
using nfse_backend.DTOs;

namespace nfse_backend.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<NfseRequest, Nfse>();
        CreateMap<Nfse, NfseResponse>();
        CreateMap<ProductRequest, Product>().ReverseMap();
        CreateMap<Product, ProductResponse>().ReverseMap();
        CreateMap<ServiceRequest, Service>().ReverseMap();
        CreateMap<Service, ServiceResponse>().ReverseMap();
        CreateMap<CompanyDto, Company>().ReverseMap();

        // Mapeamentos para as classes aninhadas da NFS-e
        CreateMap<Prestador, Prestador>().ReverseMap();
        CreateMap<Tomador, Tomador>().ReverseMap();
        CreateMap<ServicoItem, ServicoItem>().ReverseMap();
        CreateMap<InformacoesAdicionais, InformacoesAdicionais>().ReverseMap();

    }
}