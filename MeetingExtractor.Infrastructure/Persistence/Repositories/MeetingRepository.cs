using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeetingExtractor.Infrastructure.Persistence.Repositories
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly ApplicationDbContext _context;

        public MeetingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Meeting?> GetByIdAsync(Guid id)
        {
            return await _context.Meetings
                .Include(m => m.ActionItems)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
