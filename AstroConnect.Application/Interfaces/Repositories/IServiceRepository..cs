using AstroConnect.Domain.Entities;

namespace AstroConnect.Application.Interfaces.Repositories;

public interface IServiceRepository
{
    Task<List<Service>> GetAllAsync();

    Task<Service?> GetByIdAsync(int id);

    Task AddAsync(Service service);

    Task UpdateAsync(Service service);

    Task DeleteAsync(int id);
}
