using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamLibrary.Data;
using SteamLibrary.Entities;
using System.Threading.Tasks;

namespace SteamLibrary
{
    // dotnet ef migrations add InitialCreate
    // dotnet ef database update

    internal class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite("Data Source=C:\\Users\\john2\\source\\repos\\SteamLibrary\\SteamLibrary\\app.db"));
                })
                .Build();

            // Example usage
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();
        }

        //static void Main(string[] args)
        //{
        //    using Microsoft.EntityFrameworkCore;
        //    using YourApp.Infrastructure.Persistence;

        //    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //        options.UseSqlite("Data Source=app.db"));

        //    var context = new ApplicationDbContext()

        //    // Create User record
        //    var user = new User 
        //    { 
        //        UserName = "Jhon"
        //    };

        //    context.Users.Add(user);
        //    context.SaveChanges();


        //}

    }
}
