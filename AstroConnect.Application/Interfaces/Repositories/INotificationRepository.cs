using AstroConnect.Domain.Entities;

namespace AstroConnect.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(string userId);

    Task<List<Notification>> GetUnreadByUserIdAsync(string userId);

    Task<int> GetUnreadCountAsync(string userId);

    Task<Notification?> GetByIdAsync(int id);

    Task AddAsync(Notification notification);

    Task MarkAsReadAsync(int id);

    Task MarkAllAsReadAsync(string userId);
}