

using HotChocolate.Subscriptions;
using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Enums;
using MeetingExtractor.Domain.Interfaces;

using System.Text.Json;

namespace MeetingExtractor.Application.Meetings;

public interface IMeetingProcessingService
{
    Task ProcessMeetingAudioAsync(Guid meetingId);
}

public class MeetingProcessingService : IMeetingProcessingService
{
    private readonly IWhisperService _whisperService;
    private readonly IAiProvider _aiProvider;
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingEventPublisher _eventPublisher;


    public MeetingProcessingService(
        IWhisperService whisperService,
        IAiProvider aiProvider,
        IMeetingRepository meetingRepository,
        IMeetingEventPublisher eventPublisher)
    {
        _whisperService = whisperService;
        _aiProvider = aiProvider;
        _meetingRepository = meetingRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task ProcessMeetingAudioAsync(Guid meetingId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting is null || meeting.AudioUrl is null)
            return;

        meeting.Status = MeetingStatus.Transcribing;
        await _meetingRepository.SaveChangesAsync();
        await _eventPublisher.PublishMeetingStatusChangedAsync(meeting);

        var transcript = await _whisperService.TranscribeAsync(meeting.AudioUrl);
        meeting.Transcript = transcript;
        meeting.Status = MeetingStatus.Extracting;
        await _meetingRepository.SaveChangesAsync();
        await _eventPublisher.PublishMeetingStatusChangedAsync(meeting);

        var prompt = $$"""
            You are an assistant that extracts structured information from meeting transcripts.
            Given the transcript below, return ONLY a valid JSON object, no extra text, no markdown code block, in this exact format:
            {
              "summary": "a short 2-3 sentence summary of the meeting",
              "action_items": [
                { "task": "description of the task", "owner": "person responsible or null", "deadline": "YYYY-MM-DD or null" }
              ]
            }

            Transcript:
            {{transcript}}
            """;

        var aiResponse = await _aiProvider.GenerateReplyAsync(prompt);

        try
        {
            using var document = JsonDocument.Parse(aiResponse);
            var root = document.RootElement;

            meeting.Summary = root.GetProperty("summary").GetString();

            if (root.TryGetProperty("action_items", out var actionItemsElement))
            {
                foreach (var item in actionItemsElement.EnumerateArray())
                {
                    var actionItem = new ActionItem
                    {
                        Id = Guid.NewGuid(),
                        MeetingId = meeting.Id,
                        Task = item.GetProperty("task").GetString() ?? string.Empty,
                        Owner = item.TryGetProperty("owner", out var owner) ? owner.GetString() : null,
                        Deadline = item.TryGetProperty("deadline", out var deadline)
                            && DateTime.TryParse(deadline.GetString(), out var parsedDate)
                                ? parsedDate
                                : null
                    };
                    meeting.ActionItems.Add(actionItem);
                }
            }

            meeting.Status = MeetingStatus.Done;
        }
        catch (JsonException)
        {
            meeting.Status = MeetingStatus.Failed;
        }

        await _meetingRepository.SaveChangesAsync();
        await _eventPublisher.PublishMeetingStatusChangedAsync(meeting);
    }
}
