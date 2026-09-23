using MeetingExtractor.Domain.Entities;

namespace MeetingExtractor.Application.Interfaces;

public interface IMeetingEventPublisher
{
    Task PublishMeetingStatusChangedAsync(Meeting meeting);
}
