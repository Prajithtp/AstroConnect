using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<Customer?> GetByUserIdAsync(string userId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
    }
    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.FullName)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await GetByIdAsync(id);

        if (customer == null)
            return;

        customer.IsDeleted = true;

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }
}