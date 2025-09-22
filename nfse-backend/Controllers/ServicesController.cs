using Microsoft.AspNetCore.Mvc;
using nfse_backend.Data;
using nfse_backend.Models;
using nfse_backend.DTOs;
using AutoMapper;

namespace nfse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ServicesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ServiceResponse>> GetServices()
        {
            var services = _context.Services.ToList();
            return Ok(_mapper.Map<IEnumerable<ServiceResponse>>(services));
        }

        [HttpGet("{id}")]
        public ActionResult<ServiceResponse> GetService(Guid id)
        {
            var service = _context.Services.Find(id);

            if (service == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ServiceResponse>(service));
        }

        [HttpPost]
        public ActionResult<ServiceResponse> CreateService(ServiceRequest serviceRequest)
        {
            var service = _mapper.Map<Service>(serviceRequest);
            service.Id = Guid.NewGuid(); 
            _context.Services.Add(service);
            _context.SaveChanges();

            var serviceResponse = _mapper.Map<ServiceResponse>(service);
            return CreatedAtAction(nameof(GetService), new { id = serviceResponse.Id }, serviceResponse);
        }
    }
}
