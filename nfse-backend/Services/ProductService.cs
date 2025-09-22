using AutoMapper;
using nfse_backend.DTOs;
using nfse_backend.Models;
using nfse_backend.Repositories;

namespace nfse_backend.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductResponse>>(products);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse> CreateProductAsync(ProductRequest productRequest)
        {
            var product = _mapper.Map<Product>(productRequest);
            product.Id = Guid.NewGuid();
            var createdProduct = await _productRepository.AddAsync(product);
            return _mapper.Map<ProductResponse>(createdProduct);
        }

        public async Task<ProductResponse?> UpdateProductAsync(Guid id, ProductRequest productRequest)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return null;
            }

            _mapper.Map(productRequest, existingProduct);
            existingProduct.Id = id; // Ensure ID remains the same
            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            return _mapper.Map<ProductResponse>(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var deletedProduct = await _productRepository.DeleteAsync(id);
            return deletedProduct != null;
        }
    }
}
