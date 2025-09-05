namespace URegister.Infrastructure.Constants
{
    public class RegexPatterns
    {
        /// <summary>
        /// Regex шаблон за български формат на парични суми
        /// </summary>
        public const string MoneyPattern = @"^\d+(\,\d{1,2})?$";

        /// <summary>
        /// Regex шаблон за текст на кирилица
        /// </summary>
        //public const string CyrillicTextPattern = @"^[\p{IsCyrillic}\p{P}\s\p{Nd}№]+$";
        public const string CyrillicTextPattern = @"^(?!\s)[А-я0-9'"" !?.,@#$%^&()§¶№-]+(?<!\s)$";

        /// <summary>
        /// Regex шаблон за текст на латиница
        /// </summary>
        public const string LatinTextWithNumbersPattern = @"^(?!\s)[()A-Za-z0-9]+(?<!\s)$";

        /// <summary>
        /// Regex шаблон за име на лице на кирилица
        /// </summary>
        public const string CyrillicPersonNamePattern = @"^[А-я]([А-я' -]*[А-я])?$";

        /// <summary>
        /// Regex шаблон за име на лица на кирилица, разделени с запетая и интервал
        /// </summary>
        //public const string CyrillicPersonNamesCommaSeparatedPattern = @"^[\p{IsCyrillic}\p{P}\s]+([\p{IsCyrillic}\p{P}\s]+, )*$";
        public const string CyrillicPersonNamesCommaSeparatedPattern = @"^(?!\s)[А-я\p{P}\s]+([А-я\p{P}\s]+, )*(?<!\s)$";
       
        /// <summary>
        /// Regex шаблон за пощенски код
        /// </summary>
        public const string PostCode = @"^\d{4}$";

        /// <summary>
        /// Regex шаблон за низ от цифри
        /// </summary>
        public const string Digits = @"^\d+$";

        /// <summary>
        /// Regex шаблон за телефонен номер от цифри
        /// </summary>
        public const string PhoneNumber = @"^(?=.{6,40}$)(\+([1-9]\d+)|0[1-9]\d+)$";
                
        /// /// <summary>
        /// Regex шаблон за електронна поща
        /// </summary>
        public const string Email = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

        /// <summary>
        /// Regex шаблон за ЕКАТТЕ код
        /// </summary>
        public const string EkatteCode = @"^\d{5}$";

        /// <summary>
        /// Regex шаблон за URL
        /// </summary>
        public const string URL = @"\b((https?|ftp):\/\/)?((([a-zA-Z0-9\u00a1-\uffff\-_]+\.)+[a-zA-Z\u00a1-\uffff]{2,})|localhost)(:\d{1,5})?(\/[^\s]*)?\b";

        /// <summary>
        /// Regex номер на регистър
        /// </summary>
        public const string RegisterNumber = @"^R(\d){5,9}$";

        /// <summary>
        /// Regex шаблон за номер на лична карта
        /// </summary>
        public const string IDCardNumberPattern = @"^([0-9]{9}|[A-Z]{2}[0-9]{7})$";
    }
}
