using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Extentions
{
    public static class EFCoreExtensions
    {
        public static IServiceCollection InjectDbContext(this IServiceCollection services, IConfiguration config)
        {
            // Neon (bzw. jedes PostgreSQL) – der Connection String kommt aus
            // User-Secrets / Umgebungsvariable ConnectionStrings__DefaultConnection.
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
            });

            return services;
        }
    }
}
