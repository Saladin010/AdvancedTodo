using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdvancedTodoLearningCards.Models
{
    /// </summary>
    public class ReviewLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CardId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the review occurred
        /// </summary>
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Quality of recall (0-5)
        /// 0: Complete blackout
        /// 1: Incorrect, but remembered
        /// 2: Incorrect, seemed easy
        /// 3: Correct, but difficult
        /// 4: Correct, with hesitation
        /// 5: Perfect recall
        /// </summary>
        [Range(0, 5)]
        public int Quality { get; set; }

        /// <summary>
        /// Interval before this review (in days)
        /// </summary>
        public int IntervalBefore { get; set; }

        /// <summary>
        /// Interval after this review (in days)
        /// </summary>
        public int IntervalAfter { get; set; }

        /// <summary>
        /// Ease Factor before this review
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal EaseFactorBefore { get; set; }

        /// <summary>
        /// Ease Factor after this review
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal EaseFactorAfter { get; set; }

        /// <summary>
        /// Time spent on review in seconds
        /// </summary>
        public int? TimeSpentSeconds { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CardId))]
        public virtual Card Card { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}