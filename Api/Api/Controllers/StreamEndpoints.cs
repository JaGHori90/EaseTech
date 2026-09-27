using Api.Extentions;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StreamChat.Clients;
using Microsoft.EntityFrameworkCore;


namespace Api.Controllers
{
    public static class StreamEndpoints
    {

        public static IEndpointRouteBuilder MapStreamEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/CreateToken", GetTokenForStream);
            app.MapPost("/RegisterCall", RegisterCall);
            app.MapGet("/GetAllCalls", GetAllCalls);
            app.MapGet("/GetCallByUserId/{id}", GetByUserId);
            app.MapPut("/UpdateCallsStatus/{id}", UpdateStatus);
            app.MapGet("/IsEmpAvailable", IsEmpAvailable);
            return app;
        }
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        private static async Task<IResult> IsEmpAvailable(AppDbContext dbContext)
        {
            bool isVailible = await dbContext.CallStreams.Where(x => x.IsAvailable == true).AnyAsync(); ;
            return Results.Ok(isVailible);
        }

        [Authorize]
        [ProducesResponseType(typeof(CallStream), StatusCodes.Status200OK)]
        private static async Task<IResult> GetByUserId(string id, AppDbContext dbContext, ClaimsPrincipal user)
        {
            if (!user.IsSelfOrStaff(id))
            {
                return Results.Forbid();
            }

            CallStream stream = await dbContext.CallStreams.Where(x => x.UserId == id).FirstOrDefaultAsync();

            if (stream == null)
            {
                return Results.NotFound("Kein Call gefunden");
            }

            return Results.Ok(stream);
        }


        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(CallStream), StatusCodes.Status200OK)]
        private static async Task<IResult> UpdateStatus(int id, AppDbContext dbContext, ClaimsPrincipal user, [FromBody] UpdateCallStream updateCall)
        {
            CallStream stream = await dbContext.CallStreams.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (stream == null)
            {
                return Results.NotFound("Kein Call gefunden");
            }

            // Ein Mitarbeiter darf nur seinen eigenen Call-Status ändern, ein Admin jeden
            if (stream.UserId != user.GetUserId() && !user.IsAdmin())
            {
                return Results.Forbid();
            }

            stream.IsAvailable = updateCall.isAvailable;
            stream.LastUpdated = DateTime.UtcNow;
            stream.CallId = string.IsNullOrWhiteSpace(updateCall.CallId) ? stream.CallId : updateCall.CallId;

            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "CallStream update successfully." });
        }

        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(List<CallStream>), StatusCodes.Status200OK)]
        private static async Task<IResult> GetAllCalls(AppDbContext dbContext)
        {
            List<CallStream> stream = await dbContext.CallStreams.OrderBy(x => x.IsAvailable).ToListAsync();

            if (!stream.Any())
            {
                return Results.NotFound("Kein verfügbarer Call gefunden");
            }

            return Results.Ok(stream);
        }

        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(CallStream), StatusCodes.Status200OK)]
        private static async Task<IResult> RegisterCall(AppDbContext appDbContext, ClaimsPrincipal user, [FromBody] CreateCallStream stream)
        {
            // Der Call gehört immer dem angemeldeten Mitarbeiter – die UserId aus dem Body wird ignoriert,
            // damit niemand einen Call im Namen eines anderen registrieren kann.
            var userId = user.GetUserId();

            var callStream = await appDbContext.CallStreams.FirstOrDefaultAsync(x => x.UserId == userId);
            if (callStream == null)
            {
                callStream = new CallStream { UserId = userId };
                appDbContext.CallStreams.Add(callStream);
            }

            callStream.CallId = stream.CallId;
            callStream.IsAvailable = stream.isAvailable;
            callStream.LastUpdated = DateTime.UtcNow;

            await appDbContext.SaveChangesAsync();

            return Results.Ok(new
            {
                id = callStream.Id,
                message = "CallStream created successfully."
            });
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> GetTokenForStream(AppDbContext dbContext, IConfiguration config, ClaimsPrincipal userClaims)
        {
            string userId = userClaims.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var stream = await dbContext.CallStreams
                .Where(x => x.IsAvailable == true)
                .Select(x => new { x.CallId ,x.UserId})
                .FirstOrDefaultAsync();

            if (stream == null || string.IsNullOrEmpty(stream.CallId))
                return Results.NotFound("Kein verfügbarer Call gefunden oder CallId/UserId ungültig.");

            string apiKey = config["GetStream:ApiKey"];
            string secretKey = config["GetStream:SecretKey"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
                return Results.Problem("API-Schlüssel fehlen in der Konfiguration.");

            var factory = new StreamClientFactory(apiKey, secretKey);
            var userClient = factory.GetUserClient();

            // Token generieren
            var token = userClient.CreateToken(userId, DateTimeOffset.UtcNow.AddHours(8));

            return Results.Ok(new
            {
                token = token,
                apiKey = apiKey,
                callId = stream.CallId,
                userId = userId,   
                empId = stream.UserId,
            });
        }

        public record CreateCallStream(
            string UserId,
            string CallId,
            bool isAvailable
            );
        public record UpdateCallStream(
            string CallId,
            bool isAvailable
            );

    }
}
