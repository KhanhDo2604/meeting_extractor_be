namespace MeetingExtractor.Application.Interfaces
{
    public interface IAiProvider
    {
        Task<string> GenerateReplyAsync(string prompt);
    }
}
