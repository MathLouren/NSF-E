using Microsoft.AspNetCore.Mvc;
using nfse_backend.Data;
using nfse_backend.Models;
using nfse_backend.DTOs;
using AutoMapper;

namespace nfse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductResponse>> GetProducts()
        {
            var products = _context.Products.ToList();
            return Ok(_mapper.Map<IEnumerable<ProductResponse>>(products));
        }

        [HttpGet("{id}")]
        public ActionResult<ProductResponse> GetProduct(Guid id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductResponse>(product));
        }

        [HttpPost]
        public ActionResult<ProductResponse> CreateProduct(ProductRequest productRequest)
        {
            var product = _mapper.Map<Product>(productRequest);
            product.Id = Guid.NewGuid(); 
            _context.Products.Add(product);
            _context.SaveChanges();

            var productResponse = _mapper.Map<ProductResponse>(product);
            return CreatedAtAction(nameof(GetProduct), new { id = productResponse.Id }, productResponse);
        }
    }
}
