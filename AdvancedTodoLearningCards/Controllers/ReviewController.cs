using AdvancedTodoLearningCards.Services;
using AdvancedTodoLearningCards.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedTodoLearningCards.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Review/Session
        public async Task<IActionResult> Session()
        {
            var userId = GetUserId();
            var dueCards = await _reviewService.GetDueCardsAsync(userId);

            if (!dueCards.Any())
            {
                TempData["Info"] = "No cards are due for review right now. Great job!";
                return RedirectToAction("Upcoming");
            }

            var viewModel = new ReviewSessionViewModel
            {
                Cards = dueCards.Select(c => new CardReviewItem
                {
                    CardId = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    Tags = ParseTags(c.Tags),
                    Difficulty = c.Difficulty,
                    LastInterval = c.Schedule?.IntervalDays,
                    LastReviewedAt = c.Schedule?.LastReviewedAt
                }).ToList(),
                CurrentIndex = 0,
                TotalCards = dueCards.Count(),
                SessionStartTime = DateTime.UtcNow
            };

            // Store session in TempData for tracking
            TempData["SessionStart"] = viewModel.SessionStartTime.ToString("o");

            return View(viewModel);
        }

        // POST: Review/SubmitReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int cardId, int quality)
        {
            try
            {
                var userId = GetUserId();
                var reviewLog = await _reviewService.ProcessReviewAsync(cardId, userId, quality);

                return Json(new
                {
                    success = true,
                    result = new
                    {
                        quality = reviewLog.Quality,
                        cardId = reviewLog.CardId,
                        oldInterval = reviewLog.IntervalBefore,
                        newInterval = reviewLog.IntervalAfter,
                        oldEaseFactor = reviewLog.EaseFactorBefore,
                        newEaseFactor = reviewLog.EaseFactorAfter,
                        nextReviewDate = DateTime.UtcNow.AddDays(reviewLog.IntervalAfter)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing review for card {cardId}");
                return Json(new { success = false, message = "Error processing review" });
            }
        }

        // GET: Review/Upcoming
        public async Task<IActionResult> Upcoming()
        {
            var userId = GetUserId();
            var upcomingCards = await _reviewService.GetUpcomingReviewsAsync(userId, 14);

            var viewModel = new UpcomingReviewsViewModel
            {
                Reviews = upcomingCards.Select(c => new UpcomingReviewItem
                {
                    CardId = c.Id,
                    Title = c.Title,
                    NextReviewAt = c.Schedule!.NextReviewAt,
                    DaysUntilReview = (int)(c.Schedule.NextReviewAt.Date - DateTime.UtcNow.Date).TotalDays,
                    Difficulty = c.Difficulty
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Review/History
        public async Task<IActionResult> History()
        {
            var userId = GetUserId();
            var history = await _reviewService.GetReviewHistoryAsync(userId, 100);

            return View(history);
        }

        private List<string> ParseTags(string? tagsJson)
        {
            if (string.IsNullOrEmpty(tagsJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}