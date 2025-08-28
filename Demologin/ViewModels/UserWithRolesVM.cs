namespace Demologin.ViewModels
{
    public class UserWithRolesVM
    {
        public string UserId { get; set; }
        public string? Email { get; set; }

        public string? UserName { get; set; }
        public IList<string> Roles { get; set; }   // ✅ Change here
    }

}
