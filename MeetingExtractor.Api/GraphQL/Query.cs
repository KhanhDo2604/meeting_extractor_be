using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeetingExtractor.Api.GraphQL;
public class Query
{
    public async Task<List<Meeting>> GetMeetings([Service] ApplicationDbContext context)
    {
        return await context.Meetings.ToListAsync();
    }

    public async Task<Meeting?> GetMeetingById([Service] ApplicationDbContext context, Guid id)
    {
        return await context.Meetings
            .Include(m => m.ActionItems)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
