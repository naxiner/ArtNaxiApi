using ArtNaxiApi.Constants;

namespace ArtNaxiApi.Validation
{
    public static class RoleValidator
    {
        private static readonly HashSet<string> ValidRoles = new HashSet<string>
        {
            Roles.Admin,
            Roles.Moderator,
            Roles.User
        };

        public static bool IsValidRole(string role)
        {
            return ValidRoles.Contains(role);
        }
    }
}
