using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdvancedTodoLearningCards.Models
{
    public class AlgorithmSettings
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User ID for user-specific settings, null for global settings
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Initial fixed intervals in days (JSON array)
        /// Example: "[1,3,7,15,30]"
        /// </summary>
        [MaxLength(100)]
        public string InitialIntervals { get; set; } = "[1,3,7,15,30]";

        /// <summary>
        /// Initial Ease Factor for new cards (SM-2)
        /// Default: 2.5
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal InitialEaseFactor { get; set; } = 2.5m;

        /// <summary>
        /// Minimum allowed Ease Factor
        /// Default: 1.3
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal MinimumEaseFactor { get; set; } = 1.3m;

        /// <summary>
        /// Maximum allowed Ease Factor
        /// Default: 3.5
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal MaximumEaseFactor { get; set; } = 3.5m;

        /// <summary>
        /// Default scheduling mode for new cards
        /// </summary>
        public SchedulingMode DefaultSchedulingMode { get; set; } = SchedulingMode.Fixed;

        /// <summary>
        /// When to switch from Fixed to Adaptive mode
        /// Number of repetitions completed
        /// </summary>
        public int SwitchToAdaptiveAfterReps { get; set; } = 5;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}