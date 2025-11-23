using AdvancedTodoLearningCards.Services;
using AdvancedTodoLearningCards.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedTodoLearningCards.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IReviewService _reviewService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            IReviewService reviewService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _reviewService = reviewService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var stats = await _dashboardService.GetDashboardStatsAsync(userId);
            var reviewChartData = await _dashboardService.GetReviewChartDataAsync(userId, 30);
            var tagPerformance = await _dashboardService.GetTagPerformanceAsync(userId);
            var difficultyDistribution = await _dashboardService.GetDifficultyDistributionAsync(userId);
            var upcomingReviews = await _reviewService.GetUpcomingReviewsAsync(userId, 7);

            var viewModel = new DashboardViewModel
            {
                TotalCards = stats.TotalCards,
                DueToday = stats.DueToday,
                ReviewedToday = stats.ReviewedToday,
                TotalReviews = stats.TotalReviews,
                AverageQuality = stats.AverageQuality,
                RetentionRate = stats.RetentionRate,
                CurrentStreak = stats.CurrentStreak,

                ReviewsOverTime = new ChartDataViewModel
                {
                    Labels = reviewChartData.Keys.OrderBy(k => k).Select(k => k.ToString("MMM dd")).ToList(),
                    Data = reviewChartData.OrderBy(k => k.Key).Select(k => k.Value).ToList()
                },

                TagPerformance = tagPerformance,
                DifficultyDistribution = difficultyDistribution,

                UpcomingReviews = upcomingReviews.Take(5).Select(c => new UpcomingReviewPreview
                {
                    CardId = c.Id,
                    Title = c.Title,
                    NextReviewAt = c.Schedule!.NextReviewAt,
                    RelativeTime = GetRelativeTime(c.Schedule.NextReviewAt)
                }).ToList()
            };

            return View(viewModel);
        }

        // API endpoint for chart data
        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType, int days = 30)
        {
            var userId = GetUserId();

            switch (chartType.ToLower())
            {
                case "reviews":
                    var reviewData = await _dashboardService.GetReviewChartDataAsync(userId, days);
                    return Json(new
                    {
                        labels = reviewData.Keys.OrderBy(k => k).Select(k => k.ToString("MMM dd")).ToList(),
                        data = reviewData.OrderBy(k => k.Key).Select(k => k.Value).ToList()
                    });

                case "tags":
                    var tagData = await _dashboardService.GetTagPerformanceAsync(userId);
                    return Json(new
                    {
                        labels = tagData.Keys.ToList(),
                        data = tagData.Values.ToList()
                    });

                case "difficulty":
                    var difficultyData = await _dashboardService.GetDifficultyDistributionAsync(userId);
                    return Json(new
                    {
                        labels = difficultyData.Keys.ToList(),
                        data = difficultyData.Values.ToList()
                    });

                default:
                    return BadRequest("Invalid chart type");
            }
        }

        // Export data as CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            var userId = GetUserId();
            var stats = await _dashboardService.GetDashboardStatsAsync(userId);
            var reviewHistory = await _reviewService.GetReviewHistoryAsync(userId, 1000);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,CardTitle,Quality,IntervalBefore,IntervalAfter,EaseFactorBefore,EaseFactorAfter");

            foreach (var review in reviewHistory)
            {
                csv.AppendLine($"{review.ReviewedAt:yyyy-MM-dd HH:mm:ss}," +
                             $"\"{review.Card.Title}\"," +
                             $"{review.Quality}," +
                             $"{review.IntervalBefore}," +
                             $"{review.IntervalAfter}," +
                             $"{review.EaseFactorBefore}," +
                             $"{review.EaseFactorAfter}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"learning-cards-export-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private string GetRelativeTime(DateTime date)
        {
            var diff = date - DateTime.UtcNow;

            if (diff.TotalDays < 1)
                return "Today";
            else if (diff.TotalDays < 2)
                return "Tomorrow";
            else if (diff.TotalDays < 7)
                return $"In {(int)diff.TotalDays} days";
            else
                return $"In {(int)(diff.TotalDays / 7)} weeks";
        }
    }
}