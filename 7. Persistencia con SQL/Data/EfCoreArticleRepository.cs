using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    public class EfCoreArticleRepository : IArticleRepository
    {
        private readonly BlogDbContext _context;

        public EfCoreArticleRepository(BlogDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Article> GetAll()
        {
            return _context.Articles
                .Include(a => a.Comments)
                .OrderByDescending(a => a.PublishedDate)
                .ToList();
        }

        public IEnumerable<Article> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return _context.Articles
                .Include(a => a.Comments)
                .Where(a => a.PublishedDate >= startDate && a.PublishedDate <= endDate)
                .OrderByDescending(a => a.PublishedDate)
                .ToList();
        }

        public Article? GetById(int id)
        {
            return _context.Articles
                .Include(a => a.Comments)
                .FirstOrDefault(a => a.Id == id);
        }

        public Article Create(Article article)
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            // Asegurar que PublishedDate esté configurado
            if (article.PublishedDate == default)
            {
                article.PublishedDate = DateTimeOffset.UtcNow;
            }

            // Inicializar la colección de comentarios si es null
            article.Comments ??= new List<Comment>();

            _context.Articles.Add(article);
            _context.SaveChanges();
            return article;
        }

        public IEnumerable<Comment> GetCommentsByArticleId(int articleId)
        {
            return _context.Comments
                .Where(c => c.ArticleId == articleId)
                .OrderByDescending(c => c.PublishedDate)
                .ToList();
        }

        public void AddComment(Comment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            if (!_context.Articles.Any(a => a.Id == comment.ArticleId))
                throw new ArgumentException("Article does not exist.", nameof(comment));

            if (comment.PublishedDate == default)
            {
                comment.PublishedDate = DateTimeOffset.UtcNow;
            }

            _context.Comments.Add(comment);
            _context.SaveChanges();
        }
    }
}