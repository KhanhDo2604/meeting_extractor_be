using HotChocolate.Subscriptions;
using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Domain.Entities;

namespace MeetingExtractor.Infrastructure.Events;
public class MeetingEventPublisher : IMeetingEventPublisher
{
    private readonly ITopicEventSender _eventSender;

    public MeetingEventPublisher(ITopicEventSender eventSender)
    {
        _eventSender = eventSender;
    }

    public async Task PublishMeetingStatusChangedAsync(Meeting meeting)
    {
        await _eventSender.SendAsync($"MeetingStatusChanged_{meeting.Id}", meeting);
    }
}
