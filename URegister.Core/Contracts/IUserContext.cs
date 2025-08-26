using URegister.Core.Identity;

namespace URegister.Core.Contracts
{
    /// <summary>
    /// Интерфейс за потребителски контекст
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Идентификатор на потребителя
        /// </summary>
        Guid UserId { get; }

        /// <summary>
        /// Имейл на потребителя
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Пълно име на потребителя
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Име на потребителя
        /// </summary>
        string FirstName { get; }

        /// <summary>
        /// Фамилия на потребителя
        /// </summary>
        string LastName { get; }

        /// <summary>
        /// Идентификатор на администрацията
        /// </summary>
        Guid AdministrationId { get; }

        /// <summary>
        /// Проверка дали потребителят е аутентикиран
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Администрации в които е дадения потребител
        /// </summary>
        IList<AvailableAdministration> AvailableAdministrations { get; }

        /// <summary>
        /// Дали текущия потребител е глобален администратор
        /// </summary>
        public bool IsGlobalAdmin { get; }
        List<Guid> CoordinationRoles { get; }

        bool IsInRole(string role);
    }
}
