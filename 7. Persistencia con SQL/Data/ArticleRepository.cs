using Blog.Models;
using Microsoft.Data.Sqlite;

namespace Blog.Data
{
    /// <summary>
    /// Implementation of <see cref="IArticleRepository"/> using SQLite as a persistence solution.
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly string _connectionString;

        public ArticleRepository(DatabaseConfig _config)
        {
            _connectionString = _config.DefaultConnectionString ?? throw new ArgumentNullException("Connection string not found");
        }

        /// <summary>
        /// Creates the necessary tables for this application if they don't exist already.
        /// Should be called once when starting the service.
        /// </summary>
        public void EnsureCreated()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createArticleTable = @"
        CREATE TABLE IF NOT EXISTS Article (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AuthorName TEXT NOT NULL,
            AuthorEmail TEXT NOT NULL,
            Title TEXT NOT NULL,
            Content TEXT NOT NULL,
            PublishedDate TEXT NOT NULL
        );";

            var createCommentTable = @"
        CREATE TABLE IF NOT EXISTS Comment (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ArticleId INTEGER NOT NULL,
            Content TEXT NOT NULL,
            PublishedDate TEXT NOT NULL,
            FOREIGN KEY (ArticleId) REFERENCES Article(Id) ON DELETE CASCADE
        );";

            using var cmd1 = new SqliteCommand(createArticleTable, connection);
            cmd1.ExecuteNonQuery();

            using var cmd2 = new SqliteCommand(createCommentTable, connection);
            cmd2.ExecuteNonQuery();
        }

        public IEnumerable<Article> GetAll()
        {
            var articles = new List<Article>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate FROM Article ORDER BY PublishedDate DESC;";
            using var cmd = new SqliteCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                articles.Add(new Article
                {
                    Id = reader.GetInt32(0),
                    AuthorName = reader.GetString(1),
                    AuthorEmail = reader.GetString(2),
                    Title = reader.GetString(3),
                    Content = reader.GetString(4),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
                });
            }
            return articles;
        }

        public IEnumerable<Article> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var articles = new List<Article>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
        SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate
        FROM Article
        WHERE PublishedDate >= @startDate AND PublishedDate <= @endDate
        ORDER BY PublishedDate DESC;";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("o"));
            cmd.Parameters.AddWithValue("@endDate", endDate.ToString("o"));
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                articles.Add(new Article
                {
                    Id = reader.GetInt32(0),
                    AuthorName = reader.GetString(1),
                    AuthorEmail = reader.GetString(2),
                    Title = reader.GetString(3),
                    Content = reader.GetString(4),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
                });
            }
            return articles;
        }

        public Article? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate FROM Article WHERE Id = @id;";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Article
                {
                    Id = reader.GetInt32(0),
                    AuthorName = reader.GetString(1),
                    AuthorEmail = reader.GetString(2),
                    Title = reader.GetString(3),
                    Content = reader.GetString(4),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
                };
            }
            return null;
        }

        public Article Create(Article article)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
        INSERT INTO Article (AuthorName, AuthorEmail, Title, Content, PublishedDate)
        VALUES (@AuthorName, @AuthorEmail, @Title, @Content, @PublishedDate);
        SELECT last_insert_rowid();";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@AuthorName", article.AuthorName);
            cmd.Parameters.AddWithValue("@AuthorEmail", article.AuthorEmail);
            cmd.Parameters.AddWithValue("@Title", article.Title);
            cmd.Parameters.AddWithValue("@Content", article.Content);
            var publishedDate = DateTimeOffset.UtcNow;
            cmd.Parameters.AddWithValue("@PublishedDate", publishedDate.ToString("o"));
            var id = (long)cmd.ExecuteScalar();

            article.Id = (int)id;
            article.PublishedDate = publishedDate;
            return article;
        }

        public void AddComment(Comment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            // Ensure the article exists
            if (GetById(comment.ArticleId) == null)
                throw new ArgumentException("Article does not exist.", nameof(comment));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
        INSERT INTO Comment (ArticleId, Content, PublishedDate)
        VALUES (@ArticleId, @Content, @PublishedDate);";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@ArticleId", comment.ArticleId);
            cmd.Parameters.AddWithValue("@Content", comment.Content);
            cmd.Parameters.AddWithValue("@PublishedDate", DateTimeOffset.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<Comment> GetCommentsByArticleId(int articleId)
        {
            var comments = new List<Comment>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT ArticleId, Content, PublishedDate FROM Comment WHERE ArticleId = @articleId ORDER BY PublishedDate DESC;";
            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@articleId", articleId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                comments.Add(new Comment
                {
                    ArticleId = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(2))
                });
            }
            return comments;
        }
    }
}
