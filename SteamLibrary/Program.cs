using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SteamLibrary.Data;
using SteamLibrary.Entities;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SteamLibrary
{
    // dotnet ef migrations add InitialCreate
    // dotnet ef database update

    internal class Program
    {
        static void Main(string[] args)
        {
            var viewModel = new ScreenViewModel();
            viewModel.Show();

        }

        static async Task MainAlt(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options => {
                        options.UseSqlite("Data Source=C:\\Users\\john2\\source\\repos\\SteamLibrary\\SteamLibrary\\app.db");
                    });
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();
            DbSeeder.Seed(db);

            var viewModel = new ScreenViewModel();
            viewModel.Show();

        }


    }
}
