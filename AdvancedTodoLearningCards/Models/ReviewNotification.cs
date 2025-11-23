using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvancedTodoLearningCards.Models
{
    /// <summary>
    /// Tracks review notifications sent to users and their acknowledgment status
    /// </summary>
    public class ReviewNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CardId { get; set; }

        /// <summary>
        /// When the notification was sent to the user
        /// </summary>
        public DateTime NotifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the user has acknowledged/checked this notification
        /// </summary>
        public bool IsAcknowledged { get; set; } = false;

        /// <summary>
        /// When the user acknowledged the notification
        /// </summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// The scheduled review time for the card
        /// </summary>
        public DateTime ScheduledReviewAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(CardId))]
        public virtual Card Card { get; set; } = null!;
    }
}
