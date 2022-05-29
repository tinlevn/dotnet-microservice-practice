using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using( var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        private static void SeedData(AppDbContext context, bool isProd)
        {
            if(isProd)
            {
                Console.WriteLine("--> Attempting to apply migration");
                try {
                    //restore and install entityframework core in order for migrate to work
                    context.Database.Migrate();
                } catch (Exception error) {
                    Console.WriteLine($"--> Could not run migrations: {error.Message}");
                }
                
            }

            
            if(!context.Platforms.Any())
            {
                Console.WriteLine("Seeding data...");

                context.Platforms.AddRange(
                    new Platform() {Name="DotNet", Publisher="Microsoft", Cost= "Free"},
                    new Platform() {Name="SSMS", Publisher="Microsoft", Cost= "Free"},
                    new Platform() {Name="Kubernetes", Publisher="Cloud Native Computing Foundation", Cost= "Free"},
                    new Platform() {Name="Laraval", Publisher="PHP Foundationt", Cost= "Free"}
                );

                context.SaveChanges();
            }
            else 
            {
                Console.WriteLine("Data existed already");
            }
        }
    }
}