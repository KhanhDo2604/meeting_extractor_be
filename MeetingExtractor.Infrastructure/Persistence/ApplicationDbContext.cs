using MeetingExtractor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetingExtractor.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<ActionItem> ActionItems => Set<ActionItem>();
    }
}
