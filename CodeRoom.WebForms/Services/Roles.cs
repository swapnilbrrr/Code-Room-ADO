namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// Central definition of the platform roles used for authorization.
    /// Mirrors the values already stored in the Users.Role column of the source application.
    /// </summary>
    public static class Roles
    {
        public const string Student = "Student";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>Roles allowed into the /Admin section.</summary>
        public const string AdminArea = Admin + "," + SuperAdmin;
    }
}
