using SteamLibrary.Data;
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

                    CreateNewUser(new UserDTO()
                    {
                        Username = username,
                        Password = password
                    });


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
                        Console.WriteLine("Wrong password");
                    }
                }
                catch(Exception e)
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
            //TODO
            throw new Exception("DB fail!");
        }
        private bool IsPasswordCorrect(UserDTO user)
        {
            //TODO
            return true;
        }
        private bool DoesUserExist(string username)
        {
            //TODO
            return false;
        }
    }
}
