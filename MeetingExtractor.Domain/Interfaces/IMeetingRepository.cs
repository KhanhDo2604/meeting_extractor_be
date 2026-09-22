using MeetingExtractor.Domain.Entities;

namespace MeetingExtractor.Domain.Interfaces
{
    public interface IMeetingRepository
    {
        Task<Meeting?> GetByIdAsync(Guid id);
        Task SaveChangesAsync();
    }
}
