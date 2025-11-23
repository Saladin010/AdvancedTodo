using AdvancedTodoLearningCards.Data;
using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvancedTodoLearningCards.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Settings
        public async Task<IActionResult> Settings()
        {
            var globalSettings = await _context.AlgorithmSettings
                .FirstOrDefaultAsync(s => s.UserId == null);

            if (globalSettings == null)
            {
                globalSettings = new AlgorithmSettings
                {
                    UserId = null,
                    InitialIntervals = "[1,3,7,15,30]",
                    InitialEaseFactor = 2.5m,
                    MinimumEaseFactor = 1.3m,
                    MaximumEaseFactor = 3.5m,
                    DefaultSchedulingMode = SchedulingMode.Fixed,
                    SwitchToAdaptiveAfterReps = 5
                };
            }

            var viewModel = new AdminSettingsViewModel
            {
                InitialIntervals = globalSettings.InitialIntervals,
                InitialEaseFactor = globalSettings.InitialEaseFactor,
                MinimumEaseFactor = globalSettings.MinimumEaseFactor,
                MaximumEaseFactor = globalSettings.MaximumEaseFactor,
                DefaultSchedulingMode = globalSettings.DefaultSchedulingMode.ToString(),
                SwitchToAdaptiveAfterReps = globalSettings.SwitchToAdaptiveAfterReps
            };

            return View(viewModel);
        }

        // POST: Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AdminSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var settings = await _context.AlgorithmSettings
                        .FirstOrDefaultAsync(s => s.UserId == null);

                    if (settings == null)
                    {
                        settings = new AlgorithmSettings { UserId = null };
                        _context.AlgorithmSettings.Add(settings);
                    }

                    settings.InitialIntervals = model.InitialIntervals;
                    settings.InitialEaseFactor = model.InitialEaseFactor;
                    settings.MinimumEaseFactor = model.MinimumEaseFactor;
                    settings.MaximumEaseFactor = model.MaximumEaseFactor;
                    settings.DefaultSchedulingMode = Enum.Parse<SchedulingMode>(model.DefaultSchedulingMode);
                    settings.SwitchToAdaptiveAfterReps = model.SwitchToAdaptiveAfterReps;
                    settings.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Algorithm settings updated successfully!";
                    _logger.LogInformation("Admin updated algorithm settings");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating algorithm settings");
                    TempData["Error"] = "Failed to update settings. Please check your input.";
                }
            }

            return View(model);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.CreatedAt,
                    u.LastLoginAt,
                    CardCount = u.Cards.Count,
                    ReviewCount = u.ReviewLogs.Count
                })
                .ToListAsync();

            return View(users);
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalCards = await _context.Cards.CountAsync(),
                TotalReviews = await _context.ReviewLogs.CountAsync(),
                ActiveUsersToday = await _context.ReviewLogs
                    .Where(r => r.ReviewedAt.Date == DateTime.UtcNow.Date)
                    .Select(r => r.UserId)
                    .Distinct()
                    .CountAsync()
            };

            return View(stats);
        }
    }
}
