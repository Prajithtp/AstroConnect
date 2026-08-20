using AstroConnect.Domain.Entities;

namespace AstroConnect.Application.Interfaces.Repositories;

public interface IAstrologerRepository
{
    Task<List<Astrologer>> GetAllAsync();

    Task<Astrologer?> GetByIdAsync(int id);

    Task AddAsync(Astrologer astrologer);

    Task UpdateAsync(Astrologer astrologer);

    Task DeleteAsync(int id);
}


