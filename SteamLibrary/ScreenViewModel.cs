using Microsoft.EntityFrameworkCore;
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

        private UserDTO? _currentUser { get; set; }

        public string CurrentUserName
        {
            get
            {
                if (_currentUser == null) return "Anonymous";
                return _currentUser.Username;
            }
        }

        public bool IsAdministrator
        {
            get
            {
                return _currentUser != null
                    && _currentUser.Access == "Administrator";
            }
        }

        public bool IsUser
        {
            get
            {
                return _currentUser != null
                    && _currentUser.Access == "User";
            }
        }

        public bool IsGuest
        {
            get
            {
                return !IsAdministrator && !IsUser;
            }
        }

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

                    var newUser = new UserDTO()
                    {
                        Username = username,
                        Password = password,
                        Access = access
                    };
                    
                    CreateNewUser(newUser);

                    _currentUser = IsPasswordCorrect(newUser);

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
                    var user = IsPasswordCorrect(new UserDTO
                    {
                        Username = username,
                        Password = password
                    });

                    if (user != null)
                    {
                        _currentUser = user;
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
            if (!string.IsNullOrWhiteSpace(CurrentUserName))
            {
                Console.WriteLine($"Welcome, {CurrentUserName}!");
            }
            else
            {
                Console.WriteLine($"Welcome!");
            }

            string access = "Guest";
            if (IsAdministrator) access = "Administrator";
            if (IsUser) access = "User";

            Console.WriteLine($"Your access level is: {access}");
            Console.WriteLine($"---------------------");

            if (IsAdministrator)
            {
                ManageUsersScreen();
                return;
            }

            if (IsUser)
            {
                ManageGamesScreen();
                return;
            }

            // Guests are allowed to see the games catalog only
            BrowseGamesScreen(ScreenType.Unknown);
        }

        private void ManageUsersScreen()
        {
            while (true)
            {
                Console.WriteLine(@"
1. List all users
2. Delete user
");

                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    CurrentScreen = ScreenType.Library;
                    return;
                }

                switch (input)
                {
                    case "1":
                        //TODO:
                        break;

                    case "2":
                        //TODO:
                        break;

                    default:
                        break;
                }
            }
        }

        private void ManageGamesScreen()
        {
            while (true)
            {
                Console.WriteLine(@"
1. View games catalog
2. Add game
3. Delete game
");

                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    CurrentScreen = ScreenType.Library;
                    return;
                }

                switch (input)
                {
                    case "1":
                        BrowseGamesScreen(ScreenType.Library);
                        break;

                    case "2":
                        //TODO:
                        break;

                    default:
                        break;
                }
            }
        }

        private void BrowseGamesScreen(ScreenType returnTo = ScreenType.Library)
        {
            while (true)
            {
                Console.WriteLine(@"
1. List the available games
2. Search by title
");

                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    CurrentScreen = returnTo;
                    return;
                }

                switch (input)
                {
                    case "1":
                        //TODO:
                        break;
                    case "2":
                        //TODO:
                        break;
                    default:
                        break;
                }
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
                    CurrentScreen = ScreenType.Login;
                    return;
                }
                else if (input == "2")
                {
                    CurrentScreen = ScreenType.Register;
                    return;
                }
                else if (input == "3")
                {
                    CurrentScreen = ScreenType.Library;
                    return;
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
                PasswordHash = PasswordHash(user.Password),
                Email = $"{user.Username}@example.com",
                AccessId = access.Id,
            };

            _context.Users.Add(user1);

            _context.SaveChanges();
        }

        private UserDTO? IsPasswordCorrect(UserDTO user)
        {
            string username = user.Username;
            string password = user.Password;

            string hash = PasswordHash(password);

            var result = _context.Users
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
        private bool DoesUserExist(string username)
        {
            var result = _context.Users
                .Where(a => a.UserName == username)
                .Select(a => a.UserName)
                .ToList();

            return result.Count != 0;
        }

        private string PasswordHash(string password)
        {
            //TODO: Compute password hash
            return password;
        }
    }
}
