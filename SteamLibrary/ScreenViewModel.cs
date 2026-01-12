using SteamLibrary.Data;
using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public enum ScreenType
    {
        Login,
        Register,
        Library,
        Unknown
    }

    public class ScreenViewModel
    {
        public ScreenType CurrentScreen { get; set; }

        private ApplicationDbContext _context { get; set; }

        public string Guest { get; set; }

        public ScreenViewModel(ApplicationDbContext context)
        {
            this._context = context;
        }

        public void Show()
        {
            CurrentScreen = ScreenType.Unknown;
            while (true)
            {
                switch (CurrentScreen)
                {
                    case ScreenType.Login:
                        LoginScreen();
                        break;

                    case ScreenType.Register:
                        RegisterScreen();
                        break;

                    case ScreenType.Library:
                        LibraryScreen();
                        break;

                    default:
                        StartScreen();
                        break;
                }
            }

        }

        private void RegisterScreen()
        {
            while (true)
            {
                Console.WriteLine("Enter your username: ");
                string username = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(username))
                {
                    CurrentScreen = ScreenType.Unknown;
                    return;
                }

                try
                {
                    if (DoesUserExist(username))
                    {
                        Console.WriteLine("Username already taken.");
                        continue;
                    }

                    Console.WriteLine("Enter your password: ");
                    string password = Console.ReadLine();

                    Console.WriteLine("Access: 1-Administrator, 2-User, 3-Guest");
                    string command = Console.ReadLine();

                    string access = null;
                    switch (command)
                    {
                        case "1":
                            access = "Administrator";
                            break;

                        case "2":
                            access = "User";
                            break;

                        case "3":
                            access = "Guest";
                            break;

                        default:
                            Console.WriteLine("Type 1, 2 or 3.");
                            continue;
                    }

                    CreateNewUser(new UserDTO()
                    {
                        Username = username,
                        Password = password,
                        Access = access
                    });

                    CurrentScreen = ScreenType.Library;
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error ocurred: {e.Message}");
                }

            }

        }

        private void LoginScreen()
        {
            while (true)
            {
                Console.WriteLine("Username: ");
                string username = Console.ReadLine();
                Console.WriteLine("Password: ");
                string password = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(username))
                {
                    CurrentScreen = ScreenType.Unknown;
                    return;
                }

                try
                {
                    if (IsPasswordCorrect(new UserDTO
                    {
                        Username = username,
                        Password = password
                    }))
                    {
                        CurrentScreen = ScreenType.Library;
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Wrong password ot username");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error message: {e.Message}");
                }
            }
        }

        private void LibraryScreen()
        {
            while (true)
            {
                Console.WriteLine("Welcome to your library");
                string game = Console.ReadLine();
            }

        }

        private void StartScreen()
        {
            while (true)
            {
                Console.WriteLine(
@"1. Login
2. Register
3. Continue as Guest.");

                string input = Console.ReadLine();

                if (input == "1")
                {
                    //todo
                    CurrentScreen = ScreenType.Login;
                    return;
                }
                else if (input == "2")
                {
                    CurrentScreen = ScreenType.Register;
                    return;
                    //todo
                }
                else if (input == "3")
                {
                    //todo
                }
                else
                {
                    Console.WriteLine("No Such Option");
                }
            }

        }

        //-------------------------------------------

        private void CreateNewUser(UserDTO user)
        {
            var access = _context.Accesses.FirstOrDefault(a => a.Name == user.Access);
            if (access == null)
            {
                access = new Data.Entities.Access()
                {
                    Name = user.Access
                };
                _context.Accesses.Add(access);
            }

            User user1 = new User()
            {
                UserName = user.Username,
                PasswordHash = user.Password,
                Email = $"{user.Username}@example.com",
                AccessId = access.Id,
            };

            _context.Users.Add(user1);

            _context.SaveChanges();

        }
        private bool IsPasswordCorrect(UserDTO user)
        {
            string username = user.Username;
            string password = user.Password;

            var result = _context.Users
                .Where(a => a.UserName == username && a.PasswordHash == password)
                .Select(a => new { a.UserName, a.PasswordHash })
                .ToList();


            return result.Count != 0;
        }
        private bool DoesUserExist(string username)
        {
            var result = _context.Users
                .Where(a => a.UserName == username)
                .Select(a => a.UserName)
                .ToList();

            return result.Count != 0;
        }
    }
}
