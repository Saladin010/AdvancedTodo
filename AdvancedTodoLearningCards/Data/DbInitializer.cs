using AdvancedTodoLearningCards.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdvancedTodoLearningCards.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRoles(roleManager);

            // Seed Users
            await SeedUsers(userManager);

            // Seed Global Algorithm Settings
            await SeedAlgorithmSettings(context);

            // Seed Sample Cards (for demo user)
            await SeedSampleCards(context, userManager);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            // Create Admin User
            if (await userManager.FindByEmailAsync("admin@learningcards.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@learningcards.com",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Create Demo User
            if (await userManager.FindByEmailAsync("demo@learningcards.com") == null)
            {
                var demoUser = new ApplicationUser
                {
                    UserName = "demo",
                    Email = "demo@learningcards.com",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(demoUser, "Demo@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoUser, "User");
                }
            }
        }

        private static async Task SeedAlgorithmSettings(ApplicationDbContext context)
        {
            if (!await context.AlgorithmSettings.AnyAsync(s => s.UserId == null))
            {
                var globalSettings = new AlgorithmSettings
                {
                    UserId = null,
                    InitialIntervals = "[1,3,7,15,30]",
                    InitialEaseFactor = 2.5m,
                    MinimumEaseFactor = 1.3m,
                    MaximumEaseFactor = 3.5m,
                    DefaultSchedulingMode = SchedulingMode.Fixed,
                    SwitchToAdaptiveAfterReps = 5,
                    UpdatedAt = DateTime.UtcNow
                };

                context.AlgorithmSettings.Add(globalSettings);
            }
        }

        private static async Task SeedSampleCards(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            var demoUser = await userManager.FindByEmailAsync("demo@learningcards.com");
            if (demoUser == null) return;

            if (!await context.Cards.AnyAsync(c => c.UserId == demoUser.Id))
            {
                var sampleCards = new List<Card>
                {
                    new Card
                    {
                        UserId = demoUser.Id,
                        Title = "What is Spaced Repetition?",
                        Content = "Spaced repetition is a learning technique that incorporates increasing intervals of time between subsequent review of previously learned material to exploit the psychological spacing effect.",
                        Tags = JsonSerializer.Serialize(new[] { "Learning", "Memory", "Psychology" }),
                        Difficulty = CardDifficulty.Easy,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        UserId = demoUser.Id,
                        Title = "SM-2 Algorithm",
                        Content = "SM-2 is a spaced repetition algorithm developed by Piotr Wozniak in 1988. It uses an easiness factor (EF) that is modified based on the quality of recall to determine optimal review intervals.",
                        Tags = JsonSerializer.Serialize(new[] { "Algorithm", "SM-2", "SuperMemo" }),
                        Difficulty = CardDifficulty.Medium,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        UserId = demoUser.Id,
                        Title = "Active Recall vs Passive Review",
                        Content = "Active recall (retrieval practice) is more effective than passive review (re-reading) because it strengthens memory retrieval pathways and provides feedback on what you actually know vs what you think you know.",
                        Tags = JsonSerializer.Serialize(new[] { "Study Technique", "Active Learning" }),
                        Difficulty = CardDifficulty.Easy,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        UserId = demoUser.Id,
                        Title = "Ebbinghaus Forgetting Curve",
                        Content = "The forgetting curve shows that information is lost over time when there is no attempt to retain it. Without reinforcement, we forget approximately 50% of new information within an hour and 90% within a week.",
                        Tags = JsonSerializer.Serialize(new[] { "Psychology", "Memory", "Research" }),
                        Difficulty = CardDifficulty.Medium,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Card
                    {
                        UserId = demoUser.Id,
                        Title = "What is an Easiness Factor (EF)?",
                        Content = "In SM-2 algorithm, EF is a number (1.3-3.5) representing how easy a card is to remember. Higher EF means longer intervals. It starts at 2.5 and is adjusted based on recall quality (0-5 rating).",
                        Tags = JsonSerializer.Serialize(new[] { "SM-2", "Algorithm", "Technical" }),
                        Difficulty = CardDifficulty.Hard,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.Cards.AddRange(sampleCards);
                await context.SaveChangesAsync();

                // Create schedules for sample cards
                foreach (var card in sampleCards)
                {
                    var schedule = new CardSchedule
                    {
                        CardId = card.Id,
                        RepetitionNumber = 0,
                        EaseFactor = 2.5m,
                        IntervalDays = 1,
                        LastReviewedAt = null,
                        NextReviewAt = DateTime.UtcNow.AddDays(1),
                        ReviewCount = 0,
                        LapseCount = 0,
                        SchedulingMode = SchedulingMode.Fixed
                    };

                    context.CardSchedules.Add(schedule);
                }
            }
        }
    }
}