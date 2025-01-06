using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Constants
{
    /// <summary>
    /// Константи за връзка с identity provider keycloak
    /// </summary>
    public static class KeyCloakConstants
    {
        /// <summary>
        /// Взимане на token. Подава се realm
        /// </summary>
        public static string Token = "realms/{0}/protocol/openid-connect/token";

        /// <summary>
        /// Връща всички потребители. Подава се realm
        /// </summary>
        public static string GetUsers = "admin/realms/{0}/users";

        /// <summary>
        /// Връща всички групи/регистри. Подава се realm
        /// </summary>
        public static string GetGroups = "admin/realms/{0}/groups";

        /// <summary>
        /// Връща всички създадени роли. Подава се realm и keycloakClientUUID 
        /// </summary>
        public static string GetRoles = "admin/realms/{0}/clients/{1}/roles";

        /// <summary>
        /// Връща всички роли на потребител. Подава се realm и userId 
        /// </summary>
        public static string GetUserRoles = "admin/realms/{0}/users/{1}/role-mappings";

        /// <summary>
        ///  Връща всички свободни роли на потребител. Подава се realm и userId 
        /// </summary>
        public static string GetАvailableUserRoles = "admin/realms/{0}/users/{1}/role-mappings/realm/available";

        /// <summary>
        /// Добавя/премахване роля/и към потребител Подава се realm и userId 
        /// </summary>
        public static string UpdateUserRoles = "admin/realms/{0}/users/{1}/role-mappings/realm";

        /// <summary>
        /// Добавя/премахване група/и към потребител Подава се realm, userId, groupId
        /// </summary>
        public static string UpdateUserGroups = "admin/realms/{0}/users/{1}/groups/{2}";
    }
}
