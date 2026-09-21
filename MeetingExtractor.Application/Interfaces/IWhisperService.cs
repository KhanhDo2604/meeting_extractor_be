namespace MeetingExtractor.Application.Interfaces;
public interface IWhisperService
{
    Task<string> TranscribeAsync(string audioFilePath);
}
