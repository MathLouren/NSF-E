using nfse_backend.DTOs;
using nfse_backend.Models;

namespace nfse_backend.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceResponse>> GetAllServicesAsync();
        Task<ServiceResponse?> GetServiceByIdAsync(Guid id);
        Task<ServiceResponse> CreateServiceAsync(ServiceRequest serviceRequest);
        Task<ServiceResponse?> UpdateServiceAsync(Guid id, ServiceRequest serviceRequest);
        Task<bool> DeleteServiceAsync(Guid id);
    }
}
