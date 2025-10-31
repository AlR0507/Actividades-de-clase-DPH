using Microsoft.EntityFrameworkCore;
using Protegiendo_un_sitio_web.Models;


namespace Protegiendo_un_sitio_web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> Sessions => Set<UserSession>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Username).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
            });


            modelBuilder.Entity<UserSession>(e =>
            {
                e.HasIndex(s => s.SessionId).IsUnique();
            });
        }
    }
}
