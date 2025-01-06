using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class UserRoleUpdateRequest
    {
        public string UserId { get; set; }
        public List<RoleKeycloakBM> Roles { get; set; } = new List<RoleKeycloakBM>();
    }
}
