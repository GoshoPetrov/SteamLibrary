using Microsoft.EntityFrameworkCore;
using SteamLibrary.Data;
using SteamLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public static class ImportExport
    {
        public static string ExportToJson(ApplicationDbContext context)
        {
            var games = context.Games
                .Include(g => g.Publisher)
                .Include(g => g.AddedByUser)
                .Include(g => g.Users)
                    .ThenInclude(ug => ug.User)
                .AsNoTracking()
                .ToList();

            var publishers = context.Publishers
                .Include(p => p.CreatedByUser)
                .Include(p => p.Games)
                .AsNoTracking()
                .ToList();

            var exportData = new
            {
                ExportDate = DateTime.UtcNow,
                Games = games.Select(g => new
                {
                    g.Id,
                    g.Title,
                    g.Description,
                    g.Genre,
                    g.Price,
                    ReleaseDate = g.ReleaseDate.ToString("yyyy-MM-dd"),
                    g.AgeRating,
                    g.IsMultiplayer,
                    Publisher = g.Publisher != null ? new
                    {
                        g.Publisher.Id,
                        g.Publisher.Name,
                        g.Publisher.Location
                    } : null,
                    AddedBy = g.AddedByUser != null ? new
                    {
                        g.AddedByUser.Id,
                        g.AddedByUser.UserName,
                        g.AddedByUser.Email
                    } : null,
                    Users = g.Users.Select(ug => new
                    {
                        UserId = ug.UserId,
                        UserName = ug.User?.UserName,
                        AddedDate = ug.AddedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        ug.IsFavorite,
                        ug.PurchasePrice
                    }),
                    g.CreatedAt,
                    UpdatedAt = g.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                }),
                Publishers = publishers.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Location,
                    p.Email,
                    p.Phone,
                    FoundedDate = p.FoundedDate.ToString("yyyy-MM-dd"),
                    CreatedBy = p.CreatedByUser != null ? new
                    {
                        p.CreatedByUser.Id,
                        p.CreatedByUser.UserName,
                        p.CreatedByUser.Email
                    } : null,
                    GameCount = p.Games.Count,
                    GameTitles = p.Games.Select(g => g.Title),
                    p.CreatedAt,
                    UpdatedAt = p.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                })
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(exportData, options);
        }

        public static void ImportFromJson(ApplicationDbContext context, string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // -------------------------
            // Import Publishers
            // -------------------------
            if (root.TryGetProperty("publishers", out var publishersElement))
            {
                foreach (var p in publishersElement.EnumerateArray())
                {
                    var publisherId = p.GetProperty("id").GetInt32();

                    var publisher = context.Publishers
                        .Include(x => x.Games)
                        .FirstOrDefault(x => x.Id == publisherId);

                    if (publisher == null)
                    {
                        publisher = new Publisher
                        {
                            Id = publisherId
                        };
                        context.Publishers.Add(publisher);
                    }

                    publisher.Name = p.GetProperty("name").GetString();
                    publisher.Location = p.GetProperty("location").GetString();
                    publisher.Email = p.GetProperty("email").GetString();
                    publisher.Phone = p.GetProperty("phone").GetString();
                    publisher.FoundedDate = DateTime.Parse(p.GetProperty("foundedDate").GetString());
                    publisher.CreatedAt = p.GetProperty("createdAt").GetDateTime();

                    if (p.TryGetProperty("updatedAt", out var updatedAtProp) &&
                        updatedAtProp.ValueKind != JsonValueKind.Null)
                    {
                        publisher.UpdatedAt = DateTime.Parse(updatedAtProp.GetString());
                    }

                    // CreatedBy user
                    if (p.TryGetProperty("createdBy", out var createdByProp) &&
                        createdByProp.ValueKind != JsonValueKind.Null)
                    {
                        var userId = createdByProp.GetProperty("id").GetInt32();

                        var user = context.Users.FirstOrDefault(u => u.Id == userId);

                        publisher.CreatedByUser = user;
                    }
                }
            }

            context.SaveChanges();

            // -------------------------
            // Import Games
            // -------------------------
            if (root.TryGetProperty("games", out var gamesElement))
            {
                foreach (var g in gamesElement.EnumerateArray())
                {
                    var gameId = g.GetProperty("id").GetInt32();

                    var game = context.Games
                        .Include(x => x.Users)
                        .FirstOrDefault(x => x.Id == gameId);

                    if (game == null)
                    {
                        game = new Game
                        {
                            Id = gameId
                        };
                        context.Games.Add(game);
                    }

                    game.Title = g.GetProperty("title").GetString();
                    game.Description = g.GetProperty("description").GetString();
                    game.Genre = g.GetProperty("genre").GetString();
                    game.Price = g.GetProperty("price").GetDecimal();
                    game.ReleaseDate = DateTime.Parse(g.GetProperty("releaseDate").GetString());
                    game.AgeRating = g.GetProperty("ageRating").GetInt32();
                    game.IsMultiplayer = g.GetProperty("isMultiplayer").GetBoolean();
                    game.CreatedAt = g.GetProperty("createdAt").GetDateTime();

                    if (g.TryGetProperty("updatedAt", out var updatedAtProp) &&
                        updatedAtProp.ValueKind != JsonValueKind.Null)
                    {
                        game.UpdatedAt = DateTime.Parse(updatedAtProp.GetString());
                    }

                    // Publisher
                    if (g.TryGetProperty("publisher", out var publisherProp) &&
                        publisherProp.ValueKind != JsonValueKind.Null)
                    {
                        var publisherId = publisherProp.GetProperty("id").GetInt32();
                        var publisher = context.Publishers.FirstOrDefault(p => p.Id == publisherId);
                        game.Publisher = publisher;
                    }

                    // AddedBy user
                    if (g.TryGetProperty("addedBy", out var addedByProp) &&
                        addedByProp.ValueKind != JsonValueKind.Null)
                    {
                        var userId = addedByProp.GetProperty("id").GetInt32();

                        var user = context.Users.FirstOrDefault(u => u.Id == userId);
                        game.AddedByUser = user;
                    }

                    // Users (many-to-many)
                    if (g.TryGetProperty("users", out var usersProp))
                    {
                        game.Users.Clear();

                        foreach (var ug in usersProp.EnumerateArray())
                        {
                            var userId = ug.GetProperty("userId").GetInt32();

                            var user = context.Users.FirstOrDefault(u => u.Id == userId);

                            game.Users.Add(new UserGame
                            {
                                UserId = user.Id,
                                GameId = game.Id,
                                AddedDate = DateTime.Parse(ug.GetProperty("addedDate").GetString()),
                                IsFavorite = ug.GetProperty("isFavorite").GetBoolean(),
                                PurchasePrice = ug.GetProperty("purchasePrice").GetDecimal()
                            });
                        }
                    }
                }
            }

            context.SaveChanges();
        }
    }
}