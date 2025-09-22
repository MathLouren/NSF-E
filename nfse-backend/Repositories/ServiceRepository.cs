using Microsoft.EntityFrameworkCore;
using nfse_backend.Data;
using nfse_backend.Models;

namespace nfse_backend.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task<Service> AddAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<Service?> UpdateAsync(Service service)
        {
            var existingService = await _context.Services.FindAsync(service.Id);
            if (existingService == null)
            {
                return null;
            }

            _context.Entry(existingService).CurrentValues.SetValues(service);
            await _context.SaveChangesAsync();
            return existingService;
        }

        public async Task<Service?> DeleteAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return null;
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return service;
        }
    }
}
