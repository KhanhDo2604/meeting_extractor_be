namespace MeetingExtractor.Domain.Entities
{
    public class ActionItem
    {
        public Guid Id { get; set; }
        public string Task { get; set; } = string.Empty;
        public string? Owner { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsDone { get; set; } = false;

        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;
    }
}
