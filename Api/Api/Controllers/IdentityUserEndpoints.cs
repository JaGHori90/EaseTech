using Api.Enums;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Extentions;

namespace Api.Controllers
{
    public static class IdentityUserEndpoints
    {
        #region Map identity User Entpoints
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/signup", CreateUser);
            app.MapPost("/signin", SignIn);
            app.MapPost("/ResetPassword", ResetPassword);
            app.MapPut("/ChangePassword", ChangePassword);
            app.MapPost("/deleteUser", DeleteUser);
            app.MapPut("/updateUserById/{id}", UpdateUserById);
            return app;
        }
        #endregion

        #region APIs
        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Customer,Admin")]
        private static async Task<IResult> DeleteUser([FromBody] DeleteUserRequest request, AppDbContext dbContext,
            UserManager<AppUser> userManager, ClaimsPrincipal userClaims,HttpContext httpContext)
        {
            string userId = userClaims.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound(new { message = "User not found." });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && userRole != "Customer")
            {
                return Results.Forbid(); // Verweigert den Zugriff, wenn die Rolle nicht passt
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return Results.BadRequest(new { message = "Incorrect password." });
            }


            var hasOpenOrders = await dbContext.Orders
                .Where(order => order.CustomerId == userId &&
                    (order.OrderStatus != OrderStatus.Abgeschlossen && order.OrderStatus != OrderStatus.Storniert))
                .AnyAsync();

            if (hasOpenOrders)
            {
                return Results.BadRequest(new { message = "You have pending orders." });
            }

            var hasOpenInvoice = await dbContext.Invoices
                .Where(invoice => invoice.CustomerId == userId &&
                    (invoice.PaymentStatus != PaymentStatus.Erfolgreich && invoice.PaymentStatus != PaymentStatus.Erstattet))
                .AnyAsync();

            if (hasOpenInvoice)
            {
                return Results.BadRequest(new { message = "You have unpaid invoices." });
            }

            await userManager.DeleteAsync(user);

            return Results.Ok(new { message = "User deleted successfully." });
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        private static async Task<IResult> ResetPassword(UserManager<AppUser> userManager, [FromBody] ResetPasswordModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Results.NotFound(new { message = "User not found." });
            }

            // Generiere ein Reset-Token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            // Setze das Passwort zurück
            var result = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            if (result.Succeeded)
            {
                return Results.Ok(new { message = "Password has been reset successfully." });
            }
            return Results.BadRequest(result.Errors);
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        private static async Task<IResult> ChangePassword(UserManager<AppUser> userManager, [FromBody] ChangePasswordModel model, ClaimsPrincipal user)
        {
          
            string userID = user.Claims.First(x => x.Type == "userID").Value;
            var userDetails = await userManager.FindByIdAsync(userID);

            if (userDetails == null)
            {
                return Results.NotFound(new { message = "User not found." });
            }

            var result = await userManager.ChangePasswordAsync(userDetails, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Results.Ok(new { message = "Password changed successfully." });
            }
            return Results.BadRequest(result.Errors);
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        private static async Task<IResult> UpdateUserById(string id, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ClaimsPrincipal user, [FromBody] UpdateUserModel updateUserModel)
        {
            // Nur der Benutzer selbst oder Admin/Mitarbeiter dürfen ein Profil ändern
            if (!user.IsSelfOrStaff(id))
            {
                return Results.Forbid();
            }

            var existingUser = await userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return Results.NotFound(new { message = "User not found." });
            }

            existingUser.FirstName = updateUserModel.FirstName ?? existingUser.FirstName;
            existingUser.LastName = updateUserModel.LastName ?? existingUser.LastName;
            if (!string.IsNullOrWhiteSpace(updateUserModel.Email) && updateUserModel.Email != existingUser.Email)
            {
                // E-Mail ist gleichzeitig der Benutzername → beide (inkl. Normalisierung) setzen
                existingUser.Email = updateUserModel.Email;
                existingUser.UserName = updateUserModel.Email;
            }
            existingUser.PhoneNumber = updateUserModel.PhoneNumber;
            existingUser.Address = updateUserModel.Address;
            existingUser.City = updateUserModel.City;
            existingUser.ZipCode = updateUserModel.ZipCode;
            existingUser.Gender = updateUserModel.Gender;
            existingUser.Birthday = DateOnly.FromDateTime(updateUserModel.Birthday);

            // Benutzerinformationen speichern
            var updateResult = await userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
            {
                return Results.BadRequest(updateResult.Errors);
            }

            return Results.Ok(new { succeeded = true, message = "User details updated successfully." });
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        private static async Task<IResult> CreateUser(UserManager<AppUser> userManager, ClaimsPrincipal currentUser, [FromBody] UserRegistrationModel userRegistrationModel)
        {
            // Rolle festlegen: Öffentliche Registrierung ist immer "Customer".
            // Nur ein Admin darf Mitarbeiter oder weitere Admins anlegen.
            var role = string.IsNullOrWhiteSpace(userRegistrationModel.Role) ? Roles.Customer : userRegistrationModel.Role;
            if (!Roles.All.Contains(role))
            {
                return Results.BadRequest(new { message = "Invalid role." });
            }
            if (role != Roles.Customer && !currentUser.IsAdmin())
            {
                return Results.Forbid();
            }

            AppUser user = new AppUser()
            {
                UserName = userRegistrationModel.Email,
                Email = userRegistrationModel.Email,
                FirstName = userRegistrationModel.FirstName,
                LastName = userRegistrationModel.LastName,
                Birthday = DateOnly.FromDateTime(userRegistrationModel.Birthday),
                Address = userRegistrationModel.Address,
                City = userRegistrationModel.City,
                Gender = userRegistrationModel.Gender,
                ZipCode = userRegistrationModel.ZipCode,
                PhoneNumber = userRegistrationModel.PhoneNumber,
            };

            var result = await userManager.CreateAsync(user, userRegistrationModel.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result);
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return Results.BadRequest(new { message = "User created, but role assignment failed." });
            }

            return Results.Ok(new { message = "User created and role assigned successfully." });
        }


        [ProducesResponseType(typeof(AppUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        private static async Task<IResult> SignIn(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            [FromBody] LoginModel loginModel, IOptions<AppSettings> appSettings)
        {
            var invalid = Results.BadRequest(new { message = "Username or password is incorrect." });

            var user = await userManager.FindByEmailAsync(loginModel.Email ?? string.Empty);
            if (user == null)
            {
                return invalid;
            }

            // CheckPasswordSignInAsync zählt Fehlversuche und sperrt das Konto nach zu vielen Versuchen
            var check = await signInManager.CheckPasswordSignInAsync(user, loginModel.Password ?? string.Empty, lockoutOnFailure: true);
            if (check.IsLockedOut)
            {
                return Results.BadRequest(new { message = "Account locked. Please try again later." });
            }
            if (!check.Succeeded)
            {
                return invalid;
            }

            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            if (role == null)
            {
                return Results.BadRequest(new { message = "User has no role assigned." });
            }

            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Value.JWT_Secret));
            var claims = new ClaimsIdentity(new[]
            {
                new Claim("userID", user.Id),
                new Claim(ClaimTypes.Role, role),
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(appSettings.Value.TokenLifetimeHours),
                SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            return Results.Ok(new { token, role });
        }

        #endregion

        #region Record
        public record UserRegistrationModel
        (
             string FirstName,
             string LastName,
             string Email,
             string Password,
             string Role,
             string Gender,
             DateTime Birthday,
             string Address,
             string City,
             string ZipCode,
             string PhoneNumber
        );

        public record LoginModel
        (
           string Email,
           string Password

        );

        public record UpdateUserModel
        (
             string FirstName,
             string LastName,
             string Email,
             string Password,
             string Role,
             string Gender,
             DateTime Birthday,
             string Address,
             string City,
             string ZipCode,
             string PhoneNumber
            );

        public record ResetPasswordModel
        (
            string Email,
            string NewPassword
        );

        public record ChangePasswordModel
        (
            string CurrentPassword,
            string NewPassword
        );

        public record DeleteUserRequest
        (
            string Password
        );

        #endregion
    }


}
