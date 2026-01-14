using SteamLibrary.Data;
using SteamLibrary.Data.Entities;
using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public static class DbSeeder
    {
        public static void SeedAccesses(ApplicationDbContext context)
        {
            if (context.Accesses.Any())
            {
                return;
            }

            var accesses = new[]
            {
            new Access
            {
                Id = 1,
                Name = "Administrator"
            },
            new Access
            {
                Id = 2,
                Name = "User"
            },
            new Access
            {
                Id = 3,
                Name = "Guest"
            }
        };

            context.Accesses.AddRange(accesses);
            context.SaveChanges();
        }
        public static void Seed(ApplicationDbContext context)
        {
            SeedAccesses(context);

            // Prevent duplicate seeding
            if (context.Users.Any() ||
                context.Publishers.Any() ||
                context.Games.Any())
            {
                return;
            }



            // -------------------------
            // Users
            // -------------------------
            var users = Enumerable.Range(1, 10)
                .Select(i => new User
                {
                    Id = i,
                    UserName = $"user{i}",
                    Email = $"user{i}@example.com",
                    PasswordHash = $"HASHED_PASSWORD_{i}",
                    AccessId = context.Accesses.First().Id, // assumes at least one Access exists
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                })
                .ToList();

            context.Users.AddRange(users);
            context.SaveChanges();

            // -------------------------
            // Publishers
            // -------------------------
            var publishers = Enumerable.Range(1, 10)
                .Select(i => new Publisher
                {
                    Id = i,
                    Name = $"Publisher {i}",
                    Location = $"City {i}",
                    Email = $"publisher{i}@example.com",
                    Phone = $"+1-555-000{i:D2}",
                    FoundedDate = DateTime.UtcNow.AddYears(-10 - i),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = users[i % users.Count].Id
                })
                .ToList();

            context.Publishers.AddRange(publishers);
            context.SaveChanges();

            // -------------------------
            // Games
            // -------------------------
            var games = Enumerable.Range(1, 20)
                .Select(i =>
                {
                    var publisher = publishers[i % publishers.Count];
                    var addedByUser = users[i % users.Count];

                    return new Game
                    {
                        Id = i,
                        Title = $"Game {i}",
                        Description = $"Description for Game {i}",
                        Genre = i % 2 == 0 ? "Action" : "RPG",
                        Price = 19.99m + i,
                        ReleaseDate = DateTime.UtcNow.AddDays(-i * 30),
                        AgeRating = i % 3 == 0 ? 18 : 12,
                        IsMultiplayer = i % 2 == 0,
                        PublisherId = publisher.Id,
                        AddedByUserId = addedByUser.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                })
                .ToList();

            context.Games.AddRange(games);
            context.SaveChanges();

            // -------------------------
            // UserGame (many-to-many)
            // -------------------------
            var userGames = new List<UserGame>();

            foreach (var game in games)
            {
                // Each game is owned by 2–3 users
                var owners = users
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(3);

                foreach (var user in owners)
                {
                    userGames.Add(new UserGame
                    {
                        UserId = user.Id,
                        GameId = game.Id,
                        AddedDate = DateTime.UtcNow.AddDays(-5),
                        IsFavorite = Random.Shared.Next(0, 2) == 1,
                        PurchasePrice = game.Price
                    });
                }
            }

            context.Set<UserGame>().AddRange(userGames);
            context.SaveChanges();
        }
    }
}
