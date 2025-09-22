using Microsoft.AspNetCore.Mvc;
using nfse_backend.Data;
using nfse_backend.Models;
using nfse_backend.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace nfse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ConfigurationsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CompanyDto>> GetConfigurations()
        {
            // For simplicity, let's assume there's only one company settings
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return NotFound("Company configurations not found.");
            }
            return Ok(_mapper.Map<CompanyDto>(company));
        }

        [HttpPost]
        public async Task<ActionResult<CompanyDto>> UpdateConfigurations(CompanyDto companyDto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                // If no existing company, create a new one
                company = _mapper.Map<Company>(companyDto);
                company.Id = Guid.NewGuid();
                _context.Companies.Add(company);
            }
            else
            {
                // Update existing company
                _mapper.Map(companyDto, company);
            }
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<CompanyDto>(company));
        }
    }
}
