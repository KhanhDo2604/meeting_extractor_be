using MeetingExtractor.Domain.Enums;

namespace MeetingExtractor.Domain.Entities
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public string? Transcript { get; set; }
        public string? Summary { get; set; }
        public MeetingStatus Status { get; set; } = MeetingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ActionItem> ActionItems { get; set; } = new();
    }
}
