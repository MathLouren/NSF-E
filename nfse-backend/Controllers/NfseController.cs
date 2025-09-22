using Microsoft.AspNetCore.Mvc;
using nfse_backend.Data;
using nfse_backend.Models;
using nfse_backend.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nfse_backend.Services.Pdf;

namespace nfse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NfseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly NfsePdfService _nfsePdfService;

        public NfseController(AppDbContext context, IMapper mapper, NfsePdfService nfsePdfService)
        {
            _context = context;
            _mapper = mapper;
            _nfsePdfService = nfsePdfService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NfseResponse>>> GetNfses()
        {
            var nfses = await _context.Nfses
                .Include(n => n.Prestador)
                .Include(n => n.Tomador)
                .Include(n => n.InformacoesAdicionais)
                .ToListAsync(); // Servicos are owned and mapped to JSON, so no Include needed
            return Ok(_mapper.Map<IEnumerable<NfseResponse>>(nfses));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NfseResponse>> GetNfse(Guid id)
        {
            var nfse = await _context.Nfses
                .Include(n => n.Prestador)
                .Include(n => n.Tomador)
                .Include(n => n.InformacoesAdicionais)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nfse == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<NfseResponse>(nfse));
        }

        [HttpPost]
        public async Task<ActionResult<NfseResponse>> CreateNfse(NfseRequest nfseRequest)
        {
            var nfse = _mapper.Map<Nfse>(nfseRequest);
            nfse.Id = Guid.NewGuid(); // Generate a new GUID for the Id
            nfse.DataEmissao = DateTime.UtcNow; // Set issue date (from NFe JSON format)
            nfse.Status = "Issued"; // Default status

            // For Numero (sequential number), you would typically have a service to manage this per provider.
            // For simplicity, let's just use a basic count for now, but use the `Numero` property.
            nfse.Numero = await _context.Nfses.CountAsync() + 1; 
            
            _context.Nfses.Add(nfse);
            await _context.SaveChangesAsync();

            // Ensure related entities are loaded before mapping back for response
            // For owned entity types, they are usually loaded automatically if part of the main entity
            var nfseResponse = _mapper.Map<NfseResponse>(nfse);
            return CreatedAtAction(nameof(GetNfse), new { id = nfseResponse.Id }, nfseResponse);
        }

        [HttpGet("{id}/danfe")]
        public async Task<ActionResult> GetNfseDanfe(Guid id)
        {
            var nfse = await _context.Nfses
                .Include(n => n.Prestador)
                .Include(n => n.Tomador)
                .Include(n => n.InformacoesAdicionais)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nfse == null)
            {
                return NotFound("NFS-e not found.");
            }

            var pdfBytes = _nfsePdfService.GenerateNfsePdf(nfse);

            return File(pdfBytes, "application/pdf", $"DANFE_NFS-e_{nfse.Numero}.pdf");
        }
    }
}