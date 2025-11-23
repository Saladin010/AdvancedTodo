using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdvancedTodoLearningCards.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Tags { get; set; } // Stored as JSON array: ["tag1", "tag2"]

        public CardDifficulty Difficulty { get; set; } = CardDifficulty.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual CardSchedule? Schedule { get; set; }
        public virtual ICollection<ReviewLog> ReviewLogs { get; set; } = new List<ReviewLog>();
    }

    public enum CardDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
}