using URegister.Core.Models.Register;
using URegister.Users;


namespace URegister.Core.Models.User
{
    public class RolesRegistriesVM
    {
        public List<RoleData> Roles { get; set; } = new List<RoleData>();
        public List<RegisterVM> Registries { get; set; } = new List<RegisterVM>();
    }
}
