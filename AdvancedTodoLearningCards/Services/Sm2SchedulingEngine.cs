using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Services
{
    public class Sm2SchedulingEngine : ISchedulingEngine
    {
        private const decimal MinEF = 1.3m;
        private const decimal MaxEF = 3.5m;
        private const decimal InitialEF = 2.5m;

        public string GetEngineName() => "SM-2 Adaptive";

        public CardSchedule InitializeSchedule(int cardId)
        {
            return new CardSchedule
            {
                CardId = cardId,
                RepetitionNumber = 0,
                EaseFactor = InitialEF,
                IntervalDays = 1,
                LastReviewedAt = null,
                NextReviewAt = DateTime.UtcNow.AddDays(1),
                ReviewCount = 0,
                LapseCount = 0,
                SchedulingMode = SchedulingMode.Adaptive
            };
        }

        public CardSchedule CalculateNextReview(CardSchedule schedule, int quality)
        {
            // Validate quality (0-5)
            quality = Math.Clamp(quality, 0, 5);

            // Update review metadata
            schedule.ReviewCount++;
            schedule.LastReviewedAt = DateTime.UtcNow;

            // Calculate new Ease Factor using SM-2 formula
            // EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
            var newEF = schedule.EaseFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m));
            schedule.EaseFactor = Math.Clamp(newEF, MinEF, MaxEF);

            // Handle different quality levels
            if (quality < 3)
            {
                // Failed recall (lapse) - reset to beginning
                schedule.LapseCount++;
                schedule.RepetitionNumber = 0;
                schedule.IntervalDays = 1;
            }
            else
            {
                // Successful recall - calculate next interval
                schedule.RepetitionNumber++;

                if (schedule.RepetitionNumber == 1)
                {
                    schedule.IntervalDays = 1;
                }
                else if (schedule.RepetitionNumber == 2)
                {
                    schedule.IntervalDays = 6;
                }
                else
                {
                    // I(n) = I(n-1) * EF
                    schedule.IntervalDays = (int)Math.Ceiling(schedule.IntervalDays * (double)schedule.EaseFactor);
                }
            }

            // Calculate next review date
            schedule.NextReviewAt = DateTime.UtcNow.AddDays(schedule.IntervalDays);

            return schedule;
        }
    }
}