using nfse_backend.DTOs;
using nfse_backend.Models;

namespace nfse_backend.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
        Task<ProductResponse?> GetProductByIdAsync(Guid id);
        Task<ProductResponse> CreateProductAsync(ProductRequest productRequest);
        Task<ProductResponse?> UpdateProductAsync(Guid id, ProductRequest productRequest);
        Task<bool> DeleteProductAsync(Guid id);
    }
}
