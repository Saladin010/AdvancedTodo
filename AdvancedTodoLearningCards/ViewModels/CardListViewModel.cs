using AdvancedTodoLearningCards.Models;
using System.ComponentModel.DataAnnotations;

namespace AdvancedTodoLearningCards.ViewModels
{
    public class CardListViewModel
    {
        public IEnumerable<CardViewModel> Cards { get; set; } = new List<CardViewModel>();
        public string? SearchTerm { get; set; }
        public string? SelectedTag { get; set; }
        public Dictionary<string, int> TagStatistics { get; set; } = new Dictionary<string, int>();
    }

    public class CardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public CardDifficulty Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? NextReviewAt { get; set; }
        public int? IntervalDays { get; set; }
        public decimal? EaseFactor { get; set; }
        public int ReviewCount { get; set; }
        public bool IsDueToday { get; set; }
    }

    public class CreateCardViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Card Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Card Content")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Tags (comma-separated)")]
        public string? TagsString { get; set; }

        [Display(Name = "Difficulty Level")]
        public CardDifficulty Difficulty { get; set; } = CardDifficulty.Medium;
    }

    public class EditCardViewModel : CreateCardViewModel
    {
        public int Id { get; set; }
    }

    public class BulkImportViewModel
    {
        [Display(Name = "CSV File")]
        public IFormFile? CsvFile { get; set; }

        [Display(Name = "Or paste CSV content here")]
        [DataType(DataType.MultilineText)]
        public string? CsvContent { get; set; }

        public string Instructions { get; set; } =
            "CSV Format: Title,Content,Tags (semicolon-separated),Difficulty\n" +
            "Example: What is SM-2?,SuperMemo 2 algorithm,Algorithm;Learning,Medium";
    }
}