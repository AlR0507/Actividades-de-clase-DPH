using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(IArticleRepository articleRepository, ILogger<ArticlesController> logger)
        {
            _articleRepository = articleRepository;
            _logger = logger;
        }

        // GET: ArticlesController
        public ActionResult Index(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null,
            [FromQuery] string? authorEmail = null,
            [FromQuery] string? titleContains = null)
        {
                IEnumerable<Article> articles;

                    if (startDate.HasValue || endDate.HasValue)
                    {
                        var start = startDate ?? DateTimeOffset.MinValue;
                        var end = endDate ?? DateTimeOffset.MaxValue;
                        articles = _articleRepository.GetByDateRange(start, end);
                    }
                    else
                    {
                        articles = _articleRepository.GetAll();
                    }
                

                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                ViewBag.AuthorEmail = authorEmail;
                ViewBag.TitleContains = titleContains;

                return View(articles);
            }
        }

        // GET: ArticlesController/Details/5
        public ActionResult Details(int id)
        {
                var article = _articleRepository.GetById(id);
                if (article == null)
                {
                    return NotFound();
                }

                var comments = _articleRepository.GetCommentsByArticleId(id);
                var viewModel = new ArticleDetailsViewModel(article, comments);
                return View(viewModel);
            
        }

        // GET: ArticlesController/Create
        [HttpGet]
        public ActionResult Create()
        {
            _logger.LogInformation("GET Create action called");
            return View(new Article()); 
        }

        // POST: ArticlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] Article article)
        {
            _logger.LogInformation("POST Create action called");
               
                _logger.LogInformation("Attempting to create article with Title: {Title}, Author: {Author}, Email: {Email}",
                    article?.Title, article?.AuthorName, article?.AuthorEmail);

               
                if (article == null)
                {
                    _logger.LogError("Article is null");
                    ModelState.AddModelError("", "Article data is missing");
                    return View(new Article());
                }

               
                if (!ModelState.IsValid)
                {
                    var errors = new StringBuilder();
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        errors.AppendLine(modelError.ErrorMessage);
                    }
                    _logger.LogWarning("ModelState is invalid: {Errors}", errors.ToString());

                    return View(article);
                }

                if (string.IsNullOrWhiteSpace(article.Title) ||
                    string.IsNullOrWhiteSpace(article.AuthorName) ||
                    string.IsNullOrWhiteSpace(article.AuthorEmail) ||
                    string.IsNullOrWhiteSpace(article.Content))
                {
                    _logger.LogWarning("One or more required fields are empty");
                    ModelState.AddModelError("", "All fields are required");
                    return View(article);
                }

                article.PublishedDate = DateTimeOffset.UtcNow;
                article.Comments = new List<Comment>();

                _logger.LogInformation("About to call repository.Create");

                Article created = _articleRepository.Create(article);

                _logger.LogInformation("Article created successfully with ID: {Id}", created.Id);

                if (created == null || created.Id <= 0)
                {
                    _logger.LogError("Article was not created properly");
                    ModelState.AddModelError("", "Failed to create the article");
                    return View(article);
                }

                TempData["Success"] = "Article created successfully!";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int articleId, Comment comment)
        {

                Article? article = _articleRepository.GetById(articleId);
                if (article == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrWhiteSpace(comment.Content))
                {
                    TempData["Error"] = "Comment content cannot be empty.";
                    return RedirectToAction(nameof(Details), new { id = articleId });
                }

                comment.ArticleId = articleId;
                comment.PublishedDate = DateTimeOffset.UtcNow;
                _articleRepository.AddComment(comment);

                _logger.LogInformation("Comment added to article ID: {ArticleId}", articleId);
                TempData["Success"] = "Comment added successfully!";

                return RedirectToAction(nameof(Details), new { id = articleId });
            
        }
    }
}