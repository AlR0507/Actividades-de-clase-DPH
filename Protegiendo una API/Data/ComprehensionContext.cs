using Microsoft.EntityFrameworkCore;
using Comprehension.Models;

namespace Comprehension.Data
{
    public class ComprehensionContext : DbContext
    {
        public ComprehensionContext(DbContextOptions<ComprehensionContext> options) : base(options) { }

        public DbSet<Reminder> Reminder { get; set; } = default!;
        public DbSet<Event> Event { get; set; } = default!;
        public DbSet<Note> Note { get; set; } = default!;

        // NUEVOS
        public DbSet<User> User { get; set; } = default!;
        public DbSet<SessionToken> SessionToken { get; set; } = default!;
        public DbSet<SharedAccess> SharedAccess { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            b.Entity<SessionToken>().HasIndex(t => t.TokenHash).IsUnique();

            b.Entity<Note>().HasIndex(n => n.OwnerUserId);
            b.Entity<Event>().HasIndex(e => e.OwnerUserId);
            b.Entity<Reminder>().HasIndex(r => r.OwnerUserId);

            b.Entity<SharedAccess>()
             .HasIndex(s => new { s.ResourceType, s.ResourceId, s.GranteeUserId })
             .IsUnique();
        }
    }
}
