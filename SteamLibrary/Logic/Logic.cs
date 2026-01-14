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
        public static void CreateNewUser(ApplicationDbContext context, UserDTO user)
        {
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

        public static List<UserDTO> LoadAllUsers(ApplicationDbContext context, string filter)
        {
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

        public static UserDTO? IsPasswordCorrect(ApplicationDbContext context, UserDTO user)
        {
            string username = user.Username;
            string password = user.Password;

            string hash = PasswordHash(password);

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

        public static bool DoesUserExist(ApplicationDbContext context, string username)
        {
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

        internal static void DeleteUser(ApplicationDbContext context, Guid id)
        {
            var user = context.Users.FirstOrDefault(a => a.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
                context.SaveChanges();
            }
        }
    }
}
