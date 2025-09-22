using nfse_backend.DTOs;
using nfse_backend.Models;

namespace nfse_backend.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync();
        Task<CompanyDto?> GetCompanyByIdAsync(Guid id);
        Task<CompanyDto> CreateCompanyAsync(CompanyDto companyDto);
        Task<CompanyDto?> UpdateCompanyAsync(Guid id, CompanyDto companyDto);
        Task<bool> DeleteCompanyAsync(Guid id);
    }
}
