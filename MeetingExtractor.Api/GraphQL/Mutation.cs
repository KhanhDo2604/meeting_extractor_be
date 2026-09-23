using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Enums;
using MeetingExtractor.Infrastructure.Persistence;

namespace MeetingExtractor.Api.GraphQL;
public class Mutation
{
    public async Task<Meeting> CreateMeeting(
        [Service] ApplicationDbContext context,
        string title)
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = title,
            Status = MeetingStatus.Pending
        };

        context.Meetings.Add(meeting);
        await context.SaveChangesAsync();

        return meeting;
    }
}
