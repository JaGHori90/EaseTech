using Api.Models;

namespace Api.Extentions
{
    public static class AppConfigExtensions
    {
        public const string CorsPolicy = "Frontend";

        public static IServiceCollection AddAppConfig(this IServiceCollection services, IConfiguration config)
        {
            // ValidateOnStart: Die App startet gar nicht erst, wenn der JWT-Secret fehlt oder zu kurz ist.
            services.AddOptions<AppSettings>()
                .Bind(config.GetSection("AppSettings"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.JWT_Secret) && System.Text.Encoding.UTF8.GetByteCount(s.JWT_Secret) >= 32,
                    "AppSettings:JWT_Secret fehlt oder ist kürzer als 32 Byte. " +
                    "Setze ihn per User-Secrets oder Umgebungsvariable AppSettings__JWT_Secret.")
                .ValidateOnStart();

            var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
            services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

            return services;
        }

        public static WebApplication ConfigureCORS(this WebApplication app)
        {
            app.UseCors(CorsPolicy);
            return app;
        }
    }
}
