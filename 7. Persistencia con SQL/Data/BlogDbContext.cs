using Microsoft.EntityFrameworkCore;
using Blog.Models;

namespace Blog.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .HasMany(a => a.Comments)
                .WithOne(c => c.Article)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Article>()
                .Property(a => a.PublishedDate)
                .HasConversion(v => v.ToString("o"), v => DateTimeOffset.Parse(v));
                
            modelBuilder.Entity<Comment>()
                .Property(c => c.PublishedDate)
                .HasConversion(v => v.ToString("o"), v => DateTimeOffset.Parse(v));
        }
    }
}
