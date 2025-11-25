using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.Services;
using AdvancedTodoLearningCards.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedTodoLearningCards.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardsController> _logger;

        public CardsController(ICardService cardService, ILogger<CardsController> logger)
        {
            _cardService = cardService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Cards
        public async Task<IActionResult> Index(string? search, string? tag)
        {
            var userId = GetUserId();
            IEnumerable<Card> cards;

            if (!string.IsNullOrWhiteSpace(search))
            {
                cards = await _cardService.SearchCardsAsync(userId, search);
            }
            else if (!string.IsNullOrWhiteSpace(tag))
            {
                cards = await _cardService.GetCardsByTagAsync(userId, tag);
            }
            else
            {
                cards = await _cardService.GetAllCardsAsync(userId);
            }

            var tagStats = await _cardService.GetTagsStatisticsAsync(userId);

            var viewModel = new CardListViewModel
            {
                Cards = cards.Select(MapToCardViewModel),
                SearchTerm = search,
                SelectedTag = tag,
                TagStatistics = tagStats
            };

            return View(viewModel);
        }

        // GET: Cards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCardViewModel model)
        {
            if (ModelState.IsValid)
            {
                var card = new Card
                {
                    UserId = GetUserId(),
                    Title = model.Title,
                    Content = model.Content,
                    Difficulty = model.Difficulty,
                    ImageUrl = model.ImageUrl
                };

                // Process tags
                if (!string.IsNullOrWhiteSpace(model.TagsString))
                {
                    var tags = model.TagsString.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToArray();

                    card.Tags = JsonSerializer.Serialize(tags);
                }

                await _cardService.CreateCardAsync(card);
                TempData["Success"] = "Card created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Cards/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id, GetUserId());
            if (card == null)
            {
                return NotFound();
            }

            var model = new EditCardViewModel
            {
                Id = card.Id,
                Title = card.Title,
                Content = card.Content,
                Difficulty = card.Difficulty,
                ImageUrl = card.ImageUrl,
                TagsString = card.Tags != null
                    ? string.Join(", ", JsonSerializer.Deserialize<string[]>(card.Tags) ?? Array.Empty<string>())
                    : string.Empty
            };

            return View(model);
        }

        // POST: Cards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCardViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var card = await _cardService.GetCardByIdAsync(id, GetUserId());
                if (card == null)
                {
                    return NotFound();
                }

                card.Title = model.Title;
                card.Content = model.Content;
                card.Difficulty = model.Difficulty;
                card.ImageUrl = model.ImageUrl;

                // Process tags
                if (!string.IsNullOrWhiteSpace(model.TagsString))
                {
                    var tags = model.TagsString.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToArray();

                    card.Tags = JsonSerializer.Serialize(tags);
                }
                else
                {
                    card.Tags = null;
                }

                await _cardService.UpdateCardAsync(card);
                TempData["Success"] = "Card updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Cards/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id, GetUserId());
            if (card == null)
            {
                return NotFound();
            }

            var viewModel = MapToCardViewModel(card);
            return View(viewModel);
        }

        // GET: Cards/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id, GetUserId());
            if (card == null)
            {
                return NotFound();
            }

            var viewModel = MapToCardViewModel(card);
            return View(viewModel);
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _cardService.DeleteCardAsync(id, GetUserId());
            if (success)
            {
                TempData["Success"] = "Card deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete card.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cards/BulkImport
        public IActionResult BulkImport()
        {
            return View(new BulkImportViewModel());
        }

        // POST: Cards/BulkImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(BulkImportViewModel model)
        {
            string csvContent = string.Empty;

            if (model.CsvFile != null && model.CsvFile.Length > 0)
            {
                using var reader = new StreamReader(model.CsvFile.OpenReadStream());
                csvContent = await reader.ReadToEndAsync();
            }
            else if (!string.IsNullOrWhiteSpace(model.CsvContent))
            {
                csvContent = model.CsvContent;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please provide a CSV file or paste CSV content.");
                return View(model);
            }

            try
            {
                var count = await _cardService.ImportCardsFromCsvAsync(GetUserId(), csvContent);
                TempData["Success"] = $"Successfully imported {count} cards!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing cards");
                ModelState.AddModelError(string.Empty, "Error importing cards. Please check the format.");
                return View(model);
            }
        }

        private CardViewModel MapToCardViewModel(Card card)
        {
            var tags = new List<string>();
            if (!string.IsNullOrEmpty(card.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize<List<string>>(card.Tags) ?? new List<string>();
                }
                catch { }
            }

            return new CardViewModel
            {
                Id = card.Id,
                Title = card.Title,
                Content = card.Content,
                Tags = tags,
                Difficulty = card.Difficulty,
                CreatedAt = card.CreatedAt,
                NextReviewAt = card.Schedule?.NextReviewAt,
                IntervalDays = card.Schedule?.IntervalDays,
                EaseFactor = card.Schedule?.EaseFactor,
                ReviewCount = card.Schedule?.ReviewCount ?? 0,
                IsDueToday = card.Schedule?.NextReviewAt <= DateTime.UtcNow,
                ImageUrl = card.ImageUrl
            };
        }
    }
}