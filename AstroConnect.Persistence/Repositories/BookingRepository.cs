using AstroConnect.Application.Interfaces.Repositories;
using AstroConnect.Domain.Entities;
using AstroConnect.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AstroConnect.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetAllAsync()
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Service)
            .Include(b => b.Astrologer)
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Service)
            .Include(b => b.Astrologer)
            .Where(b => b.CustomerId == customerId && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Astrologer)
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking != null)
        {
            booking.IsDeleted = true;

            _context.Bookings.Update(booking);

            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTimeSlotAvailableAsync(
        int astrologerId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return !await _context.Bookings.AnyAsync(b =>
            b.AstrologerId == astrologerId &&
            b.BookingDate == bookingDate &&
            b.StartTime < endTime &&
            b.EndTime > startTime &&
            !b.IsDeleted);
    }

    public async Task<bool> IsTimeSlotAvailableForEditAsync(
        int bookingId,
        int astrologerId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return !await _context.Bookings.AnyAsync(b =>
            b.Id != bookingId &&
            b.AstrologerId == astrologerId &&
            b.BookingDate == bookingDate &&
            b.StartTime < endTime &&
            b.EndTime > startTime &&
            !b.IsDeleted);
    }
}