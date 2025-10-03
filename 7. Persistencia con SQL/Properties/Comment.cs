using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        /// <summary>
        /// The identifier of the article this comment belongs to.
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// The content of the comment.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Represents the moment the comment was posted.
        /// </summary>
        public DateTimeOffset PublishedDate { get; set; }

        [ForeignKey("ArticleId")]
        public Article Article { get; set; }
    }
}
