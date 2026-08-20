using AstroConnect.Domain.Entities;

namespace AstroConnect.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();

    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByUserIdAsync(string userId);

    Task AddAsync(Customer customer);

    Task UpdateAsync(Customer customer);

    Task DeleteAsync(int id);
}