using MeetingExtractor.Domain.Entities;

namespace MeetingExtractor.Api.GraphQL;
public class Subscription
{
    [Subscribe]
    [Topic("MeetingStatusChanged_{meetingId}")]
    public Meeting OnMeetingStatusChange(
        Guid meetingId,
        [EventMessage] Meeting meeting)
    {
        return meeting;
    }
}
