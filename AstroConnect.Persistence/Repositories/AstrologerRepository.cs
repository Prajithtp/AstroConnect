
using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Persistence.Repositories;

public class AstrologerRepository : IAstrologerRepository
{
    private readonly ApplicationDbContext _context;

    public AstrologerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Astrologer>> GetAllAsync()
    {
        return await _context.Astrologers
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.FullName)
            .ToListAsync();
    }

    public async Task<Astrologer?> GetByIdAsync(int id)
    {
        return await _context.Astrologers
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task AddAsync(Astrologer astrologer)
    {
        _context.Astrologers.Add(astrologer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Astrologer astrologer)
    {
        _context.Astrologers.Update(astrologer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var astrologer = await GetByIdAsync(id);

        if (astrologer == null)
            return;

        astrologer.IsDeleted = true;

        _context.Astrologers.Update(astrologer);
        await _context.SaveChangesAsync();
    }
}