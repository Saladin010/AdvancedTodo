using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdvancedTodoLearningCards.Models
{
    public class CardSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CardId { get; set; }

        /// <summary>
        /// Number of times the card has been reviewed
        /// </summary>
        public int RepetitionNumber { get; set; } = 0;

        /// <summary>
        /// Easiness Factor (EF) - determines how easy/hard the card is (SM-2)
        /// Default: 2.5, Range: 1.3 to 3.5
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal EaseFactor { get; set; } = 2.5m;

        /// <summary>
        /// Interval in days until next review
        /// </summary>
        public int IntervalDays { get; set; } = 1;

        /// <summary>
        /// Last time the card was reviewed
        /// </summary>
        public DateTime? LastReviewedAt { get; set; }

        /// <summary>
        /// Next scheduled review date
        /// </summary>
        public DateTime NextReviewAt { get; set; } = DateTime.UtcNow.AddDays(1);

        /// <summary>
        /// Total number of reviews completed
        /// </summary>
        public int ReviewCount { get; set; } = 0;

        /// <summary>
        /// Number of times user failed to recall (lapses)
        /// </summary>
        public int LapseCount { get; set; } = 0;

        /// <summary>
        /// Current scheduling mode: Fixed or Adaptive
        /// </summary>
        public SchedulingMode SchedulingMode { get; set; } = SchedulingMode.Fixed;

        // Navigation property
        [ForeignKey(nameof(CardId))]
        public virtual Card Card { get; set; } = null!;
    }

    public enum SchedulingMode
    {
        Fixed = 0,      // Use initial fixed intervals [1,3,7,15,30]
        Adaptive = 1    // Use SM-2 or FSRS algorithm
    }
}