using System.ComponentModel;

namespace URegister.Infrastructure.Constants
{
    public static class UserRoles
    {
        /// <summary>
        /// Регистратор
        /// </summary>
        [Description("Регистратор")]
        public const string Registrator = "Registrator";

        /// <summary>
        /// Администратор МЕУ
        /// </summary>
        [Description("Администратор МЕУ")]
        public const string GlobalAdmin = "GlobalAdmin";

        /// <summary>
        /// АО – обработващ
        /// </summary>
        [Description("АО – обработващ")]
        public const string Editor = "Editor";

        /// <summary>
        /// АО – заявител/длъжностно лице
        /// </summary>
        [Description("АО – заявител/длъжностно лице")]
        public const string Manager = "Manager";

        /// <summary>
        /// АО – оторизиран служител
        /// </summary>
        [Description("АО – оторизиран служител")]
        public const string Admin = "Admin";
    }
}
