using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Services
{
    public interface ISchedulingEngine
    {
        /// <summary>
        /// Calculate next review schedule based on quality rating
        /// </summary>
        /// <param name="schedule">Current card schedule</param>
        /// <param name="quality">Quality rating (0-5)</param>
        /// <returns>Updated schedule with new interval and ease factor</returns>
        CardSchedule CalculateNextReview(CardSchedule schedule, int quality);

        /// <summary>
        /// Initialize schedule for a new card
        /// </summary>
        /// <param name="cardId">Card ID</param>
        /// <returns>Initial schedule</returns>
        CardSchedule InitializeSchedule(int cardId);

        /// <summary>
        /// Get engine name
        /// </summary>
        string GetEngineName();
    }
}