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
    /// <summary>
    /// Represents the different screens that can be shown to the user
    /// in the console UI.
    /// </summary>
    public enum ScreenType
    {
        Login,
        Register,
        Library,
        Unknown
    }

    /// <summary>
    /// View model responsible for driving the console screens and
    /// handling user navigation and basic interactions.
    /// </summary>
    public class ScreenViewModel
    {
        /// <summary>
        /// The currently active screen.
        /// </summary>
        public ScreenType CurrentScreen { get; set; }

        private UserDTO? _currentUser { get; set; }

        /// <summary>
        /// Gets the name of the current user or "Anonymous" if no user is logged in.
        /// </summary>
        public string CurrentUserName
        {
            get
            {
                if (_currentUser == null) return "Anonymous";
                return _currentUser.Username;
            }
        }

        /// <summary>
        /// Returns true if the current user has Administrator access.
        /// </summary>
        public bool IsAdministrator
        {
            get
            {
                return _currentUser != null
                    && _currentUser.Access == "Administrator";
            }
        }

        /// <summary>
        /// Returns true if the current user has User access.
        /// </summary>
        public bool IsUser
        {
            get
            {
                return _currentUser != null
                    && _currentUser.Access == "User";
            }
        }

        /// <summary>
        /// Returns true when the current user is neither Administrator nor User.
        /// </summary>
        public bool IsGuest
        {
            get
            {
                return !IsAdministrator && !IsUser;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ScreenViewModel"/>.
        /// </summary>
        public ScreenViewModel()
        {
        }

        /// <summary>
        /// Starts the main loop that displays screens and handles navigation.
        /// This method blocks until the application is closed externally.
        /// </summary>
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

        /// <summary>
        /// Shows the registration screen where a new user can be created.
        /// </summary>
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
                    if (Logic.DoesUserExist(username))
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
                    
                    Logic.CreateNewUser(newUser);

                    _currentUser = Logic.IsPasswordCorrect(newUser);

                    CurrentScreen = ScreenType.Library;
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error ocurred: {e.Message}");
                }

            }

        }

        /// <summary>
        /// Shows the login screen and authenticates a user.
        /// </summary>
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
                    var user = Logic.IsPasswordCorrect(new UserDTO
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

        /// <summary>
        /// Shows the library screen, greets the user and dispatches to the
        /// appropriate management screen based on access level.
        /// </summary>
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

        /// <summary>
        /// Presents options for administrators to manage users.
        /// </summary>
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
                        ListAllUsers(null);
                        break;

                    case "2":
                        DeleteUserScreen();
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Prompts for a username and deletes the matching user if found.
        /// </summary>
        private void DeleteUserScreen()
        {
            while (true)
            {
                Console.WriteLine("Type the name of the user to delete:");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                var users = Logic.LoadAllUsers(input);

                if (users.Count == 0)
                {
                    Console.WriteLine($"Count not find user \"{input}\"");
                    continue;
                }

                var exactMatch = users.FirstOrDefault(a => a.Username == input);
                if (users.Count == 1)
                {
                    exactMatch = users[0];
                }

                if (exactMatch != null)
                {
                    Logic.DeleteUser(exactMatch.Id);
                    Console.WriteLine($"User \"{exactMatch.Username}\" was deleted.");
                    continue;
                }

                ShowUsersList(users, input);
                Console.WriteLine("Type the name more precisely.");

            }
        }

        /// <summary>
        /// Presents game management options for regular users.
        /// </summary>
        private void ManageGamesScreen()
        {
            while (true)
            {
                Console.WriteLine(@"
1. View games catalog
2. Add game
3. Delete game
4. Export to JSON
5. Import from JSON
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

                    case "3":
                        //TODO:
                        break;

                    case "4":
                        ExportToJsonScreen();
                        break;

                    case "5":
                        ImportFromJsonScreen();
                        break;


                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Shows browsing options for the games catalog. The optional
        /// returnTo value controls which screen to go back to.
        /// </summary>
        /// <param name="returnTo">Screen to return to when exiting browse mode.</param>
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
                        ShowGameList();
                        break;
                    case "2":
                        Console.WriteLine("Enter game name here: ");
                        string game = Console.ReadLine();
                        SearchGame(game);
                        break;
                    default:
                        break;
                }
            }
        }



        /// <summary>
        /// Displays the start screen with options to login, register or continue as guest.
        /// </summary>
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
                    _currentUser = null;
                    CurrentScreen = ScreenType.Library;
                    return;
                }
                else
                {
                    Console.WriteLine("No Such Option");
                }
            }

        }

        /// <summary>
        /// Exports the current database to a JSON file with the provided name.
        /// </summary>
        private void ExportToJsonScreen()
        {
            while (true)
            {
                Console.WriteLine("Type the name of the json file:");

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                try
                {
                    var jsonStr = ImportExport.ExportToJson(Logic.GetContext());
                    File.WriteAllText(input, jsonStr);

                    Console.WriteLine("Done!");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Imports database content from a JSON file and shows statistics after import.
        /// </summary>
        private void ImportFromJsonScreen()
        {
            while (true)
            {
                Console.WriteLine("Type the name of the json file to import:");

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                try
                {
                    var jsonStr = File.ReadAllText(input);
                    ImportExport.ImportFromJson(Logic.GetContext(), jsonStr);


                    Console.WriteLine("Done!");

                    ShowStatistic();

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");

                }
            }
        }

        //-------------------------------------------

        /// <summary>
        /// Prints all games currently available.
        /// </summary>
        private void ShowGameList()
        {
            var games = Logic.LoadAllGames();

            foreach (var game in games)
            {
                Console.WriteLine($"{game.Name, -30}, {game.Publisher}");
            }

        }

        /// <summary>
        /// Searches for games by an optional filter and displays the matches.
        /// </summary>
        /// <param name="filter">Optional search text to filter games by name.</param>
        private void SearchGame(string? filter)
        {
            var games = Logic.LoadAllGames(filter);

            Console.WriteLine($"{games.Count} match filter {filter}");

            foreach (var game in games)
            {
                Console.WriteLine($"{game.Name,-30}, {game.Publisher}");
            }
        }

        /// <summary>
        /// Shows basic statistics about the database contents.
        /// </summary>
        private void ShowStatistic()
        {
            Console.WriteLine("Database has:");
            var stat = Logic.CountRecords();
            foreach (var item in stat)
            {
                Console.WriteLine($"{item.Key,-10}: {item.Value,7} records");
            }
        }

        /// <summary>
        /// Loads and lists all users optionally filtered by the provided string.
        /// </summary>
        private void ListAllUsers(string filter = null)
        {
            var users = Logic.LoadAllUsers(filter);
            ShowUsersList(users, filter);
        }

        /// <summary>
        /// Displays a list of users and indicates how many matched the filter.
        /// </summary>
        private void ShowUsersList(List<UserDTO> users, string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                Console.WriteLine($"{users.Count} users match \"{filter}\"");
            }
            else
            {
                Console.WriteLine($"{users.Count} users");
            }

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Username,-20}, {user.Access,-15}");
            }
        }
    }
}
