using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Identity
{
    /// <summary>
    /// Клас за потребителски контекст
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        /// <summary>
        /// Идентификатор на потребителя
        /// </summary>
        public Guid UserId
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (value == null)
                {
                    return Guid.Empty;
                }

                return Guid.Parse(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Email
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

                if (value == null)
                {
                    return string.Empty;
                }

                return value;
            }
        }

        /// <summary>
        /// Пълно име на потребителя
        /// </summary>
        public string FullName
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

                if (value == null)
                {
                    return string.Empty;
                }

                return value;
            }
        }

        /// <summary>
        /// Име на потребителя
        /// </summary>
        public string FirstName
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimType.FirstName)?.Value;

                if (value == null)
                {
                    return string.Empty;
                }

                return value;
            }
        }

        /// <summary>
        /// Фамилия на потребителя
        /// </summary>
        public string LastName
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimType.LastName)?.Value;

                if (value == null)
                {
                    return string.Empty;
                }

                return value;
            }
        }

        /// <summary>
        /// Идентификатор на администрацията
        /// </summary>
        public Guid AdministrationId
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimType.AdministrationId)?.Value;

                if (value == null)
                {
                    return Guid.Empty;
                }

                return Guid.Parse(value);
            }
        }

        /// <summary>
        /// Проверка дали потребителят е аутентикиран
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated;

                return value ?? false;
            }
        }


        /// <summary>
        /// Администрации в които е дадения потребител
        /// </summary>
        public IList<AvailableAdministration> AvailableAdministrations 
        {
            get
            {
                var availableAdministrations = httpContextAccessor
                    .HttpContext?
                    .User
                    .FindAll(c => c.Type == CustomClaimType.AvailableAdministration)
                    .Select(c => new AvailableAdministration()
                    {
                        Id = c.Value.Split('!', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty,
                        Name = c.Value.Split('!', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty
                    });

                return availableAdministrations?.ToList() ?? new List<AvailableAdministration>();
            }
        }

        /// <summary>
        /// Дали текущия потребител е глобален администратор
        /// </summary>
        public bool IsGlobalAdmin
        {
            get
            {
                var value = httpContextAccessor.HttpContext?.User.IsInRole(UserRoles.GlobalAdmin);

                return value ?? false;
            }
        }
        // <summary>
        /// Дали текущия потребител има роля
        /// </summary>
        public bool IsInRole(string role)
        {
            return httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
        }

        public List<Guid> CoordinationRoles
        {
            get
            {
                var result = new List<Guid>();
                var identities = httpContextAccessor.HttpContext?.User.Identities;
                if (identities == null)
                {
                    return result;
                }
                foreach (var identity in identities!)
                {
                    if (identity != null)
                    {
                        foreach (var claim in identity.FindAll(identity.RoleClaimType))
                        {
                            if (Guid.TryParse(claim.Value, out var roleGuid))
                                result.Add(roleGuid);
                        }
                    }
                }
                return result;
            }
        }
    }
}
