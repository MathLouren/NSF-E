using AutoMapper;
using nfse_backend.DTOs;
using nfse_backend.Models;
using nfse_backend.Repositories;

namespace nfse_backend.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceResponse>> GetAllServicesAsync()
        {
            var services = await _serviceRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceResponse>>(services);
        }

        public async Task<ServiceResponse?> GetServiceByIdAsync(Guid id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            return _mapper.Map<ServiceResponse>(service);
        }

        public async Task<ServiceResponse> CreateServiceAsync(ServiceRequest serviceRequest)
        {
            var service = _mapper.Map<Service>(serviceRequest);
            service.Id = Guid.NewGuid();
            var createdService = await _serviceRepository.AddAsync(service);
            return _mapper.Map<ServiceResponse>(createdService);
        }

        public async Task<ServiceResponse?> UpdateServiceAsync(Guid id, ServiceRequest serviceRequest)
        {
            var existingService = await _serviceRepository.GetByIdAsync(id);
            if (existingService == null)
            {
                return null;
            }

            _mapper.Map(serviceRequest, existingService);
            existingService.Id = id; // Ensure ID remains the same
            var updatedService = await _serviceRepository.UpdateAsync(existingService);
            return _mapper.Map<ServiceResponse>(updatedService);
        }

        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            var deletedService = await _serviceRepository.DeleteAsync(id);
            return deletedService != null;
        }
    }
}
