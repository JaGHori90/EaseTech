using System.Security.Claims;

namespace Api.Extentions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirst("userID")?.Value;

        public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);

        /// <summary>Admin oder Mitarbeiter.</summary>
        public static bool IsStaff(this ClaimsPrincipal user) =>
            user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Employee);

        /// <summary>Der Benutzer selbst oder ein Mitarbeiter/Admin.</summary>
        public static bool IsSelfOrStaff(this ClaimsPrincipal user, string userId) =>
            user.IsStaff() || (!string.IsNullOrEmpty(userId) && user.GetUserId() == userId);
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string Customer = "Customer";

        public static readonly string[] All = [Admin, Employee, Customer];
    }
}
