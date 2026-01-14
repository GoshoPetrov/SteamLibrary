using Microsoft.EntityFrameworkCore;
using SteamLibrary.Data;
using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary
{
    /// <summary>
    /// Central application logic helper that provides data access helpers and simple operations
    /// used throughout the application (loading users/games, creating users, counting records, etc.).
    /// </summary>
    public static class Logic
    {
        /// <summary>
        /// Create and return a new <see cref="ApplicationDbContext"/> configured to use the
        /// local SQLite file. Ensures database migrations are applied and initial seed data is present.
        /// </summary>
        public static ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(@"Data Source=app.db")
                .Options;
            var db = new ApplicationDbContext(options);

            // Apply any pending migrations and ensure the database is up-to-date
            db.Database.Migrate();

            // Seed initial data if necessary (users, publishers, games, etc.)
            DbSeeder.Seed(db);

            return db;
        }

        /// <summary>
        /// Count records in the main tables and return a dictionary with the counts.
        /// </summary>
        public static Dictionary<string, int> CountRecords()
        {
            var context = GetContext();

            var result = new Dictionary<string, int>();

            // Count records for commonly displayed entities
            result.Add("Users", context.Users.Count());
            result.Add("Games", context.Games.Count());
            result.Add("Publishers", context.Publishers.Count());
            result.Add("UserGame", context.UserGame.Count());

            return result;
        }

        /// <summary>
        /// Create a new user from a <see cref="UserDTO"/>. If the requested access role does not exist,
        /// it will be created.
        /// </summary>
        /// <param name="user">Data transfer object with user information (username, password, access).</param>
        public static void CreateNewUser(UserDTO user)
        {
            var context = GetContext();

            // Ensure the access/role exists, create it if missing
            var access = context.Accesses.FirstOrDefault(a => a.Name == user.Access);
            if (access == null)
            {
                access = new Data.Entities.Access()
                {
                    Name = user.Access
                };
                context.Accesses.Add(access);
            }

            // Map DTO to entity. Password is hashed via PasswordHash (note: not secure in this sample).
            User user1 = new User()
            {
                UserName = user.Username,
                PasswordHash = PasswordHash(user.Password),
                Email = $"{user.Username}@example.com",
                AccessId = access.Id,
            };

            context.Users.Add(user1);

            // Persist changes to the database
            context.SaveChanges();
        }

        /// <summary>
        /// Load users from the database, optionally filtering by username.
        /// Returns a list of <see cref="UserDTO"/> values.
        /// </summary>
        /// <param name="filter">Optional substring to filter usernames (case-insensitive).</param>
        public static List<UserDTO> LoadAllUsers(string filter)
        {
            var context = GetContext();

            var users = context.Users
                .Include(u => u.Access)
                .Where(u => string.IsNullOrEmpty(filter)
                    || u.UserName.ToLower().Contains(filter.ToLower()))
                .ToList();

            var result = new List<UserDTO>();
            foreach (var user in users)
            {
                result.Add(new UserDTO()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Access = user.Access.Name
                });
            }

            return result;
        }

        /// <summary>
        /// Verify whether the provided username and password are correct. Returns a <see cref="UserDTO"/>
        /// for the logged in user on success, or null when authentication fails.
        /// </summary>
        /// <param name="user">DTO containing username and plain-text password to verify.</param>
        public static UserDTO? IsPasswordCorrect(UserDTO user)
        {
            string username = user.Username;
            string password = user.Password;

            // Compute hash for the supplied password
            string hash = PasswordHash(password);

            var context = GetContext();

            // Look up a user matching the username + password hash
            var result = context.Users
                .Include(u => u.Access)
                .Where(a => a.UserName == username && a.PasswordHash == hash)
                .ToList();

            if (result.Count == 0)
            {
                return null;
            }

            var loggedUser = result[0];
            return new UserDTO()
            {
                Id = loggedUser.Id,
                Username = loggedUser.UserName,
                Access = loggedUser.Access.Name
            };
        }

        /// <summary>
        /// Check whether a user with the given username exists in the database.
        /// </summary>
        /// <param name="username">Username to check for existence.</param>
        /// <returns>True if user exists, otherwise false.</returns>
        public static bool DoesUserExist(string username)
        {
            var context = GetContext();

            var result = context.Users
                .Where(a => a.UserName == username)
                .Select(a => a.UserName)
                .ToList();

            return result.Count != 0;
        }

        /// <summary>
        /// Compute a password hash representation for storage/verification.
        /// IMPORTANT: In this sample application the method currently returns the plain password
        /// (no hashing). This is NOT safe and only present for simplicity. Replace with a
        /// secure hashing algorithm (Argon2, bcrypt, PBKDF2, scrypt) for production use.
        /// </summary>
        /// <param name="password">Plain-text password.</param>
        /// <returns>Hash string to store/compare.</returns>
        public static string PasswordHash(string password)
        {
            //TODO: Compute password hash
            // SECURITY WARNING: This is not a secure password hashing method.
            // In a real application, you should use a secure password hashing algorithm like Argon2 or scrypt.
            return password;
        }

        /// <summary>
        /// Delete a user by id if they exist. Internal - intended for use by tests or admin tools.
        /// </summary>
        internal static void DeleteUser(int id)
        {
            var context = GetContext();

            var user = context.Users
                .FirstOrDefault(a => a.Id == id);

            if (user != null)
            {
                context.Users.Remove(user);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Load games from the database, optionally filtering by title. Returns a list of
        /// <see cref="GameDTO"/> values ordered by title.
        /// </summary>
        /// <param name="filter">Optional substring to filter game titles (case-insensitive).</param>
        public static List<GameDTO> LoadAllGames(string filter = null)
        {
            var context = GetContext();

            var games = context.Games
                .Include(g => g.Publisher)
                .Where(g => string.IsNullOrEmpty(filter)
                    || g.Title.ToLower().Contains(filter.ToLower()))
                .OrderBy(g => g.Title)
                .ToList();

            var result = new List<GameDTO>();
            foreach (var game in games)
            {
                result.Add(new GameDTO()
                {
                    Name = game.Title,
                    Publisher = game.Publisher.Name
                });
            }

            return result;
        }
    }
}
