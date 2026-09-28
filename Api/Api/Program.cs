using Api.Controllers;
using Api.Data;
using Api.Extentions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(option =>
                option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.AddSwaggerExplorer()
            .InjectDbContext(builder.Configuration)
            .AddAppConfig(builder.Configuration)
            .AddIdentityHanlerAndStores()
            .ConfigureIdentityOptions()
            .AddIdentityAuth(builder.Configuration);

        var app = builder.Build();

        app.ConfigureSwaggerExplorer()
            .ConfigureCORS()
            .AddIdentityAuthMiddlewares();

        app.MapControllers();

        app.MapGroup("/api")
            .MapIdentityUserEndpoints()
            .MapAccountEndPoints()
            .MapServiceEndPoints()
            .MapOrderEndPoints()
            .MapUserRequestEndpoints()
            .MapInvoiceEndPoints()
            .MapStreamEndpoints();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

        await DbInitializer.InitializeAsync(app.Services, app.Configuration);

        await app.RunAsync();
    }
}
