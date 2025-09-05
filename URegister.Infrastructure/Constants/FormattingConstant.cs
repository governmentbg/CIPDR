namespace URegister.Infrastructure.Constants
{
    public static class FormattingConstant
    {
        /// <summary>
        /// Formatting of date dd.MM.yyyy - HH:mm:ss;
        /// </summary>
        public const string DateFormat = "dd.MM.yyyy - HH:mm:ss";

        /// <summary>
        /// Formatting of date dd.MM.yyyy;
        /// </summary>
        public const string NormalDateFormat = "dd.MM.yyyy";

        /// <summary>
        /// Formatting of date dd.MM.yyyy HH:mm;
        /// </summary>
        public const string DateTimeFormat = "dd.MM.yyyy HH:mm";

        /// <summary>
        /// Formatting of date yyyy-MM-ddTHH:mm:sszzz;
        /// </summary>
        public const string EFormDateFormat = "yyyy-MM-ddTHH:mm:sszzz";

        /// <summary>
        /// Formatting of date yyyy-MM-ddTHH:mm:ss.FFF'Z'
        /// </summary>
        public const string ISO8601DateFormat = "yyyy-MM-ddTHH:mm:ss.FFF'Z'";

        /// <summary>
        /// Formatting time to HH:mm
        /// </summary>
        public const string NormalTimeFormat = "HH:mm";

        /// <summary>
        /// Formatting decimal to #00.00
        /// </summary>
        public const string DecimalValueFormat = "#0.00";

        /// <summary>
        /// Formatting decimal to #00
        /// </summary>
        public const string IntValueFormat = "N";

    }
}
