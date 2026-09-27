using Api;
using Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APITest.Infrastructure
{
    /// <summary>
    /// Startet die komplette API im Speicher (TestServer) mit einer
    /// In-Memory-Datenbank statt PostgreSQL. Jede Factory-Instanz hat ihre eigene DB.
    /// </summary>
    public class EaseTechApiFactory : WebApplicationFactory<Program>
    {
        // Nur für die In-Memory-Testdatenbank
        public const string DemoPassword = "Test!Demo2345";
        public const string AdminEmail = "admin@easetech.local";
        public const string AdminPassword = DemoPassword;
        public const string EmployeeEmail = "employee@easetech.local";
        public const string EmployeePassword = DemoPassword;
        public const string CustomerEmail = "customer@easetech.local";
        public const string CustomerPassword = DemoPassword;

        private readonly string _databaseName = "EaseTechTests-" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:JWT_Secret"] = "test-secret-test-secret-test-secret-123456",
                    ["ConnectionStrings:DefaultConnection"] = "Host=unused",
                    ["Database:MigrateOnStartup"] = "false",
                    ["Seed:DemoData"] = "true",
                    ["Seed:DemoPassword"] = DemoPassword,
                    ["GetStream:ApiKey"] = "test-api-key",
                    ["GetStream:SecretKey"] = "test-secret-key-test-secret-key-test-secret-key",
                });
            });

            builder.ConfigureServices(services =>
            {
                // PostgreSQL-Registrierung entfernen und durch In-Memory ersetzen
                var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }

        /// <summary>Führt Code mit einem frischen DbContext aus (für Arrange/Assert direkt in der DB).</summary>
        public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await action(db);
        }
    }
}
