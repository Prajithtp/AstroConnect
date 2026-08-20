using AstroConnect.Domain.Entities;

namespace AstroConnect.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<List<Booking>> GetAllAsync();

    Task<List<Booking>> GetByCustomerIdAsync(int customerId);

    Task<Booking?> GetByIdAsync(int id);

    Task AddAsync(Booking booking);

    Task UpdateAsync(Booking booking);

    Task DeleteAsync(int id);

    Task<bool> IsTimeSlotAvailableAsync(
        int astrologerId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime);

    Task<bool> IsTimeSlotAvailableForEditAsync(
        int bookingId,
        int astrologerId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime);
}