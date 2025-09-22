using AutoMapper;
using nfse_backend.DTOs;
using nfse_backend.Models;
using nfse_backend.Repositories;

namespace nfse_backend.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CompanyService(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllCompaniesAsync();
            return _mapper.Map<IEnumerable<CompanyDto>>(companies);
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(Guid id)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(id);
            return _mapper.Map<CompanyDto>(company);
        }

        public async Task<CompanyDto> CreateCompanyAsync(CompanyDto companyDto)
        {
            var company = _mapper.Map<Company>(companyDto);
            company.Id = Guid.NewGuid(); // Assign a new GUID for the Id
            await _companyRepository.AddCompanyAsync(company);
            return _mapper.Map<CompanyDto>(company);
        }

        public async Task<CompanyDto?> UpdateCompanyAsync(Guid id, CompanyDto companyDto)
        {
            var existingCompany = await _companyRepository.GetCompanyByIdAsync(id);
            if (existingCompany == null)
            {
                return null;
            }

            _mapper.Map(companyDto, existingCompany);
            existingCompany.Id = id; // Ensure the ID remains the same
            await _companyRepository.UpdateCompanyAsync(existingCompany);
            return _mapper.Map<CompanyDto>(existingCompany);
        }

        public async Task<bool> DeleteCompanyAsync(Guid id)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return false;
            }
            await _companyRepository.DeleteCompanyAsync(id);
            return true;
        }
    }
}
