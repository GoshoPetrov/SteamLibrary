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
    public static class Logic
    {
        public static ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(@"Data Source=app.db")
                .Options;
            var db = new ApplicationDbContext(options);

            db.Database.Migrate();
            DbSeeder.Seed(db);

            return db;
        }

        public static Dictionary<string, int> CountRecords()
        {
            var context = GetContext();

            var result = new Dictionary<string, int>();

            result.Add("Users", context.Users.Count());
            result.Add("Games", context.Games.Count());
            result.Add("Publishers", context.Publishers.Count());
            result.Add("UserGame", context.UserGame.Count());

            return result;
        }

        public static void CreateNewUser(UserDTO user)
        {
            var context = GetContext();

            var access = context.Accesses.FirstOrDefault(a => a.Name == user.Access);
            if (access == null)
            {
                access = new Data.Entities.Access()
                {
                    Name = user.Access
                };
                context.Accesses.Add(access);
            }

            User user1 = new User()
            {
                UserName = user.Username,
                PasswordHash = PasswordHash(user.Password),
                Email = $"{user.Username}@example.com",
                AccessId = access.Id,
            };

            context.Users.Add(user1);

            context.SaveChanges();
        }

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

        public static UserDTO? IsPasswordCorrect(UserDTO user)
        {
            string username = user.Username;
            string password = user.Password;

            string hash = PasswordHash(password);

            var context = GetContext();

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

        public static bool DoesUserExist(string username)
        {
            var context = GetContext();

            var result = context.Users
                .Where(a => a.UserName == username)
                .Select(a => a.UserName)
                .ToList();

            return result.Count != 0;
        }

        public static string PasswordHash(string password)
        {
            //TODO: Compute password hash
            return password;
        }

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
