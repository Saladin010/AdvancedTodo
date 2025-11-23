using Microsoft.AspNetCore.Identity;

namespace AdvancedTodoLearningCards.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
        public virtual ICollection<ReviewLog> ReviewLogs { get; set; } = new List<ReviewLog>();
        public virtual ICollection<AlgorithmSettings> AlgorithmSettings { get; set; } = new List<AlgorithmSettings>();
    }
}
