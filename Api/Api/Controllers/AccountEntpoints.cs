using Api.Extentions;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    public static class AccountEntpoints
    {
        #region Map Account User Endpoints
        public static IEndpointRouteBuilder MapAccountEndPoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/UserProfile", GetUserProfile);
            app.MapGet("/GetAllUserProfile", GetAllUserProfiles);
            app.MapGet("/GetUserProfileById/{id}", GetUserProfileById);
            app.MapGet("/GetAllCustomer",GetAllCustomer);
            app.MapGet("/GetAllEmployeesAndAdmins", GetAllEmployeesAndAdmins);

            return app;
        }
        #endregion

        #region APIs
        [ProducesResponseType(typeof(AppUser),StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Employee")]
        private static async Task<IResult> GetAllEmployeesAndAdmins(UserManager<AppUser> userManager)
        {
            var users = await userManager.Users
                        .OrderBy(u => u.FirstName)
                        .ThenBy(u => u.LastName)
                        .ToListAsync();

            var userProfiles = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                if (roles.Contains("Admin")||roles.Contains("Employee"))
                {
                    userProfiles.Add(new
                    {
                        user.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.Gender,
                        user.PhoneNumber,
                        user.Address,
                        user.ZipCode,
                        user.City,
                        user.Birthday,
                        Role = roles.FirstOrDefault()
                    });
                }
            }

            return Results.Ok(userProfiles);
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Employee")]
        private static async Task<IResult> GetAllCustomer(UserManager<AppUser> userManager)
        {
            var users = await userManager.Users
                        .OrderBy(u => u.FirstName)
                        .ThenBy(u => u.LastName)
                        .ToListAsync();

            var userProfiles = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user); 

                if (roles.Contains("Customer"))
                {
                    userProfiles.Add(new
                    {
                        user.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.Gender,
                        user.PhoneNumber,
                        user.Address,
                        user.ZipCode,
                        user.City,
                        user.Birthday,
                        Role = roles.FirstOrDefault() 
                    });
                }
            }

            return Results.Ok(userProfiles);
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        private static async Task<IResult> GetUserProfileById(string id, UserManager<AppUser> userManager, ClaimsPrincipal user)
        {
            var userDetails = await userManager.FindByIdAsync(id);
            if (userDetails == null)
            {
                return Results.NotFound("User not found");
            }

            if (!user.IsSelfOrStaff(id))
            {
                // Kunden dürfen von Mitarbeitern nur den Namen sehen (z. B. auf der Rechnung),
                // aber keine Profile anderer Kunden.
                var targetRoles = await userManager.GetRolesAsync(userDetails);
                if (!targetRoles.Contains(Roles.Admin) && !targetRoles.Contains(Roles.Employee))
                {
                    return Results.Forbid();
                }
                return Results.Ok(new { userDetails.Id, userDetails.FirstName, userDetails.LastName });
            }

            return Results.Ok(new
            {
                userDetails.Id,
                userDetails.Email,
                userDetails.FirstName,
                userDetails.LastName,
                userDetails.Gender,
                userDetails.PhoneNumber,
                userDetails.Address,
                userDetails.ZipCode,
                userDetails.City,
                userDetails.Birthday
            });
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Employee")]
        private static async Task<IResult> GetAllUserProfiles(UserManager<AppUser> userManager)
        {
            var users = await userManager.Users
                        .OrderBy(u => u.FirstName)
                        .ThenBy(u => u.LastName)
                        .ToListAsync();

            var userProfiles = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user); 
                userProfiles.Add(new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Gender,
                    user.PhoneNumber,
                    user.Address,
                    user.ZipCode,
                    user.City,
                    user.Birthday,
                    Role = roles.FirstOrDefault() 
                });
            }

            return Results.Ok(userProfiles);
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        private static async Task<IResult> GetUserProfile(ClaimsPrincipal user, UserManager<AppUser> userManager)
        {
            string userID = user.GetUserId();
            var userDetails = await userManager.FindByIdAsync(userID);

            if (userDetails == null)
            {
                return Results.NotFound(new { message = "User not found." });
            }

            var role=  await userManager.GetRolesAsync(userDetails);
            

            return Results.Ok(new
            {
                Id=userDetails?.Id,
                Email = userDetails?.Email,
                FirstName = userDetails?.FirstName,
                LastName = userDetails?.LastName,
                Gender=userDetails?.Gender,
                PhoneNumber=userDetails?.PhoneNumber,
                Address=userDetails?.Address,
                ZipCode=userDetails?.ZipCode,
                City=userDetails?.City,
                Birthday=userDetails?.Birthday,
                Role = role.FirstOrDefault()
            });
            
        }

        #endregion
    }
}
