using nfse_backend.Models;

namespace nfse_backend.Repositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(Guid id);
        Task<Service> AddAsync(Service service);
        Task<Service?> UpdateAsync(Service service);
        Task<Service?> DeleteAsync(Guid id);
    }
}
