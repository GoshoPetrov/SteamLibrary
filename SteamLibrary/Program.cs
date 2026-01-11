namespace SteamLibrary
{
    internal class Program
    {
        private static int currentScreen = 0;

        static void Main(string[] args)
        {
            var screen = new ScreenViewModel();

            screen.Show();
        }

    }
}
