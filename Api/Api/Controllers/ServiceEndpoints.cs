using Api.Enums;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace Api.Controllers
{
    public static class ServiceEndpoints
    {
        public static IEndpointRouteBuilder MapServiceEndPoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/CreateService",CreateService);
            app.MapGet("/GetAllService",GetAllService);
            app.MapGet("/GetServiceById/{id}", GetServiceById);
            app.MapPut("/UpdateServiceById/{id}", UpdateServiceById);
            app.MapDelete("DeleteServiceById/{id}", DeleteService);
            return app;
        }

        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Service), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        private static async Task<IResult> DeleteService(int id, HttpContext httpContext, AppDbContext dbContext)
        {
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                return Results.Forbid(); // Verweigert den Zugriff, wenn die Rolle nicht passt
            }

            var service = await dbContext.Services.Where(x => x.ServiceId == id).FirstOrDefaultAsync();
            if (service == null)
            {
                return Results.NotFound(new { message = "Service not found." });
            }

            dbContext.Services.Remove(service);
            await dbContext.SaveChangesAsync();
            return Results.Ok(new {message = "Serive removed successfully"});
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(List<Service>), StatusCodes.Status200OK)]
        private static async Task<IResult> GetAllService(AppDbContext dbContext)
        {
            var allService = await dbContext.Services.Select(x => new Api.Models.Service
            {
                ServiceDetails = x.ServiceDetails,
                ServiceId = x.ServiceId,
                PricePerMinute = x.PricePerMinute,
                ServiceName = x.ServiceName,
            }).OrderBy(x => x.ServiceName).ToListAsync();

            return Results.Ok(allService);
        }


        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Service), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        private static async Task<IResult> UpdateServiceById(int id, HttpContext httpContext, AppDbContext dbContext, [FromBody] ServiceUpdateDto serviceUpdateDto)
        {
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                return Results.Forbid(); // Verweigert den Zugriff, wenn die Rolle nicht passt
            }

            var existingService = await dbContext.Services.Where(x => x.ServiceId == id).FirstOrDefaultAsync();
            if (existingService == null)
            {
                return Results.NotFound(new { message = "Service not found." });
            }
   
            existingService.ServiceName=serviceUpdateDto.ServiceName ?? existingService.ServiceName;
            existingService.ServiceDetails=serviceUpdateDto.ServiceDetails ?? existingService.ServiceDetails;
            existingService.PricePerMinute=serviceUpdateDto.PricePerMinute;

            dbContext.Services.Update(existingService);
            await dbContext.SaveChangesAsync();
            return Results.Ok(new { message = "Service update successfully." });
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(Service), StatusCodes.Status200OK)]
        private static async Task<IResult> GetServiceById(int id,AppDbContext dbContext)
        {
            var service = await dbContext.Services.Where(x => x.ServiceId == id).FirstOrDefaultAsync();
            if (service == null)
            {
                return Results.NotFound(new { message = "Service not found." });
            }
            return Results.Ok(service);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin")]
        private static async Task<IResult> CreateService(AppDbContext dbContext, HttpContext httpContext, [FromBody] ServiceDto serviceDto)
        {
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                return Results.Forbid(); // Verweigert den Zugriff, wenn die Rolle nicht passt
            }

            var newService = new Service()
            {
                ServiceName = serviceDto.ServiceName,
                ServiceDetails = serviceDto.ServiceDetails,
                PricePerMinute = serviceDto.PricePerMinute,
            };

            dbContext.Services.Add(newService);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Service created successfully." });
        }


        public record ServiceDto(
            string ServiceName,
            decimal PricePerMinute,
            string ServiceDetails);

        public record ServiceUpdateDto(
            string ServiceName,
            decimal PricePerMinute,
            string ServiceDetails);
    }
}
