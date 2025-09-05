
namespace URegister.Core.Models.User
{
    public class UserRoleUpdateRequest
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string RegisterId { get; set; }
        public string RegisterCode { get; set; }
    }
}
