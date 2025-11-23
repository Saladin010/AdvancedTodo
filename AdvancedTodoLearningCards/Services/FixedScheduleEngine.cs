using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Services
{
    public class FixedScheduleEngine : ISchedulingEngine
    {
        private readonly int[] _fixedIntervals = { 1, 3, 7, 15, 30 };

        public string GetEngineName() => "Fixed Schedule";

        public CardSchedule InitializeSchedule(int cardId)
        {
            return new CardSchedule
            {
                CardId = cardId,
                RepetitionNumber = 0,
                EaseFactor = 2.5m,
                IntervalDays = _fixedIntervals[0],
                LastReviewedAt = null,
                NextReviewAt = DateTime.UtcNow.AddDays(_fixedIntervals[0]),
                ReviewCount = 0,
                LapseCount = 0,
                SchedulingMode = SchedulingMode.Fixed
            };
        }

        public CardSchedule CalculateNextReview(CardSchedule schedule, int quality)
        {
            // Validate quality (0-5)
            quality = Math.Clamp(quality, 0, 5);

            // Update review count
            schedule.ReviewCount++;
            schedule.LastReviewedAt = DateTime.UtcNow;

            // If quality is too low (0 or 1), reset to first interval (lapse)
            if (quality < 2)
            {
                schedule.LapseCount++;
                schedule.RepetitionNumber = 0;
                schedule.IntervalDays = _fixedIntervals[0];
                schedule.NextReviewAt = DateTime.UtcNow.AddDays(_fixedIntervals[0]);
                return schedule;
            }

            // Move to next repetition
            schedule.RepetitionNumber++;

            // Determine next interval
            if (schedule.RepetitionNumber < _fixedIntervals.Length)
            {
                // Still in fixed intervals
                schedule.IntervalDays = _fixedIntervals[schedule.RepetitionNumber];
            }
            else
            {
                // Completed fixed intervals - switch to adaptive mode
                schedule.SchedulingMode = SchedulingMode.Adaptive;

                // Continue with exponential growth based on last fixed interval
                var lastFixedInterval = _fixedIntervals[^1];
                schedule.IntervalDays = lastFixedInterval * 2;
            }

            schedule.NextReviewAt = DateTime.UtcNow.AddDays(schedule.IntervalDays);
            return schedule;
        }
    }
}