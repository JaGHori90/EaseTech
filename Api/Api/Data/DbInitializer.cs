using Api.Extentions;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    /// <summary>
    /// Bereitet die Datenbank beim Start vor:
    /// 1. Migrationen anwenden (optional, Database:MigrateOnStartup)
    /// 2. Rollen Admin/Employee/Customer anlegen (früher mussten sie händisch erstellt werden)
    /// 3. Ersten Admin aus der Konfiguration anlegen (Seed:AdminEmail / Seed:AdminPassword)
    /// 4. Optional Demo-Daten für die Entwicklung (Seed:DemoData, Passwort aus Seed:DemoPassword)
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));
            var db = provider.GetRequiredService<AppDbContext>();

            if (config.GetValue<bool>("Database:MigrateOnStartup") && db.Database.IsRelational())
            {
                logger.LogInformation("Wende Datenbank-Migrationen an ...");
                await db.Database.MigrateAsync();
            }

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var userManager = provider.GetRequiredService<UserManager<AppUser>>();

            var adminEmail = config["Seed:AdminEmail"];
            var adminPassword = config["Seed:AdminPassword"];
            if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            {
                await EnsureUserAsync(userManager, logger, adminEmail, adminPassword, "Admin", "EaseTech", Roles.Admin);
            }

            if (config.GetValue<bool>("Seed:DemoData"))
            {
                await SeedDemoDataAsync(db, userManager, logger, config["Seed:DemoPassword"]);
            }
        }

        private static async Task<AppUser> EnsureUserAsync(UserManager<AppUser> userManager, ILogger logger,
            string email, string password, string firstName, string lastName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return user;
            }

            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Seed-Benutzer {Email} konnte nicht angelegt werden: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("Seed-Benutzer {Email} ({Role}) angelegt.", email, role);
            return user;
        }

        private static async Task SeedDemoDataAsync(AppDbContext db, UserManager<AppUser> userManager, ILogger logger, string demoPassword)
        {
            // Demo-Zugänge – NUR für die lokale Entwicklung gedacht.
            // Das Passwort steht bewusst nicht im Code, sondern in den User-Secrets (Seed:DemoPassword).
            if (string.IsNullOrWhiteSpace(demoPassword))
            {
                logger.LogWarning("Seed:DemoData ist aktiv, aber Seed:DemoPassword fehlt – Demo-Benutzer werden nicht angelegt.");
            }
            else
            {
                await EnsureUserAsync(userManager, logger, "admin@easetech.local", demoPassword, "Max", "Bauer", Roles.Admin);
                await EnsureUserAsync(userManager, logger, "employee@easetech.local", demoPassword, "Andrea", "Leitner", Roles.Employee);
                await EnsureUserAsync(userManager, logger, "customer@easetech.local", demoPassword, "Anna", "Schmidt", Roles.Customer);
            }

            if (!await db.Services.AnyAsync())
            {
                db.Services.AddRange(
                    new Service { ServiceName = "PC-Hilfe", PricePerMinute = 1.50m, ServiceDetails = "Hilfe bei Windows, macOS und Software-Problemen." },
                    new Service { ServiceName = "Smartphone-Hilfe", PricePerMinute = 1.20m, ServiceDetails = "Einrichtung und Hilfe für Android und iPhone." },
                    new Service { ServiceName = "Netzwerk & WLAN", PricePerMinute = 2.00m, ServiceDetails = "Router, WLAN und Heimnetzwerk einrichten." });
                await db.SaveChangesAsync();
            }
        }
    }
}
