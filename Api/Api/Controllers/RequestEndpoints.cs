
using Api.Dtos;
using Api.Enums;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    public static class RequestEndpoints
    {
        #region Map UserRequest 
        public static IEndpointRouteBuilder MapUserRequestEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/CreateRequest", CreateRequest);
            app.MapGet("/GetAllRequest", GetAllRequest);
            app.MapGet("/GetRequestById/{id}", GetRequestById);
            app.MapPut("/UpdateRequestById/{id}", UpdateRequestById);

            return app;
        }
        #endregion

        #region APIs
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(UserRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        private static async Task<IResult> UpdateRequestById(int id, AppDbContext dbcontext, [FromBody] RequestUpdateDto requestUpdateDto)
        {
            var existingRequest = await dbcontext.UserRequests.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (existingRequest == null)
            {
                return Results.NotFound(new { message = "UserRequest not found." });
            }

            existingRequest.Referrer = requestUpdateDto.Referrer ?? existingRequest.Referrer;
            existingRequest.Name = requestUpdateDto.Name ?? existingRequest.Name;
            existingRequest.EmployeeId = string.IsNullOrWhiteSpace(requestUpdateDto.EmployeeId) ? existingRequest.EmployeeId : requestUpdateDto.EmployeeId;
            existingRequest.Message = requestUpdateDto.Message ?? existingRequest.Message;
            existingRequest.PhoneNumber = requestUpdateDto.PhoneNumber ?? existingRequest.PhoneNumber;
            if (!Enum.TryParse(requestUpdateDto.ContactStatus, true, out ContactStatus contactStatus))
            {
                return Results.BadRequest(new { message = "Invalid ContactStatus." });
            }
            existingRequest.ContactStatus = contactStatus;
            existingRequest.Date = requestUpdateDto.Date;

            await dbcontext.SaveChangesAsync();
            return Results.Ok(new { message = "UserRequest update successfully." });
        }

        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(UserRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        private static async Task<IResult> GetRequestById(int id, AppDbContext dbcontext)
        {
            var request = await dbcontext.UserRequests.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (request == null)
            {
                return Results.NotFound(new { message = "Request not found." });
            }
            return Results.Ok(request);
        }

        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(List<UserRequest>), StatusCodes.Status200OK)]
        private static async Task<IResult> GetAllRequest(AppDbContext dbcontext)
        {
            var allRequest = await dbcontext.UserRequests.Select(x => new UserRequestListDto()
            {
                Id = x.Id,
                Message = x.Message,
                Referrer = x.Referrer,
                Name = x.Name,
                PhoneNumber = x.PhoneNumber,
                ContactStatus = x.ContactStatus,
                EmployeeName = x.Employee.FirstName +" "+x.Employee.LastName,
                Date = x.Date,
            }).OrderBy(x => x.Date)
              .ThenBy(x => x.Name)
              .ToListAsync();

            return Results.Ok(allRequest);
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(UserRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        private static async Task<IResult> CreateRequest(AppDbContext dbContext, [FromBody] RequestDto RequestDto)
        {
            if (string.IsNullOrWhiteSpace(RequestDto.PhoneNumber))
            {
                return Results.BadRequest(new { message = "PhoneNumber is required." });
            }

            // Öffentliches Kontaktformular: Status, Datum und Mitarbeiter legt der Server fest
            var request = new UserRequest
            {
                Name = RequestDto.Name,
                Referrer = RequestDto.Referrer,
                PhoneNumber = RequestDto.PhoneNumber,
                Message = RequestDto.Message,
                Date = DateTime.UtcNow,
                EmployeeId = null,
                ContactStatus = ContactStatus.Unberührt
            };

            dbContext.UserRequests.Add(request);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "UserRequest created successfully." });

        }
        #endregion

        #region Record
        public record RequestDto
        (
             string Referrer,
             string Name,
             string Message,
             DateTime Date,
             string ContactStatus,
             string PhoneNumber,
             string EmployeeId
        );
        
        public record RequestUpdateDto
        (
             string Referrer,
             string Name ,
             string Message,
             DateTime Date ,
            string ContactStatus ,
            string PhoneNumber ,
             string EmployeeId 
        );
        #endregion
    }
}
